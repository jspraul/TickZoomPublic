#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using TickZoom.Api;

namespace TickZoom.FIX
{
	public class FIXServerSymbolHandler : IDisposable {
		private static Log log = Factory.SysLog.GetLogger(typeof(FIXServerSymbolHandler));
		private static bool trace = log.IsTraceEnabled;
		private static bool debug = log.IsDebugEnabled;
		private FillSimulator fillSimulator;
		private TickReader reader;
		private Func<SymbolInfo,Tick,Yield> onTick;
		private Task queueTask;
		private TickSync tickSync;
		private SymbolInfo symbol;
		private TickIO nextTick = Factory.TickUtil.TickIO();
		
		public FIXServerSymbolHandler( string symbolString, Func<SymbolInfo,Tick,Yield> onTick, Action<PhysicalFill> onPhysicalFill) {
			this.onTick = onTick;
			this.symbol = Factory.Symbol.LookupSymbol(symbolString);
			reader = Factory.TickUtil.TickReader();
			reader.Initialize("Test\\MockProviderData", symbolString);
			fillSimulator = Factory.Utility.FillSimulator( symbol);
			fillSimulator.OnPhysicalFill = onPhysicalFill;
			tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
			tickSync.ForceClear();
			queueTask = Factory.Parallel.Loop("FIXServerSymbol-"+symbolString, OnException, ProcessQueue);
		}
		
	    private void UnLockTickSync() {
	    	if( trace) log.Trace("Unlocking TickSync.");
	    	tickSync.Clear();
	    }
	    
	    private void TryCompleteTick() {
	    	if( tickSync.Completed) {
		    	if( trace) log.Trace("TryCompleteTick()");
		    	UnLockTickSync();
	    	}
	    }
		
		public int ActualPosition {
			get {
				return (int) fillSimulator.ActualPosition;
			}
		}
		
		public void CreateOrder(PhysicalOrder order) {
			fillSimulator.OnCreateBrokerOrder( order);
		}
		
		public void ChangeOrder(PhysicalOrder order, object origBrokerOrder) {
			fillSimulator.OnChangeBrokerOrder( order, origBrokerOrder);
		}
		
		public void CancelOrder(object origBrokerOrder) {
			fillSimulator.OnCancelBrokerOrder( origBrokerOrder);
		}
		
		public PhysicalOrder GetOrderById(string clientOrderId) {
			return fillSimulator.GetOrderById( clientOrderId);
		}
		
		public void ReprocessOrders() {
			fillSimulator.ReprocessOrders();
		}
	    
		private Yield ProcessQueue() {
			if( SyncTicks.Enabled && !tickSync.TryLock()) {
				TryCompleteTick();
				return Yield.NoWork.Repeat;
			}
			if( trace) log.Trace("Locked tickSync for " + symbol);
			return Yield.DidWork.Invoke(DequeueTick);
		}
		
		private Yield DequeueTick() {
			var result = Yield.NoWork.Repeat;
			var binary = new TickBinary();
			try { 
				if( reader.ReadQueue.TryDequeue( ref binary)) {
				   	nextTick.Inject( binary);
				   	tickSync.AddTick();
				   	fillSimulator.ProcessOrders( nextTick);
				   	result = Yield.DidWork.Invoke(ProcessTick);
				}
			} catch( QueueException ex) {
				if( ex.EntryType != EventType.EndHistorical) {
					throw;
				}
			}
			return result;
		}
		
		private Yield ProcessTick() {
			return onTick( symbol, nextTick);
		}
		
		private void OnException( Exception ex) {
			// Attempt to propagate the exception.
			log.Error("Exception occurred", ex);
			Dispose();
		}
		
	 	protected volatile bool isDisposed = false;
	    public void Dispose() 
	    {
	        Dispose(true);
	        GC.SuppressFinalize(this);      
	    }
	
	    protected virtual void Dispose(bool disposing)
	    {
	       		if( !isDisposed) {
	            isDisposed = true;   
	            if (disposing) {
	            	if( debug) log.Debug("Dispose()");
	            	if( reader != null) {
	            		reader.Dispose();
	            	}
	            	if( queueTask != null) {
	            		queueTask.Stop();
	            	}
	            }
    		}
	    }    
	        
	}
}