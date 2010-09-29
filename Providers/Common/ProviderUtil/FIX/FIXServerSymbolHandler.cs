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
		private TickReader reader;
		private Func<SymbolInfo,Tick,Yield> onTick;
		private Task queueTask;
		private SymbolInfo symbol;
		
		public FIXServerSymbolHandler( string symbol, Func<SymbolInfo,Tick,Yield> onTick) {
			this.onTick = onTick;
			this.symbol = Factory.Symbol.LookupSymbol(symbol);
			reader = Factory.TickUtil.TickReader();
			reader.Initialize("MockProviderData", symbol);
			queueTask = Factory.Parallel.Loop("FIXServerSymbol-"+symbol, OnException, CheckQueue);
		}
		
		public Yield CheckQueue() {
			var result = Yield.DidWork.Repeat;
			var binary = new TickBinary();
			TickIO tickIO = Factory.TickUtil.TickIO();
			try { 
				if( reader.ReadQueue.TryDequeue( ref binary)) {
				   	tickIO.Inject( binary);
				   	result = onTick( symbol, tickIO);
				}
			} catch( QueueException ex) {
				if( ex.EntryType == EventType.EndHistorical) {
					result = Yield.Terminate;
				} else {
					throw;
				}
			}
			return result;
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
