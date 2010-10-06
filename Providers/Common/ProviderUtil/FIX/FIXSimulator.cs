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
using System.Collections.Generic;
using TickZoom.Api;

namespace TickZoom.FIX
{
	public class FIXSimulator : IDisposable {
		private string localAddress = "0.0.0.0";
		private static Log log = Factory.SysLog.GetLogger(typeof(FIXSimulator));
		private static bool trace = log.IsTraceEnabled;
		private static bool debug = log.IsDebugEnabled;
		private SimpleLock symbolHandlersLocker = new SimpleLock();
		
		// FIX fields.
		private ushort fixPort = 0;
		private Selector fixSelector;
		protected Socket fixSocket;
		private Packet fixReadPacket;
		protected Packet fixWritePacket;
		private YieldMethod WriteToFixMethod;
		private Task fixTask;
		private bool isFIXSimulationStarted = false;
		private PacketFactory fixPacketFactory;
		
		// Quote fields.
		private ushort quotesPort = 0;
		private Selector quoteSelector;
		protected Socket quoteSocket;
		private Packet quoteReadPacket;
		protected Packet quoteWritePacket;
		private YieldMethod WriteToQuotesMethod;
		private Task quoteTask;
		private bool isQuoteSimulationStarted = false;
		private PacketFactory quotePacketFactory;		
		
		private Dictionary<long,FIXServerSymbolHandler> symbolHandlers = new Dictionary<long,FIXServerSymbolHandler>();

		public FIXSimulator(ushort fixPort, ushort quotesPort, PacketFactory fixPacketFactory, PacketFactory quotePacketFactory) {
			WriteToFixMethod = WriteToFIX;
			WriteToQuotesMethod = WriteToQuotes;
			this.fixPacketFactory = fixPacketFactory;
			this.quotePacketFactory = quotePacketFactory;
			ListenToFIX(fixPort);
			ListenToQuotes(quotesPort);
		}
		
		private void ListenToFIX(ushort fixPort) {
			this.fixPort = fixPort;
			fixSelector = Factory.Provider.Selector(localAddress, fixPort, 0, OnException);
			fixSelector.OnConnect = OnConnectFIX;
			fixSelector.OnDisconnect = OnDisconnectFIX;
			fixSelector.Start();
			fixPort = fixSelector.ListenPort;
			log.Info("Listening to " + localAddress + " on port " + fixPort);
		}
		
		private void ListenToQuotes(ushort quotesPort) {
			this.quotesPort = quotesPort;
			quoteSelector = Factory.Provider.Selector(localAddress, quotesPort, 0, OnException);
			quoteSelector.OnConnect = OnConnectQuotes;
			quoteSelector.OnDisconnect = OnDisconnectQuotes;
			quoteSelector.Start();
			quotesPort = quoteSelector.ListenPort;
			log.Info("Listening to " + localAddress + " on port " + quotesPort);
		}
		
		protected virtual void OnConnectFIX( Socket socket) {
			fixSocket = socket;
			fixSocket.PacketFactory = fixPacketFactory;
			fixSelector.AddReader(socket);
			fixSelector.AddWriter(socket);
			log.Info("Received FIX connection: " + socket);
			StartFIXSimulation();
			fixTask = Factory.Parallel.Loop( "FIXServerReader", OnException, FIXReadLoop);
		}
		
		protected virtual void OnConnectQuotes( Socket socket) {
			quoteSocket = socket;
			quoteSocket.PacketFactory = quotePacketFactory;
			quoteSelector.AddReader(socket);
			quoteSelector.AddWriter(socket);
			log.Info("Received quotes connection: " + socket);
			StartQuoteSimulation();
			quoteTask = Factory.Parallel.Loop( "QuotesServerReader", OnException, QuotesReadLoop);
		}
		
		private void OnDisconnectFIX( Socket socket) {
			if( this.fixSocket == socket ) {
				log.Info("FIX socket disconnect: " + socket);
				CloseSockets();
			}
		}
		
		private void OnDisconnectQuotes( Socket socket) {
			if( this.quoteSocket == socket ) {
				log.Info("Quotes socket disconnect: " + socket);
				CloseSockets();
			}
		}
		
		protected virtual void CloseSockets() {
        	if( symbolHandlers != null) {
				if( symbolHandlersLocker.TryLock()) {
					try {
		            	foreach( var kvp in symbolHandlers) {
		            		var handler = kvp.Value;
		            		handler.Dispose();
		            	}
		            	symbolHandlers.Clear();
					} finally {
						symbolHandlersLocker.Unlock();
					}
				}
        	}
			if( fixTask != null) fixTask.Stop();
			if( fixSocket != null) fixSocket.Dispose();
			if( quoteTask != null) quoteTask.Stop();
			if( quoteSocket != null) quoteSocket.Dispose();
		}
		
		public virtual void StartFIXSimulation() {
			isFIXSimulationStarted = true;
		}
		
		public virtual void StartQuoteSimulation() {
			isQuoteSimulationStarted = true;
		}
		
		private Yield FIXReadLoop() {
			var result = Yield.NoWork.Repeat;
			if( isFIXSimulationStarted) {
				if( fixSocket.TryGetPacket(out fixReadPacket)) {
					if( trace) log.Trace("Local Read: " + fixReadPacket);
					result = ParseFIXMessage(fixReadPacket);
				}
			}
			return result;
		}
		
		public void AddSymbol( string symbol, Func<SymbolInfo,Tick,Yield> onTick, Action<PhysicalFill> onPhysicalFill) {
			var symbolInfo = Factory.Symbol.LookupSymbol(symbol);
			if( !symbolHandlers.ContainsKey( symbolInfo.BinaryIdentifier)) {
				var symbolHandler = new FIXServerSymbolHandler(symbol,onTick,onPhysicalFill);
				symbolHandlers.Add(symbolInfo.BinaryIdentifier,symbolHandler);
			}
		}
		
		public int GetPosition( SymbolInfo symbol) {
			var symbolHandler = symbolHandlers[symbol.BinaryIdentifier];
			return symbolHandler.ActualPosition;
		}
		
		public void AddOrder( PhysicalOrder order) {
			var symbolHandler = symbolHandlers[order.Symbol.BinaryIdentifier];
			symbolHandler.AddOrder( order);
		}
		
		public void ProcessOrders(SymbolInfo symbol) {
			var symbolHandler = symbolHandlers[symbol.BinaryIdentifier];
			symbolHandler.ProcessOrders();
		}
		
		private Yield QuotesReadLoop() {
			var result = Yield.NoWork.Repeat;
			if( isQuoteSimulationStarted) {
				if( quoteSocket.TryGetPacket(out quoteReadPacket)) {
					if( trace) log.Trace("Local Read: " + quoteReadPacket);
					result = ParseQuotesMessage(quoteReadPacket);
				}
			}
			return result;
		}
		
		public virtual Yield ParseFIXMessage(Packet packet) {
			if( debug) log.Debug("Received FIX message: " + packet);
			return Yield.DidWork.Repeat;
		}
	
		public virtual Yield ParseQuotesMessage(Packet packet) {
			if( debug) log.Debug("Received Quotes message: " + packet);
			return Yield.DidWork.Repeat;
		}
	
		public Yield WriteToFIX() {
			if( fixSocket.TrySendPacket(fixWritePacket)) {
				if(trace) log.Trace("Local Write: " + fixWritePacket);
				return Yield.DidWork.Return;
			} else {
				return Yield.NoWork.Repeat;
			}
		}
	
		public Yield WriteToQuotes() {
			if( quoteSocket.TrySendPacket(quoteWritePacket)) {
				if(trace) log.Trace("Local Write: " + quoteWritePacket);
				return Yield.DidWork.Return;
			} else {
				return Yield.NoWork.Repeat;
			}
		}
		
		public void OnException( Exception ex) {
			log.Error("Exception occurred", ex);
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
	            	if( symbolHandlers != null) {
						if( symbolHandlersLocker.TryLock()) {
	            			try {
				            	foreach( var kvp in symbolHandlers) {
				            		var handler = kvp.Value;
				            		handler.Dispose();
				            	}
				            	symbolHandlers.Clear();
	            			} finally {
	            				symbolHandlersLocker.Unlock();
	            			}
	            		}
	            	}
	            	if( fixTask != null) {
	            		fixTask.Stop();
	            	}
	            	if( fixSelector != null) {
	            		fixSelector.Dispose();
	            	}
	            	if( fixSocket != null) {
		            	fixSocket.Dispose();
	            	}
	            	if( quoteTask != null) {
	            		quoteTask.Stop();
	            	}
	            	if( quoteSelector != null) {
	            		quoteSelector.Dispose();
	            	}
	            	if( quoteSocket != null) {
		            	quoteSocket.Dispose();
	            	}
	            }
    		}
	    }    
	        
		public ushort FIXPort {
			get { return fixPort; }
		}
		
		public ushort QuotesPort {
			get { return quotesPort; }
		}
	}
}