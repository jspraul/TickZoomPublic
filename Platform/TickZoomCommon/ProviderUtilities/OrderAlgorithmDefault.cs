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
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.Common
{
	public class OrderAlgorithmDefault : OrderAlgorithm {
		private static readonly Log staticLog = Factory.SysLog.GetLogger("TickZoom.Common.OrderAlgorithm");
		private static readonly bool debug = staticLog.IsDebugEnabled;
		private static readonly bool trace = staticLog.IsTraceEnabled;
		private Log log;
		private SymbolInfo symbol;
		private PhysicalOrderHandler physicalOrderHandler;
		private LogicalOrderCache orderCache;
		private ActiveList<PhysicalOrder> originalPhysicals;
		private ActiveList<LogicalOrder> originalLogicals;
		private ActiveList<LogicalOrder> logicalOrders;
		private ActiveList<PhysicalOrder> physicalOrders;
		private List<LogicalOrder> extraLogicals = new List<LogicalOrder>();
		private int desiredPosition;
		private Action<SymbolInfo,LogicalFillBinary> onProcessFill;
		private bool handleSimulatedExits = false;
		private int actualPosition = 0;
		private int sentPhysicalOrders = 0;
		private TickSync tickSync;
		private object performCompareLocker = new object();
		private Dictionary<long,long> filledOrders = new Dictionary<long,long>();
		
		public OrderAlgorithmDefault(string name, SymbolInfo symbol, PhysicalOrderHandler brokerOrders) {
			this.log = Factory.SysLog.GetLogger("TickZoom.Common.OrderAlgorithm." + name + "." + symbol);
			this.symbol = symbol;
			this.tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
			this.physicalOrderHandler = brokerOrders;
			this.originalLogicals = new ActiveList<LogicalOrder>();
			this.originalPhysicals = new ActiveList<PhysicalOrder>();
			this.logicalOrders = new ActiveList<LogicalOrder>();
			this.physicalOrders = new ActiveList<PhysicalOrder>();
			this.orderCache = Factory.Engine.LogicalOrderCache(symbol);
		}
		
		private bool TryMatchId( LogicalOrder logical, out PhysicalOrder physicalOrder) {
			foreach( var physical in originalPhysicals) {
				if( logical.Id == physical.LogicalOrderId) {
					physicalOrder = physical;
					return true;
				}
			}
			physicalOrder = default(PhysicalOrder);
			return false;
		}
		
		private bool TryMatchTypeOnly( LogicalOrder logical, out PhysicalOrder physicalOrder) {
			double difference = logical.Positions - Math.Abs(actualPosition);
			foreach( var physical in originalPhysicals) {
				if( logical.Type == physical.Type) {
					if( logical.TradeDirection == TradeDirection.Entry) {
						if( difference != 0) {
							physicalOrder = physical;
							return true;
						}
					}
					if( logical.TradeDirection == TradeDirection.Exit) {
						if( actualPosition != 0) {
							physicalOrder = physical;
							return true;
						}
					}
				}
			}
			physicalOrder = default(PhysicalOrder);
			return false;
		}
		
		private bool TryCancelBrokerOrder(PhysicalOrder physical) {
			bool result = false;
			if( physical.OrderState != OrderState.Pending &&
			    // Market orders can't be canceled.
			    physical.Type != OrderType.BuyMarket &&
			    physical.Type != OrderType.SellMarket) {
				if( debug) log.Debug("Cancel Broker Order: " + physical);
				sentPhysicalOrders++;
				TryAddPhysicalOrder(physical);
				physicalOrderHandler.OnCancelBrokerOrder(symbol, physical.BrokerOrder);
				result = true;	
			}
			return result;
		}
		
		private void TryChangeBrokerOrder(PhysicalOrder physical, object origBrokerOrder) {
			if( physical.OrderState == OrderState.Active) {
				if( debug) log.Debug("Change Broker Order: " + physical);
				sentPhysicalOrders++;
				TryAddPhysicalOrder(physical);
				physicalOrderHandler.OnChangeBrokerOrder(physical,origBrokerOrder);
			}
		}
		
		private void TryAddPhysicalOrder(PhysicalOrder physical) {
			if( SyncTicks.Enabled) tickSync.AddPhysicalOrder(physical);
		}
		
		private void TryCreateBrokerOrder(PhysicalOrder physical) {
			if( debug) log.Debug("Create Broker Order " + physical);
			sentPhysicalOrders++;
			TryAddPhysicalOrder(physical);
			physicalOrderHandler.OnCreateBrokerOrder(physical);
		}
		
		private void ProcessMatchPhysicalEntry( LogicalOrder logical, PhysicalOrder physical) {
			log.Trace("ProcessMatchPhysicalEntry()");
			var strategyPosition = logical.StrategyPosition;
			var difference = logical.Positions - Math.Abs(strategyPosition);
			log.Trace("position difference = " + difference);
			if( difference == 0) {
				TryCancelBrokerOrder(physical);
			} else if( difference != physical.Size) {
				var origBrokerOrder = physical.BrokerOrder;
				if( strategyPosition == 0) {
					physicalOrders.Remove(physical);
					var side = GetOrderSide(logical.Type,strategyPosition);
					physical = new PhysicalOrderDefault(OrderState.Active,symbol,logical,side,difference);
					TryChangeBrokerOrder(physical,origBrokerOrder);
				} else {
					if( strategyPosition > 0) {
						if( logical.Type == OrderType.BuyStop || logical.Type == OrderType.BuyLimit) {
							physicalOrders.Remove(physical);
							var side = GetOrderSide(logical.Type,strategyPosition);
							physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,difference);
							TryChangeBrokerOrder(physical, origBrokerOrder);
						} else {
							TryCancelBrokerOrder(physical);
						}
					}
					if( strategyPosition < 0) {
						if( logical.Type == OrderType.SellStop || logical.Type == OrderType.SellLimit) {
							physicalOrders.Remove(physical);
							var side = GetOrderSide(logical.Type,strategyPosition);
							physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,difference);
							TryChangeBrokerOrder(physical, origBrokerOrder);
						} else {
							TryCancelBrokerOrder(physical);
						}
					}
				}
			} else if( logical.Price.ToLong() != physical.Price.ToLong()) {
				var origBrokerOrder = physical.BrokerOrder;
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,difference);
				TryChangeBrokerOrder(physical, origBrokerOrder);
			}
		}
		
		private void ProcessMatchPhysicalReverse( LogicalOrder logical, PhysicalOrder physical) {
			var strategyPosition = logical.StrategyPosition;
			var logicalPosition =
				logical.Type == OrderType.BuyLimit ||
				logical.Type == OrderType.BuyMarket ||
				logical.Type == OrderType.BuyStop ? 
				logical.Positions : - logical.Positions;
			var physicalPosition = 
				physical.Side == OrderSide.Buy ?
				physical.Size : - physical.Size;
			var delta = logicalPosition - strategyPosition;
			var difference = delta - physicalPosition;
			if( delta == 0) {
				TryCancelBrokerOrder(physical);
			} else if( difference != 0) {
				if( delta > 0) {
					physical = new PhysicalOrderDefault(OrderState.Active,symbol, logical,OrderSide.Buy,Math.Abs(delta));
					TryCreateBrokerOrder(physical);
				} else {
					OrderSide side;
					if( strategyPosition > 0 && logicalPosition < 0) {
						side = OrderSide.Sell;
						delta = strategyPosition;
					} else {
						side = OrderSide.SellShort;
					}
					side = (long) strategyPosition >= (long) Math.Abs(delta) ? OrderSide.Sell : OrderSide.SellShort;
					physical = new PhysicalOrderDefault(OrderState.Active,symbol, logical, side, Math.Abs(delta));
					TryCreateBrokerOrder(physical);
				}
			} else if( logical.Price.ToLong() != physical.Price.ToLong()) {
				var origBrokerOrder = physical.BrokerOrder;
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol, logical, side, Math.Abs(delta));
				TryChangeBrokerOrder(physical, origBrokerOrder);
			}
		}
		
		private void ProcessMatchPhysicalExit( LogicalOrder logical, PhysicalOrder physical) {
			var strategyPosition = logical.StrategyPosition;
			if( strategyPosition == 0) {
				TryCancelBrokerOrder(physical);
			} else if( Math.Abs(strategyPosition) != physical.Size || logical.Price.ToLong() != physical.Price.ToLong()) {
				var origBrokerOrder = physical.BrokerOrder;
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,Math.Abs(strategyPosition));
				TryChangeBrokerOrder(physical, origBrokerOrder);
			}
		}
		
		private void ProcessMatchPhysicalExitStrategy( LogicalOrder logical, PhysicalOrder physical) {
			var strategyPosition = logical.StrategyPosition;
			if( strategyPosition == 0) {
				TryCancelBrokerOrder(physical);
			} else if( Math.Abs(strategyPosition) != physical.Size || logical.Price.ToLong() != physical.Price.ToLong()) {
				var origBrokerOrder = physical.BrokerOrder;
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,Math.Abs(strategyPosition));
				TryChangeBrokerOrder(physical, origBrokerOrder);
			}
		}
		
		private void ProcessMatch(LogicalOrder logical, PhysicalOrder physical) {
			if( trace) log.Trace("Process Match()");
			if( physical.OrderState == OrderState.Suspended) {
				if( debug) log.Trace("Cannot change a suspended order: " + physical);
				return;
			}
			switch( logical.TradeDirection) {
				case TradeDirection.Entry:
					ProcessMatchPhysicalEntry( logical, physical);
					break;
				case TradeDirection.Exit:
					ProcessMatchPhysicalExit( logical, physical);
					break;
				case TradeDirection.ExitStrategy:
					ProcessMatchPhysicalExitStrategy( logical, physical);
					break;
				case TradeDirection.Reverse:
					ProcessMatchPhysicalReverse( logical, physical);
					break;
				default:
					throw new ApplicationException("Unknown TradeDirection: " + logical.TradeDirection);
			}
		}
		
		private void ProcessExtraLogical(LogicalOrder logical) {
			// When flat, allow entry orders.
			switch(logical.TradeDirection) {
				case TradeDirection.Entry:
					if( logical.StrategyPosition == 0) {
						if(debug) log.Debug("ProcessMissingPhysicalEntry("+logical+")");
						var side = GetOrderSide(logical.Type,actualPosition);
						var physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,logical.Positions);
						TryCreateBrokerOrder(physical);
					}
					break;
				case TradeDirection.Exit:
				case TradeDirection.ExitStrategy:
					if( logical.StrategyPosition != 0) {
						var size = Math.Abs(logical.StrategyPosition);
						ProcessMissingPhysical( logical, size);
					}
					break;
				case TradeDirection.Reverse:
					if( logical.StrategyPosition != 0) {
						var logicalPosition =
							logical.Type == OrderType.BuyLimit ||
							logical.Type == OrderType.BuyMarket ||
							logical.Type == OrderType.BuyStop ?
							logical.Positions : - logical.Positions;
						var size = Math.Abs(logicalPosition - logical.StrategyPosition);
						if( size != 0) {
							ProcessMissingPhysical( logical, size);
						}
					}
					break;
				case TradeDirection.Change:
					if( logical.StrategyPosition != 0) {
						var logicalPosition = 
							logical.Type == OrderType.BuyLimit ||
							logical.Type == OrderType.BuyMarket ||
							logical.Type == OrderType.BuyStop ?
							logical.Positions : - logical.Positions;
						var size = Math.Abs(logicalPosition - logical.StrategyPosition);
						if( size != 0 &&
						   (logical.Positions == 0 ||
						    Math.Sign(logicalPosition) == Math.Sign(logical.StrategyPosition))) {
							if(debug) log.Debug("ProcessMissingPhysicalEntry("+logical+")");
							var side = GetOrderSide(logical.Type,actualPosition);
							var physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,size);
							TryCreateBrokerOrder(physical);
						}
					}
					break;
				default:
					throw new ApplicationException("Unknown trade direction: " + logical.TradeDirection);
			}
			if( logical.TradeDirection == TradeDirection.Change) {
				if(debug) log.Debug("ProcessMissingPhysicalChange("+logical+")");
				var side = GetOrderSide(logical.Type,actualPosition);
				PhysicalOrder physical = new PhysicalOrderDefault(OrderState.Active,symbol,logical,side,logical.Positions);
				TryCreateBrokerOrder(physical);
			}
		}
		
		private void ProcessMissingPhysical(LogicalOrder logical, int size) {
			if( logical.StrategyPosition > 0) {
				if( logical.Type == OrderType.SellLimit ||
				  logical.Type == OrderType.SellStop ||
				  logical.Type == OrderType.SellMarket) {
					if(debug) log.Debug("ProcessMissingPhysical("+logical+")");
					var side = GetOrderSide(logical.Type,actualPosition);
					var physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,size);
					TryCreateBrokerOrder(physical);
				}
			}
			if( logical.StrategyPosition < 0) {
				if( logical.Type == OrderType.BuyLimit ||
				  logical.Type == OrderType.BuyStop ||
				  logical.Type == OrderType.BuyMarket) {
					if(debug) log.Debug("ProcessMissingPhysical("+logical+")");
					var side = GetOrderSide(logical.Type,actualPosition);
					var physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,size);
					TryCreateBrokerOrder(physical);
				}
			}
		}
		
		private OrderSide GetOrderSide(OrderType type, double actualPosition) {
			switch( type) {
				case OrderType.BuyLimit:
				case OrderType.BuyMarket:
				case OrderType.BuyStop:
					return OrderSide.Buy;
				case OrderType.SellLimit:
				case OrderType.SellMarket:
				case OrderType.SellStop:
					if( actualPosition > 0) {
						return OrderSide.Sell;
					} else {
						return OrderSide.SellShort;
					}
				default:
					throw new ApplicationException("Unknown OrderType: " + type);
			}
		}
		
		private bool ProcessExtraPhysical(PhysicalOrder physical) {
			return TryCancelBrokerOrder( physical);
		}
		
		private int FindPendingAdjustments() {
			var positionDelta = desiredPosition - actualPosition;
			var pendingAdjustments = 0;
			var next = originalPhysicals.First;
			for( var node = next; node != null; node = next) {
				next = node.Next;
				PhysicalOrder order = node.Value;
				if(order.Type != OrderType.BuyMarket &&
				   order.Type != OrderType.SellMarket) {
					continue;
				}
				if( order.LogicalOrderId == 0) {
					if( order.Type == OrderType.BuyMarket) {
						pendingAdjustments += order.Size;
					}
					if( order.Type == OrderType.SellMarket) {
						pendingAdjustments -= order.Size;
					}
					if( positionDelta > 0) {
						if( pendingAdjustments > positionDelta) {
							TryCancelBrokerOrder(order);
							pendingAdjustments -= order.Size;
						} else if( pendingAdjustments < 0) {
							TryCancelBrokerOrder(order);
							pendingAdjustments += order.Size;
						}
					}
					if( positionDelta < 0) {
						if( pendingAdjustments < positionDelta) {
							TryCancelBrokerOrder(order);
							pendingAdjustments += order.Size;
						} else if( pendingAdjustments > 0) {
							TryCancelBrokerOrder(order);
							pendingAdjustments -= order.Size;
						}
					}
					if( positionDelta == 0) {
						TryCancelBrokerOrder(order);
						pendingAdjustments += order.Type == OrderType.SellMarket ? order.Size : -order.Size;
					}
					physicalOrders.Remove(order);
				}
			}
			return pendingAdjustments;
		}
		
		private bool TrySyncPosition(int pendingAdjustments) {
			var positionDelta = desiredPosition - actualPosition;
			var delta = positionDelta - pendingAdjustments;
			PhysicalOrder physical;
//			if( delta != 0) {
//				throw new ApplicationException("Position delta was " + delta + ", desired="+desiredPosition+", actual="+actualPosition +", pending="+pendingAdjustments);
//			}
			if( delta > 0) {
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,OrderSide.Buy,OrderType.BuyMarket,0,delta,0,0,null,null);
				TryCreateBrokerOrder(physical);
				return true;
			} else if( delta < 0) {
				OrderSide side;
				if( actualPosition > 0 && desiredPosition < 0) {
					side = OrderSide.Sell;
					delta = actualPosition;
				} else {
					side = OrderSide.SellShort;
				}
				side = (long) actualPosition >= (long) Math.Abs(delta) ? OrderSide.Sell : OrderSide.SellShort;
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,side,OrderType.SellMarket,0,Math.Abs(delta),0,0,null,null);
				TryCreateBrokerOrder(physical);
				return true;
			} else {
				return false;
			}
		}
		
		public void SetLogicalOrders( Iterable<LogicalOrder> inputLogicals) {
			if( trace) {
				int count = originalLogicals == null ? 0 : originalLogicals.Count;
				log.Trace("SetLogicalOrders() order count = " + count);
			}
			orderCache.SetActiveOrders(inputLogicals);
			originalLogicals.Clear();
			originalLogicals.AddLast(orderCache.ActiveOrders);
		}
		
		public void SetDesiredPosition(	int position) {
			this.desiredPosition = position;
		}
		
		private bool CheckForPending() {
			var result = false;
			foreach( var order in originalPhysicals) {
				if( order.OrderState == OrderState.Pending ||
				    order.Type == OrderType.BuyMarket ||
				    order.Type == OrderType.SellMarket) {
					if( debug) log.Debug("Pending order: " + order);
					result = true;	
				}
			}
			return result;
		}
		
		private LogicalOrder FindLogicalOrder(int orderId) {
			foreach( var order in originalLogicals) {
				if( order.Id == orderId) {
					return order;
				}
			}
			throw new ApplicationException("LogicalOrder was not found for order id: " + orderId);
		}
		
		public void ProcessFill( PhysicalFill physical) {
			if( debug) log.Debug( "ProcessFill() physical: " + physical);
//			log.Warn( "ProcessFill() physical: " + physical);
			physicalOrders.Remove(physical.Order);
			LogicalFillBinary fill;
			try { 
				var logical = FindLogicalOrder(physical.Order.LogicalOrderId);
				UpdateOrderCache(physical.Size,logical);
				desiredPosition += physical.Size;
				if( debug) log.Debug("Adjusting symbol position to desired " + desiredPosition + ", physical fill was " + physical.Size);
				var position = logical.StrategyPosition;
				if( debug) log.Debug("Creating logical fill with position " + position + " from strategy position " + logical.StrategyPosition);
				fill = new LogicalFillBinary(
					position, physical.Price, physical.Time, physical.UtcTime, physical.Order.LogicalOrderId, physical.Order.LogicalSerialNumber);
			} catch( ApplicationException) {
				if( debug) log.Debug("Leaving symbol position at desired " + desiredPosition + ", since this was an adjustment market order.");
				if( debug) log.Debug("Skipping logical fill for an adjustment market order.");
				if( debug) log.Debug("Performing extra compare.");
				PerformCompareInternal();
				TryRemovePhysicalFill(physical);
				return;
			}
			if( debug) log.Debug("Fill price: " + fill);
			ProcessFill( fill);
		}		
		
		private long nextOrderId = 1000000000;
		private long GetUniqueOrderId() {
			if( false) {
				return Interlocked.Increment(ref nextOrderId);
			} else {
				return TimeStamp.UtcNow.Internal;
			}
		}
		
		private void TryRemovePhysicalFill(PhysicalFill fill) {
			if( SyncTicks.Enabled) tickSync.RemovePhysicalFill(fill);
		}
		
		private void ProcessFill( LogicalFillBinary fill) {
			if( debug) log.Debug( "ProcessFill() logical: " + fill );
			bool cancelAllEntries = false;
			bool cancelAllExits = false;
			bool cancelAllExitStrategies = false;
			bool cancelAllReverse = false;
			int orderId = fill.OrderId;
			if( orderId == 0) {
				// This is an adjust-to-position market order.
				// Position gets set via SetPosition instead.
				return;
			}
			
			logicalOrders.Clear();
			if( originalLogicals != null) {
				logicalOrders.AddLast(originalLogicals);
			}
			try { 
				var filledOrder = FindLogicalOrder( orderId);
				if( debug) log.Debug( "Matched fill with order: " + filledOrder);
				filledOrders.Add(filledOrder.SerialNumber,TimeStamp.UtcNow.Internal);
				originalLogicals.Remove(filledOrder);
				
			
				bool clean = false;
				switch( filledOrder.TradeDirection) {
					case TradeDirection.Change:
						cancelAllEntries = true;
						clean = true;
						break;
					case TradeDirection.Entry:
						cancelAllEntries = true;
						clean = true;
						break;
					case TradeDirection.Exit:
					case TradeDirection.ExitStrategy:
						cancelAllExits = true;
						cancelAllExitStrategies = true;
						clean = true;
						break;
					case TradeDirection.Reverse:
						cancelAllReverse = true;
						clean = true;
						break;
					default:
						throw new ApplicationException("Unknown trade direction: " + filledOrder.TradeDirection);
				}
				if( clean) {
					foreach( var order in logicalOrders) {
						if( order.StrategyId == filledOrder.StrategyId) {
							switch( order.TradeDirection) {
								case TradeDirection.Entry:
									if( cancelAllEntries) originalLogicals.Remove(order);
									break;
								case TradeDirection.Change:
									if( cancelAllEntries) originalLogicals.Remove(order);
									break;
								case TradeDirection.Exit:
									if( cancelAllExits) originalLogicals.Remove(order);
									break;
								case TradeDirection.ExitStrategy:
									if( cancelAllExitStrategies) originalLogicals.Remove(order);
									break;
								case TradeDirection.Reverse:
									if( cancelAllReverse) originalLogicals.Remove(order);
									break;
								default:
									throw new ApplicationException("Unknown trade direction: " + filledOrder.TradeDirection);
							}
						}
					}
				}
			} catch( ApplicationException) {
				
			}
			if( onProcessFill != null) {
				if( debug) log.Debug("Sending logical fill for " + symbol + ": " + fill);
				onProcessFill( symbol, fill);
			}
			if( debug) log.Debug("Performing extra compare.");
			lock( performCompareLocker) {
				PerformCompareInternal();
			}
		}
		
		private void UpdateOrderCache(int fillSize, LogicalOrder order) {
			var strategyPosition = orderCache.GetStrategyPosition(order.StrategyId);
		
			var position = strategyPosition.Position + fillSize;
			if( debug) log.Debug("Adjusting strategy position to " + position + ", fill size was " + fillSize +", strategy position was " + strategyPosition.Position);
			strategyPosition.Position = position;
			orderCache.RemoveInactive(order);
		}
		
		public int PerformCompare() {
			sentPhysicalOrders = 0;
			lock( performCompareLocker) {
				PerformCompareInternal();
			}
			return sentPhysicalOrders;
		}

		private bool CheckForFilledOrders() {
			foreach( var logical in originalLogicals) {
				var binaryTime = 0L;
				if( filledOrders.TryGetValue( logical.SerialNumber, out binaryTime)) {
					if( debug) log.Debug("Found already filled order: " + logical);
				   	return true;
				}
			}
			return false;
		}
		
		private void PerformCompareInternal() {
			if( debug) {
				log.Debug( "PerformCompare for " + symbol + " with " +
			                     actualPosition + " actual " + 
			                     desiredPosition + " desired and " +
			                     originalLogicals.Count + " logical, " +
			                     originalPhysicals.Count + " physical.");
			}
			originalPhysicals.Clear();
			originalPhysicals.AddLast( physicalOrderHandler.GetActiveOrders(symbol));
			if( debug) {
				foreach( var order in originalLogicals) {
					log.Debug("Logical Order: " + order);
				}
				
				foreach( var order in originalPhysicals) {
					log.Debug("Physical Order: " + order);
				}
			}
			
			if( CheckForFilledOrders()) {
				if( debug) log.Debug("Found already filled orders in position change event. Skipping compare.");
				originalLogicals.Clear();
				originalLogicals.AddLast(logicalOrders);
			}
			
			if( CheckForPending()) {
				if( debug) log.Debug("Found pending physical orders. Skipping compare.");
				return;
			}
			
			logicalOrders.Clear();
			if(originalLogicals != null) {
				logicalOrders.AddLast(originalLogicals);
			}
			
			physicalOrders.Clear();
			if(originalPhysicals != null) {
				physicalOrders.AddLast(originalPhysicals);
			}
			
			PhysicalOrder physical;
			extraLogicals.Clear();
			while( logicalOrders.Count > 0) {
				var logical = logicalOrders.First.Value;
				if( TryMatchId(logical, out physical)) {
					ProcessMatch(logical,physical);
					physicalOrders.Remove(physical);
				} else {
					extraLogicals.Add(logical);
				}
				logicalOrders.Remove(logical);
			}

			// Find any pending adjustments.
			int pendingAdjustments = FindPendingAdjustments();
			
			if( trace) log.Trace("Found " + physicalOrders.Count + " extra physicals.");
			int cancelCount = 0;
			while( physicalOrders.Count > 0) {
				physical = physicalOrders.First.Value;
				if( ProcessExtraPhysical(physical)) {
					cancelCount++;
				}
				physicalOrders.Remove(physical);
			}
			
			if( cancelCount > 0) {
				// Wait for cancels to complete before creating any orders.
				return;
			}

			if( TrySyncPosition( pendingAdjustments)) {
				// Wait for fill to process before creating any orders.
				return;
			}
			
			if( trace) log.Trace("Found " + extraLogicals.Count + " extra logicals.");
			while( extraLogicals.Count > 0) {
				var logical = extraLogicals[0];
				ProcessExtraLogical(logical);
				extraLogicals.Remove(logical);
			}
		}
	
		public int ActualPosition {
			get { return actualPosition; }
		}

		public void SetActualPosition( int position) {
			actualPosition = position;
		}
		public PhysicalOrderHandler PhysicalOrderHandler {
			get { return physicalOrderHandler; }
		}
		
		public Action<SymbolInfo,LogicalFillBinary> OnProcessFill {
			get { return onProcessFill; }
			set { onProcessFill = value; }
		}
		
		public bool HandleSimulatedExits {
			get { return handleSimulatedExits; }
			set { handleSimulatedExits = value; }
		}
		
		// This is a callback to confirm order was properly placed.
		public void OnChangeBrokerOrder(PhysicalOrder order, object origBrokerOrder)
		{
			lock( performCompareLocker) {
				PerformCompareInternal();
			}
			if( SyncTicks.Enabled) {
				tickSync.RemovePhysicalOrder( order);
			}
		}
		
		public void OnCreateBrokerOrder(PhysicalOrder order)
		{
			lock( performCompareLocker) {
				PerformCompareInternal();
			}
			if( SyncTicks.Enabled) {
				tickSync.RemovePhysicalOrder( order);
			}
		}
		
		public void OnCancelBrokerOrder(SymbolInfo symbol, object origBrokerOrder)
		{
			lock( performCompareLocker) {
				PerformCompareInternal();
			}
			if( SyncTicks.Enabled) {
				tickSync.RemovePhysicalOrder( origBrokerOrder);
			}
		}
		
		public Iterable<PhysicalOrder> GetActiveOrders(SymbolInfo symbol)
		{
			throw new NotImplementedException();
		}
	}
}
