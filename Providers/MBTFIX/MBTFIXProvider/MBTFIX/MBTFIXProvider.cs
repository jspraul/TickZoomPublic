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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.MBTFIX
{
	public class MBTFIXProvider : FIXProviderSupport, PhysicalOrderHandler
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(MBTFIXProvider));
		private static readonly bool info = log.IsDebugEnabled;
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		private static long nextConnectTime = 0L;
		private readonly object orderHandlerLocker = new object();
        private Dictionary<long,LogicalOrderHandler> orderHandlers = new Dictionary<long,LogicalOrderHandler>();
		private Dictionary<string,PhysicalOrder> openOrders = new Dictionary<string,PhysicalOrder>();
		
		public MBTFIXProvider()
		{
  			ProviderName = "MBTFIXProvider";
  			HeartbeatDelay = 35;
		}
		
		public override void OnDisconnect() {
			SendEndBroker();
		}

		public override void OnRetry() {
		}
		
		private void SendStartBroker() {
			lock( symbolsRequestedLocker) {
				foreach( var kvp in symbolsRequested) {
					SymbolInfo symbol = kvp.Value;
					long end = Factory.Parallel.TickCount + 2000;
					while( !receiver.OnEvent(symbol,(int)EventType.StartBroker,symbol)) {
						if( isDisposed) return;
						if( Factory.Parallel.TickCount > end) {
							throw new ApplicationException("Timeout while sending start broker.");
						}
						Factory.Parallel.Yield();
					}
				}
			}
		}
		
		private void SendEndBroker() {
			lock( symbolsRequestedLocker) {
				foreach(var kvp in symbolsRequested) {
					SymbolInfo symbol = kvp.Value;
					long end = Factory.Parallel.TickCount + 2000;
					while( !receiver.OnEvent(symbol,(int)EventType.EndBroker,symbol)) {
						if( isDisposed) return;
						if( Factory.Parallel.TickCount > end) {
							throw new ApplicationException("Timeout while sending end broker.");
						}
						Factory.Parallel.Yield();
					}
				}
			}
		}
		
		long lastLoginTry = long.MinValue;
		long loginRetryTime = 10000; //milliseconds = 10 seconds.
		
		public override Yield OnLogin() {
			if( debug) log.Debug("Login()");
			
			if( lastLoginTry + loginRetryTime > Factory.Parallel.TickCount) {
				return Yield.NoWork.Repeat;
			}
			
			lastLoginTry = Factory.Parallel.TickCount;
			
			Packet packet = Socket.CreatePacket();
			
			var mbtMsg = new FIXMessage4_4(UserName);
			mbtMsg.SetEncryption(0);
			mbtMsg.SetHeartBeatInterval(30);
			mbtMsg.ResetSequence();
			mbtMsg.SetEncoding("554_H1");
			mbtMsg.SetPassword(Password);
			mbtMsg.AddHeader("A");
			
			string login = mbtMsg.ToString();
			packet.DataOut.Write(login.ToCharArray());
			string packetString = packet.ToString();
			if( debug) {
				log.Debug("Login message: \n" + packetString);
			}
			long end = Factory.Parallel.TickCount + 2000;
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return Yield.NoWork.Repeat;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending login message.");
				}
				Factory.Parallel.Yield();
			}
			
			end = Factory.Parallel.TickCount + 15 * 1000;
			while( !Socket.TryGetPacket(out packet)) {
				if( IsInterrupted) return Yield.NoWork.Repeat;
				Factory.Parallel.Yield();
				if( Factory.Parallel.TickCount > end) {
					FailLogin(packetString);
				}
			}
			
			if( !VerifyLogin(packet)) {
				return Yield.NoWork.Repeat;
			}
			
			StartRecovery();
			
            return Yield.DidWork.Repeat;
        }
		
		protected override void OnStartRecovery()
		{
			isPositionUpdateComplete = false;
			isOrderUpdateComplete = false;
			if( !LogRecovery) {
				PacketFIXT1_1.IsQuietRecovery = true;
			}

			RequestOrders();
			
			RequestPositions();
		}
		
		public override void OnStartSymbol(SymbolInfo symbol)
		{
        	if( IsRecovered) {
				long end = Factory.Parallel.TickCount + 2000;
        		while( !receiver.OnEvent(symbol,(int)EventType.StartBroker,symbol)) {
        			if( IsInterrupted) return;
					if( Factory.Parallel.TickCount > end) {
						throw new ApplicationException("Timeout while sending start broker.");
					}
        			Factory.Parallel.Yield();
        		}
        	}
		}

		public override void OnStopSymbol(SymbolInfo symbol)
		{
			long end = Factory.Parallel.TickCount + 2000;
			while( !receiver.OnEvent(symbol,(int)EventType.EndBroker,symbol)) {
				if( IsInterrupted) return;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending stop broker.");
				}
				Factory.Parallel.Yield();
			}
		}
		

		private void RequestPositions() {
			Packet packet = Socket.CreatePacket();
			
			var fixMsg = new FIXMessage4_4(UserName);
			fixMsg.SetSubscriptionRequestType(1);
			fixMsg.SetAccount(AccountNumber);
			fixMsg.SetPositionRequestId(1);
			fixMsg.SetPositionRequestType(0);
			fixMsg.AddHeader("AN");
			string login = fixMsg.ToString();
			if( debug) {
				string view = login.Replace(FIXTBuffer.EndFieldStr,"  ");
				log.Info("Request Positions message: \n" + view);
			}
			packet.DataOut.Write(login.ToCharArray());
			long end = Factory.Parallel.TickCount + 2000;
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending request positions.");
				}
				Factory.Parallel.Yield();
			}
		}

		private void RequestOrders() {
			Packet packet = Socket.CreatePacket();
			var fixMsg = new FIXMessage4_4(UserName);
			fixMsg.SetAccount(AccountNumber);
			fixMsg.SetMassStatusRequestID(TimeStamp.UtcNow);
			fixMsg.SetMassStatusRequestType(90);
			fixMsg.AddHeader("AF");
			string login = fixMsg.ToString();
			if( debug) {
				string view = login.Replace(FIXTBuffer.EndFieldStr,"  ");
				log.Debug("Request Orders message: \n" + view);
			}
			packet.DataOut.Write(login.ToCharArray());
			long end = Factory.Parallel.TickCount + 2000;
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending request orders.");
				}
				Factory.Parallel.Yield();
			}
		}
		
		private void SendHeartbeat() {
			Packet packet = Socket.CreatePacket();
			var fixMsg = new FIXMessage4_4(UserName);
			fixMsg.AddHeader("0");
			string login = fixMsg.ToString();
			if( trace) {
				string view = login.Replace(FIXTBuffer.EndFieldStr,"  ");
				log.Trace("Send heartbeat message: \n" + view);
			}
			packet.DataOut.Write(login.ToCharArray());
			long end = Factory.Parallel.TickCount + 2000;
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending heartbeat.");
				}
				Factory.Parallel.Yield();
			}
		}
		
		private unsafe bool VerifyLogin(Packet packet) {
			PacketFIX4_4 packetFIX = (PacketFIX4_4) packet;
			if( !("A" == packetFIX.MessageType &&
				"FIX.4.4" == packetFIX.Version &&
				"MBT" == packetFIX.Sender && 
				UserName == packetFIX.Target && 
				"0" == packetFIX.Encryption && 
				30 == packetFIX.HeartBeatInterval) ) {
				StringBuilder message = new StringBuilder();
				message.AppendLine("Invalid login response:");
				message.AppendLine("  message type = " + packetFIX.MessageType);
				message.AppendLine("  version = " + packetFIX.Version);
				message.AppendLine("  sender = " + packetFIX.Sender);
				message.AppendLine("  target = " + packetFIX.Target);
				message.AppendLine("  encryption = " + packetFIX.Encryption);
				message.AppendLine("  heartbeat interval = " + packetFIX.HeartBeatInterval);
				message.AppendLine(packetFIX.ToString());
				log.Error(message);
				throw new ApplicationException(message.ToString());
			}
			return 1 == packetFIX.Sequence;
		}
		
		protected override Yield ReceiveMessage() {
			Packet packet;
			if(Socket.TryGetPacket(out packet)) {
				PacketFIX4_4 packetFIX = (PacketFIX4_4) packet;
				switch( packetFIX.MessageType) {
					case "AP":
						PositionUpdate( packetFIX);
						break;
					case "AO":
						PositionUpdate( packetFIX);
						break;
					case "8":
					case "9":
						ExecutionReport( packetFIX);
						break;
					case "1":
						SendHeartbeat();
						break;
					case "0":
						// Received heartbeat
						break;
					case "j":
						BusinessReject( packetFIX);
						break;
					default:
						log.Error("Unknown packet: '" + packetFIX.MessageType + "'\n" + packetFIX);
						break;
				}
				return Yield.DidWork.Repeat;
			} else {
				return Yield.NoWork.Repeat;
			}
		}
		
		private void BusinessReject(PacketFIX4_4 packetFIX) {
			string message = "FIX Server reported an error: " + packetFIX.Text + "\n" + packetFIX;
			string text = packetFIX.Text.ToLower();
			if( text.Contains("order") && text.Contains("server")) {
				log.Warn( packetFIX.Text + " -- Sending EndBroker event.");
				SendEndBroker();
			} else {
				throw new ApplicationException( packetFIX.Text + "\n" + message);
			}
		}
		
		private bool isPositionUpdateComplete = false;
		private bool isOrderUpdateComplete = false;
		private void TryEndRecovery() {
			if( isPositionUpdateComplete && isOrderUpdateComplete) {
				isPositionUpdateComplete = false;
				isOrderUpdateComplete = false;
				if( !TryCancelPendingOrders() ) {
					ReportRecovery();
				}
			}
		}
		
		private bool isCancelingPendingOrders = false;
		
		private bool TryCancelPendingOrders() {
			var pending = new List<PhysicalOrder>();
			foreach( var kvp in openOrders) {
				PhysicalOrder order = kvp.Value;
				if( !order.IsActive && "ReplaceRejected".Equals(order.Tag)) {
					pending.Add(order);
				}
			}
			if( pending.Count == 0) {
				isCancelingPendingOrders = false;
				return false;
			} else if( !isCancelingPendingOrders) {
				isCancelingPendingOrders = true;
				log.Info("Recovery completed with pending orders. Canceling them now..");
				foreach( var order in pending) {
					log.Info("Canceling Pending Order: " + order);
					OnCancelBrokerOrder(order);
				}
			}
			return isCancelingPendingOrders;
		}

		private void ReportRecovery() {
			StringBuilder sb = new StringBuilder();
			foreach( var kvp in openOrders) {
				PhysicalOrder order = kvp.Value;
				sb.Append( "    ");
				sb.Append( (string) order.BrokerOrder);
				sb.Append( " ");
				sb.Append( order);
				sb.AppendLine();
			}
			log.Info("Recovered Open Orders:\n" + sb);
			SendStartBroker();
			PacketFIXT1_1.IsQuietRecovery = false;
			EndRecovery();
		}
		
		private void PositionUpdate( PacketFIX4_4 packetFIX) {
			if( packetFIX.MessageType == "AO") {
				isPositionUpdateComplete = true;
				if(debug) log.Debug("PositionUpdate Complete.");
				TryEndRecovery();
			} else {
				double position = packetFIX.LongQuantity + packetFIX.ShortQuantity;
				SymbolInfo symbolInfo;
				try {
					symbolInfo = Factory.Symbol.LookupSymbol(packetFIX.Symbol);
				} catch( ApplicationException ex) {
					log.Error("Error looking up " + packetFIX.Symbol + ": " + ex.Message);
					return;
				}
				log.Info("PositionUpdate: " + symbolInfo + "=" + position);
				LogicalOrderHandler orderHandler = GetOrderHandler(symbolInfo.BinaryIdentifier);
				orderHandler.SetActualPosition( position);
			}
		}
		
		private void ExecutionReport( PacketFIX4_4 packetFIX) {
			if( packetFIX.ClientOrderId == "4.3488742031145") {
				log.Info( "WATCH: " + packetFIX);
			}
			if( packetFIX.Text == "END") {
				isOrderUpdateComplete = true;
				if(debug) log.Debug("ExecutionReport Complete.");
				TryEndRecovery();
			} else {
				if( debug && (LogRecovery || !IsRecovery) ) {
					log.Debug("ExecutionReport: " + packetFIX);
				}
				string orderStatus = packetFIX.OrderStatus;
				switch( orderStatus) {
					case "0": // New
						UpdateOrder( packetFIX, true, null);
						break;
					case "1": // Partial
						UpdatePartial( packetFIX);
						if( IsRecovered) {
							SendFill( packetFIX);
						}
						break;
					case "2":  // Filled 
						RemoveOrder( packetFIX, packetFIX.ClientOrderId);
						if( IsRecovered) {
							SendFill( packetFIX);
						}
						break;
					case "4": // Canceled
						RemoveOrder( packetFIX, packetFIX.ClientOrderId);
						break;
					case "5": // Replaced
						UpdateOrder( packetFIX, true, null);
						RemoveOrder( packetFIX, packetFIX.OriginalClientOrderId);
						break;
					case "6": // Pending Cancel
						RemoveOrder( packetFIX, packetFIX.OriginalClientOrderId);
						break;
					case "8": // Rejected
						RejectOrder( packetFIX);
						break;
					case "9": // Suspended
						UpdateOrder( packetFIX, false, packetFIX);
						// Ignore 
						break;
					case "A": // Accepted
						UpdateOrder( packetFIX, true, null);
						break;
					case "E": // Pending Replace
						UpdateOrder( packetFIX, false, "PendingReplace");
						break;
					case "R": // Resumed.
						UpdateOrder( packetFIX, true, null);
						// Ignore
						break;
					default:
						throw new ApplicationException("Unknown order status: '" + orderStatus + "'");
				}
			}
		}
		
		public void SendFill( PacketFIX4_4 packetFIX) {
			string clientOrderId = packetFIX.ClientOrderId;
			if( debug ) log.Debug("SendFill( " + clientOrderId + ")");
			SymbolInfo symbolInfo = Factory.Symbol.LookupSymbol(packetFIX.Symbol);
			if( GetSymbolStatus(symbolInfo)) {
				string[] parts = clientOrderId.Split(DOT_SEPARATOR);
				int logicalOrderId = 0;
				try {
					logicalOrderId = int.Parse(parts[0]);
				} catch( FormatException) {
					log.Warn("Fill received from order " + clientOrderId + " created externally. So it lacks any logical order id. That means a fill cannot be sent to the strategy. This will get resolved at next synchronization.");
					return;
				}
				TimeStamp executionTime = new TimeStamp(packetFIX.TransactionTime);
				LogicalOrderHandler orderHandler = GetOrderHandler(symbolInfo.BinaryIdentifier);
				LogicalFillBinary binary = new LogicalFillBinary(orderHandler.ActualPosition,packetFIX.AveragePrice,executionTime,logicalOrderId);
				if( debug) log.Debug( "Sending logical fill: " + binary);
	            receiver.OnEvent(symbolInfo,(int)EventType.LogicalFill,binary);
            	openOrders.Remove(packetFIX.ClientOrderId);
	            ProcessFill( symbolInfo, binary);
			}
		}

		private void ProcessFill( SymbolInfo symbol, LogicalFill fill) {
			if( debug) log.Debug("===============================================");
			if( debug) log.Debug("Process Fill( " + symbol + ", " + fill + " ) ");
			if( debug) log.Debug("===============================================");
			LogicalOrderHandler handler = GetOrderHandler(symbol.BinaryIdentifier);
			handler.ProcessFill( fill);
			
			lock( orderHandlerLocker) {
				handler.ClearPhysicalOrders();
	        	foreach( var kvp in openOrders) {
					PhysicalOrder order = kvp.Value;
					if( order.Symbol == symbol) {
	        			HandleOpenOrder(kvp.Value);
					}
	        	}
    			handler.PerformCompare();
			}
		}
		
		
		public void RejectOrder( PacketFIX4_4 packetFIX) {
			if( !IsRecovered) {
				if( debug && (LogRecovery || !IsRecovery) ) {
					log.Debug("Order Rejected: " + packetFIX.Text + ")");
				}
				RemoveOrder( packetFIX, packetFIX.ClientOrderId);
			} else {
				string message = "Order Rejected: " + packetFIX.Text + "\n" + packetFIX;
				log.Error( message);
				throw new ApplicationException( message);
			}
		}
		
		private static readonly char[] DOT_SEPARATOR = new char[] { '.' };
		public void RemoveOrder( PacketFIX4_4 packetFIX, string clientOrderId) {
			if( debug && (LogRecovery || !IsRecovery) ) {
				log.Debug("RemoveOrder( " + clientOrderId + ")");
			}
			if( openOrders.ContainsKey(clientOrderId)) {
				if( trace) log.Trace( "Removing open order id: " + clientOrderId);
				openOrders.Remove(clientOrderId);
			}
		}	
		
		public void UpdateOrder( PacketFIX4_4 packetFIX, bool isActive, object note) {
			string clientOrderId = packetFIX.ClientOrderId;
			if( string.IsNullOrEmpty(clientOrderId)) {
				throw new ApplicationException("Client Order Id was null or empty.\n" + packetFIX);
			}
			if( debug && (LogRecovery || !IsRecovery) ) {
				log.Debug("UpdateOrder( " + clientOrderId + ", active = " + isActive + ")");
			}
			if( string.IsNullOrEmpty(packetFIX.Symbol) ) {
				throw new ApplicationException("Symbol was null or empty: \n" + packetFIX);
			}
			SymbolInfo symbolInfo;
			try {
				symbolInfo = Factory.Symbol.LookupSymbol(packetFIX.Symbol);
			} catch( ApplicationException ex) {
				log.Error("Error looking up " + packetFIX.Symbol + ": " + ex.Message);
				return;
			}
			OrderType orderType = OrderType.None;
			switch( packetFIX.Side) {
				case "1":
					switch( packetFIX.OrderType) {
						case "1":
							orderType = OrderType.BuyMarket;
							break;
						case "2":
							orderType = OrderType.BuyLimit;
							break;
						case "3":
							orderType = OrderType.BuyStop;
							break;
						default:
							break;
					}
					break;
				case "2":
				case "5":
					switch( packetFIX.OrderType) {
						case "1":
							orderType = OrderType.SellMarket;
							break;
						case "2":
							orderType = OrderType.SellLimit;
							break;
						case "3":
							orderType = OrderType.SellStop;
							break;
						default:
							break;
					}
					break;
				default:
					throw new ApplicationException("Unknown order side: '" + packetFIX.Side + "'\n" + packetFIX);
			}
			if( orderType == OrderType.None) {
				if( string.IsNullOrEmpty(packetFIX.OrderType)) {
					PhysicalOrder origOrder;
					if( openOrders.TryGetValue(packetFIX.ClientOrderId, out origOrder)) {
					   	orderType = origOrder.Type;
					}
				} else {
					throw new ApplicationException("Unknown order type: '" + packetFIX.OrderType + "'\n" + packetFIX);
				}
			}
			OrderSide side;
			switch( packetFIX.Side) {
				case "1":
					side = OrderSide.Buy;
					break;
				case "2":
					side = OrderSide.Sell;
					break;
				case "5":
					side = OrderSide.SellShort;
					break;
				default:
					throw new ApplicationException( "Unknown order side: " + packetFIX.Side + "\n" + packetFIX);
			}
			string[] parts = clientOrderId.Split(DOT_SEPARATOR);
			int logicalOrderId = 0;
			try {
				logicalOrderId = int.Parse(parts[0]);
			} catch( FormatException) {
			}
			PhysicalOrder order = Factory.Utility.PhysicalOrder(
				isActive, symbolInfo, side, orderType, packetFIX.Price, packetFIX.OrderQuantity, logicalOrderId, packetFIX.ClientOrderId, note);
			if( info && (LogRecovery || !IsRecovery) ) {
				log.Info("Updated order: " + order + ".  Executed: " + packetFIX.CumulativeQuantity + " Remaining: " + packetFIX.LeavesQuantity);
			}
			openOrders[packetFIX.ClientOrderId] = order;
			
			if( trace) {
				log.Trace("Updated order list:");
				foreach( var kvp in openOrders) {
					PhysicalOrder temp = kvp.Value;
					log.Trace( "    " + temp.BrokerOrder + " " + temp);
				}
			}
		}

		public void UpdatePartial( PacketFIX4_4 packetFIX) {
			string clientOrderId = packetFIX.OriginalClientOrderId;
			if( clientOrderId == null) {
				clientOrderId = packetFIX.ClientOrderId;
			}
		}
		
		private void TestMethod(PacketFIX4_4 packetFIX) {
			string account = packetFIX.Account;
			string destination = packetFIX.Destination;
			int orderQuantity = packetFIX.OrderQuantity;
			double averagePrice = packetFIX.AveragePrice;
			string orderID = packetFIX.OrderId;
			string massStatusRequestId = packetFIX.MassStatusRequestId;
			string positionEffect = packetFIX.PositionEffect;
			string orderType = packetFIX.OrderType;
			string clientOrderId = packetFIX.ClientOrderId;
			double price = packetFIX.Price;
			int cumulativeQuantity = packetFIX.CumulativeQuantity;
			string executionId = packetFIX.ExecutionId;
			int productType = packetFIX.ProductType;
			string symbol = packetFIX.Symbol;
			string side = packetFIX.Side;
			string timeInForce = packetFIX.TimeInForce;
			string executionType = packetFIX.ExecutionType;
			string internalOrderId = packetFIX.InternalOrderId;
			string transactionTime = packetFIX.TransactionTime;
			int leavesQuantity = packetFIX.LeavesQuantity;
		}
		
		private void HandleOpenOrder(PhysicalOrder order) {
			LogicalOrderHandler handler = GetOrderHandler(order.Symbol.BinaryIdentifier);
			handler.AddPhysicalOrder( order);
		}
		
		private void OnException( Exception ex) {
			// Attempt to propagate the exception.
			log.Error("Exception occurred", ex);
			SendError( ex.Message + "\n" + ex.StackTrace);
			Dispose();
		}
		
		private LogicalOrderHandler GetOrderHandler(long symbol) {
			LogicalOrderHandler handler;
			lock( orderHandlerLocker) {
				if( !orderHandlers.TryGetValue(symbol, out handler)) {
					SymbolInfo symbolInfo = Factory.Symbol.LookupSymbol(symbol);
					handler = Factory.Utility.LogicalOrderHandler( symbolInfo, this);
					orderHandlers.Add(symbol,handler);
				}
			}
			return handler;
		}
		
		private bool RemoveOrderHandler(long symbol) {
			lock( orderHandlerLocker) {
				if( orderHandlers.ContainsKey(symbol)) {
					orderHandlers.Remove(symbol);
					return true;
				} else {
					return false;
				}
			}
		}
		
		public override void PositionChange(Receiver receiver, SymbolInfo symbol, double signal, Iterable<LogicalOrder> orders)
		{
			if( !IsRecovered) {
				throw new ApplicationException("PositionChange event received prior to completing FIX recovery. Current connection status is: " + ConnectionStatus);
			}
			log.Info("Received PositionChange for " + symbol + " with position " + signal + " and the following orders:");
			
			if( orders != null) {
				orders.Iterate( (order) => {
					log.Info("Logical Order: " + order);
				});
			}
			
			if( openOrders != null) {
				foreach( var kvp in openOrders) {
					PhysicalOrder order = kvp.Value;
					log.Info("Physical Order: " + order);
				}
			}
			
			LogicalOrderHandler handler = GetOrderHandler(symbol.BinaryIdentifier);
			handler.SetDesiredPosition(signal);
			handler.SetLogicalOrders(orders);
			
			lock( orderHandlerLocker) {
				handler.ClearPhysicalOrders();
	        	foreach( var kvp in openOrders) {
					PhysicalOrder order = kvp.Value;
					if( order.Symbol == symbol) {
	        			HandleOpenOrder(kvp.Value);
					}
	        	}
    			handler.PerformCompare();
			}
		}
		
	    protected override void Dispose(bool disposing)
	    {
	    	base.Dispose(disposing);
           	nextConnectTime = Factory.Parallel.TickCount + 10000;
	    }    
	        
		private int GetLogicalOrderId(int physicalOrderId) {
        	int logicalOrderId;
        	if( physicalToLogicalOrderMap.TryGetValue(physicalOrderId,out logicalOrderId)) {
        		return logicalOrderId;
        	} else {
        		return 0;
        	}
		}
		Dictionary<int,int> physicalToLogicalOrderMap = new Dictionary<int, int>();
	        
		public void OnCreateBrokerOrder(PhysicalOrder physicalOrder)
		{
			if( debug) log.Debug( "OnCreateBrokerOrder " + physicalOrder);
			OnCreateOrChangeBrokerOrder(physicalOrder, false);
		}
	        
		private void OnCreateOrChangeBrokerOrder(PhysicalOrder physicalOrder, bool isChange)
		{
			Packet packet = Socket.CreatePacket();
			
			var fixMsg = new FIXMessage4_4(UserName);
			TimeStamp timeStamp = TimeStamp.UtcNow;
			fixMsg.SetClientOrderId(physicalOrder.LogicalOrderId + "." + timeStamp.Internal);
			fixMsg.SetAccount(AccountNumber);
			if( isChange) {
				fixMsg.AddHeader("G");
			} else {
				fixMsg.AddHeader("D");
			}
			fixMsg.SetHandlingInstructions(1);
			if( !isChange) {
				if( physicalOrder.Symbol.Destination.ToLower() == "default") {
					fixMsg.SetDestination("MBTX");
				} else {
					fixMsg.SetDestination(physicalOrder.Symbol.Destination);
				}
			}
			fixMsg.SetSymbol(physicalOrder.Symbol.Symbol);
			switch( physicalOrder.Side) {
				case OrderSide.Buy:
					fixMsg.SetSide(1);
					break;
				case OrderSide.Sell:
					fixMsg.SetSide(2);
					break;
				case OrderSide.SellShort:
					fixMsg.SetSide(5);
					break;
				case OrderSide.SellShortExempt:
					fixMsg.SetSide(6);
					break;
				default:
					throw new ApplicationException("Unknown OrderSide: " + physicalOrder.Side);
			}
			switch( physicalOrder.Type) {
				case OrderType.BuyLimit:
					fixMsg.SetOrderType(2);
					fixMsg.SetPrice(physicalOrder.Price);
					fixMsg.SetTimeInForce(1);
					break;
				case OrderType.BuyMarket:
					fixMsg.SetOrderType(1);
					fixMsg.SetTimeInForce(0);
					break;
				case OrderType.BuyStop:
					fixMsg.SetOrderType(3);
					fixMsg.SetPrice(physicalOrder.Price);
					fixMsg.SetStopPrice(physicalOrder.Price);
					fixMsg.SetTimeInForce(1);
					break;
				case OrderType.SellLimit:
					fixMsg.SetOrderType(2);
					fixMsg.SetPrice(physicalOrder.Price);
					fixMsg.SetTimeInForce(1);
					break;
				case OrderType.SellMarket:
					fixMsg.SetOrderType(1);
					fixMsg.SetTimeInForce(0);
					break;
				case OrderType.SellStop:
					fixMsg.SetOrderType(3);
					fixMsg.SetPrice(physicalOrder.Price);
					fixMsg.SetStopPrice(physicalOrder.Price);
					fixMsg.SetTimeInForce(1);
					break;
			}
			fixMsg.SetOriginalClientOrderId((string)physicalOrder.BrokerOrder);
			fixMsg.SetLocateRequired("N");
			fixMsg.SetTransactTime(timeStamp);
			fixMsg.SetOrderQuantity((int)physicalOrder.Size);
			fixMsg.SetOrderCapacity("A");
			fixMsg.SetUserName();
			string message = fixMsg.ToString();
			string view = message.Replace(FIXTBuffer.EndFieldStr,"  ");
			if( isChange) {
				log.Info("Change order: \n" + view);
			} else {
				log.Info("Create new order: \n" + view);
			}
			packet.DataOut.Write(message.ToCharArray());
			long end = Factory.Parallel.TickCount + 2000;
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending an order.");
				}
				Factory.Parallel.Yield();
			}
		}
		
		public void OnCancelBrokerOrder(PhysicalOrder physicalOrder)
		{
			if( debug) log.Debug( "OnCancelBrokerOrder " + physicalOrder);
			Packet packet = Socket.CreatePacket();
			
			var fixMsg = new FIXMessage4_4(UserName);
			TimeStamp timeStamp = TimeStamp.UtcNow;
			string newClientOrderId = physicalOrder.LogicalOrderId + "." + timeStamp.Internal;
			fixMsg.SetOriginalClientOrderId((string)physicalOrder.BrokerOrder);
			fixMsg.SetClientOrderId(newClientOrderId);
			fixMsg.SetAccount(AccountNumber);
			fixMsg.AddHeader("F");
			fixMsg.SetSymbol(physicalOrder.Symbol.Symbol);
			switch( physicalOrder.Type) {
				case OrderType.BuyLimit:
				case OrderType.BuyMarket:
				case OrderType.BuyStop:
					fixMsg.SetSide(1);
					break;
				case OrderType.SellLimit:
				case OrderType.SellMarket:
				case OrderType.SellStop:
					fixMsg.SetSide(2);
					break;
			}
			fixMsg.SetTransactTime(timeStamp);
			string message = fixMsg.ToString();
			packet.DataOut.Write(message.ToCharArray());
			log.Info("Cancel order: \n" + packet);
			long end = Factory.Parallel.TickCount + 2000;
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending a cancel order.");
				}
				Factory.Parallel.Yield();
			}
		}
		
		public void OnChangeBrokerOrder(PhysicalOrder physicalOrder)
		{
			if( debug) log.Debug( "OnChangeBrokerOrder " + physicalOrder);
			OnCreateOrChangeBrokerOrder( physicalOrder, true);
		}
	}
}
