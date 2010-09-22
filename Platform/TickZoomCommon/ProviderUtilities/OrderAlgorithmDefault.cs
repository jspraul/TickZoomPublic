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
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(OrderAlgorithmDefault));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		private SymbolInfo symbol;
		private PhysicalOrderHandler physicalOrderHandler;
		private ActiveList<PhysicalOrder> physicalOrders;
		private ActiveList<LogicalOrder> originalLogicals;
		private ActiveList<LogicalOrder> logicalOrders;
		private List<LogicalOrder> extraLogicals = new List<LogicalOrder>();
		private double actualPosition;
		private double desiredPosition;
		private Action<LogicalFillBinary> createLogicalFill;
		
		public OrderAlgorithmDefault(SymbolInfo symbol, PhysicalOrderHandler brokerOrders) {
			this.symbol = symbol;
			this.physicalOrderHandler = brokerOrders;
			this.originalLogicals = new ActiveList<LogicalOrder>();
			this.physicalOrders = new ActiveList<PhysicalOrder>();
			this.logicalOrders = new ActiveList<LogicalOrder>();
		}
		
		public void ClearPhysicalOrders() {
			physicalOrders.Clear();
		}
		
		public void SetActualPosition(double position) {
			this.actualPosition = position;
		}
		
		public void AddPhysicalOrder( OrderState orderState, OrderSide side, OrderType type, double price, int size, int logicalOrderId, object brokerOrder, object tag) {
			physicalOrders.AddLast( new PhysicalOrderDefault(orderState, symbol,side,type,price,size,logicalOrderId,brokerOrder, tag));
		}

		public void AddPhysicalOrder( OrderState orderState, OrderSide side, OrderType type, double price, int size, int logicalOrderId, object brokerOrder) {
			physicalOrders.AddLast( new PhysicalOrderDefault(orderState, symbol,side,type,price,size,logicalOrderId,brokerOrder, null));
		}

		public void AddPhysicalOrder( PhysicalOrder order) {
			physicalOrders.AddLast( order);
		}
		private bool TryMatchId( LogicalOrder logical, out PhysicalOrder physicalOrder) {
			foreach( var physical in physicalOrders) {
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
			foreach( var physical in physicalOrders) {
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
				if( trace) log.Trace("Cancel Broker Order " + physical);
				physicalOrderHandler.OnCancelBrokerOrder(physical);
				result = true;	
			}
			return result;
		}
		
		private void TryChangeBrokerOrder(PhysicalOrder physical) {
			if( physical.OrderState == OrderState.Active) {
				if( trace) log.Trace("Change Broker Order " + physical);
				physicalOrderHandler.OnChangeBrokerOrder(physical);
			}
		}
		
		private void CreateBrokerOrder(PhysicalOrder physical) {
			if( trace) log.Trace("Create Broker Order " + physical);
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
				if( strategyPosition == 0) {
					physicalOrders.Remove(physical);
					var side = GetOrderSide(logical.Type,strategyPosition);
					physical = new PhysicalOrderDefault(OrderState.Active,symbol,logical,side,difference,physical.BrokerOrder);
					TryChangeBrokerOrder(physical);
				} else {
					if( strategyPosition > 0) {
						if( logical.Type == OrderType.BuyStop || logical.Type == OrderType.BuyLimit) {
							physicalOrders.Remove(physical);
							var side = GetOrderSide(logical.Type,strategyPosition);
							physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,difference,physical.BrokerOrder);
							TryChangeBrokerOrder(physical);
						} else {
							TryCancelBrokerOrder(physical);
						}
					}
					if( strategyPosition < 0) {
						if( logical.Type == OrderType.SellStop || logical.Type == OrderType.SellLimit) {
							physicalOrders.Remove(physical);
							var side = GetOrderSide(logical.Type,strategyPosition);
							physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,difference,physical.BrokerOrder);
							TryChangeBrokerOrder(physical);
						} else {
							TryCancelBrokerOrder(physical);
						}
					}
				}
			} else if( logical.Price != physical.Price) {
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,difference,physical.BrokerOrder);
				TryChangeBrokerOrder(physical);
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
					physical = new PhysicalOrderDefault(OrderState.Active,symbol, logical,OrderSide.Buy,Math.Abs(delta),null);
					CreateBrokerOrder(physical);
				} else {
					OrderSide side;
					if( strategyPosition > 0 && logicalPosition < 0) {
						side = OrderSide.Sell;
						delta = strategyPosition;
					} else {
						side = OrderSide.SellShort;
					}
					side = (long) strategyPosition >= (long) Math.Abs(delta) ? OrderSide.Sell : OrderSide.SellShort;
					physical = new PhysicalOrderDefault(OrderState.Active,symbol, logical, side, Math.Abs(delta), null);
					CreateBrokerOrder(physical);
				}
			} else if( logical.Price != physical.Price) {
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol, logical, side, Math.Abs(delta),physical.BrokerOrder);
				TryChangeBrokerOrder(physical);
			}
		}
		
		private void ProcessMatchPhysicalExit( LogicalOrder logical, PhysicalOrder physical) {
			var strategyPosition = logical.StrategyPosition;
			if( strategyPosition == 0) {
				TryCancelBrokerOrder(physical);
			} else if( Math.Abs(strategyPosition) != physical.Size || logical.Price != physical.Price) {
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,Math.Abs(strategyPosition),physical.BrokerOrder);
				TryChangeBrokerOrder(physical);
			}
		}
		
		private void ProcessMatchPhysicalExitStrategy( LogicalOrder logical, PhysicalOrder physical) {
			var strategyPosition = logical.StrategyPosition;
			if( strategyPosition == 0) {
				TryCancelBrokerOrder(physical);
			} else if( Math.Abs(strategyPosition) != physical.Size || logical.Price != physical.Price) {
				physicalOrders.Remove(physical);
				var side = GetOrderSide(logical.Type,strategyPosition);
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,Math.Abs(strategyPosition),physical.BrokerOrder);
				TryChangeBrokerOrder(physical);
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
						PhysicalOrder physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,logical.Positions,null);
						CreateBrokerOrder(physical);
					}
					break;
				case TradeDirection.Exit:
					if( logical.StrategyPosition != 0) {
						var size = Math.Abs(logical.StrategyPosition);
						ProcessMissingPhysical( logical, size);
					}
					break;
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
				default:
					throw new ApplicationException("Unknown trade direction: " + logical.TradeDirection);
			}
			if( logical.TradeDirection == TradeDirection.Change) {
				if(debug) log.Debug("ProcessMissingPhysicalChange("+logical+")");
				var side = GetOrderSide(logical.Type,actualPosition);
				PhysicalOrder physical = new PhysicalOrderDefault(OrderState.Active,symbol,logical,side,logical.Positions,null);
				CreateBrokerOrder(physical);
			}
		}
		
		private void ProcessMissingPhysical(LogicalOrder logical, double size) {
			if( logical.StrategyPosition > 0) {
				if( logical.Type == OrderType.SellLimit ||
				  logical.Type == OrderType.SellStop ||
				  logical.Type == OrderType.SellMarket) {
					if(debug) log.Debug("ProcessMissingPhysical("+logical+")");
					var side = GetOrderSide(logical.Type,actualPosition);
					PhysicalOrder physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,size,null);
					CreateBrokerOrder(physical);
				}
			}
			if( logical.StrategyPosition < 0) {
				if( logical.Type == OrderType.BuyLimit ||
				  logical.Type == OrderType.BuyStop ||
				  logical.Type == OrderType.BuyMarket) {
					if(debug) log.Debug("ProcessMissingPhysical("+logical+")");
					var side = GetOrderSide(logical.Type,actualPosition);
					PhysicalOrder physical = new PhysicalOrderDefault(OrderState.Active, symbol,logical,side,size,null);
					CreateBrokerOrder(physical);
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
		
		private double FindPendingAdjustments() {
			double positionDelta = desiredPosition - actualPosition;
			double pendingAdjustments = 0D;
			var next = physicalOrders.First;
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
		
		private bool TrySyncPosition(double pendingAdjustments) {
			double positionDelta = desiredPosition - actualPosition;
			double delta = positionDelta - pendingAdjustments;
			PhysicalOrder physical;
			if( delta > 0) {
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,OrderSide.Buy,OrderType.BuyMarket,0,delta,0,null,null);
				CreateBrokerOrder(physical);
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
				physical = new PhysicalOrderDefault(OrderState.Active, symbol,side,OrderType.SellMarket,0,Math.Abs(delta),0,null,null);
				CreateBrokerOrder(physical);
				return true;
			} else {
				return false;
			}
		}
		
		public void SetLogicalOrders( Iterable<LogicalOrder> originalLogicals) {
			if( trace) {
				int count = originalLogicals == null ? 0 : originalLogicals.Count;
				log.Trace("SetLogicalOrders() order count = " + count);
			}
			this.originalLogicals.Clear();
			this.originalLogicals.AddLast(originalLogicals);
		}
		
		public void SetDesiredPosition(	double position) {
			this.desiredPosition = position;
		}
		
		private bool CheckForPending() {
			foreach( var order in physicalOrders) {
				if( order.OrderState == OrderState.Pending ||
				    order.Type == OrderType.BuyMarket ||
				    order.Type == OrderType.SellMarket) {
					return true;	
				}
			}
			return false;
		}
		
		public void ProcessFill( LogicalFillBinary fill) {
			if( debug) log.Debug( "Considering fill: " + fill );
			actualPosition = fill.Position;		
			bool cancelAllEntries = false;
			bool cancelAllExits = false;
			bool cancelAllExitStrategies = false;
			int orderId = fill.OrderId;
			if( orderId == 0) {
				// This is an adjust-to-position market order.
				// Position gets set via SetPosition instead.
				return;
			}
			LogicalOrder filledOrder = null;
			logicalOrders.Clear();
			if( originalLogicals != null) {
				logicalOrders.AddLast(originalLogicals);
			}
			foreach( var order in logicalOrders) {
				if( order.Id == orderId) {
					if( debug) log.Debug( "Matched fill with orderId: " + orderId);
					filledOrder = order;
					if( debug) log.Debug( "****REMOVED*****Changed desired to: " + desiredPosition);
					break;
				}
			}
			if( filledOrder != null) {
				bool clean = false;
				if( filledOrder.TradeDirection == TradeDirection.Change ) {
					cancelAllEntries = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.Entry ) {
					cancelAllEntries = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.Exit ) {
					cancelAllExits = true;
					cancelAllExitStrategies = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.ExitStrategy ) {
					cancelAllExitStrategies = true;
					clean = true;
				}
				if( clean) {
					foreach( var order in logicalOrders) {
						if( order.StrategyId == filledOrder.StrategyId) {
							if( order.TradeDirection == TradeDirection.Entry && cancelAllEntries) {
								originalLogicals.Remove(order);
							}
							if( order.TradeDirection == TradeDirection.Change && cancelAllEntries) {
								originalLogicals.Remove(order);
							}
							if( order.TradeDirection == TradeDirection.Exit && cancelAllExits) {
								originalLogicals.Remove(order);
							}
							if( order.TradeDirection == TradeDirection.ExitStrategy && cancelAllExitStrategies) {
								originalLogicals.Remove(order);
							}
						}
					}
				}
				if( createLogicalFill != null) {
					createLogicalFill( fill);
				}
			}
		}
		
		public void PerformCompare() {
			if( debug) log.Debug( "PerformCompare for " + symbol + " with " +
			                     actualPosition + " actual " + 
			                     desiredPosition + " desired and " +
			                     originalLogicals.Count + " logical, " +
			                     physicalOrders.Count + " physical.");
			
			if( debug) {
				foreach( var order in originalLogicals) {
					log.Debug("Logical Order: " + order);
				}
				
				foreach( var order in physicalOrders) {
					log.Debug("Physical Order: " + order);
				}
			}
			
			if( CheckForPending()) {
				if( debug) log.Debug("Found pending physical orders. Skipping compare.");
				return;
			}
			
			// First synchronize the orders to cancel any
			// invalid physical orders.
			logicalOrders.Clear();
			if(originalLogicals != null) {
				logicalOrders.AddLast(originalLogicals);
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
			double pendingAdjustments = FindPendingAdjustments();
			
			if( debug) log.Debug("Found " + physicalOrders.Count + " extra physicals.");
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
			
			if( debug) log.Debug("Found " + extraLogicals.Count + " extra logicals.");
			while( extraLogicals.Count > 0) {
				var logical = extraLogicals[0];
				ProcessExtraLogical(logical);
				extraLogicals.Remove(logical);
			}
		}
		
		public double ActualPosition {
			get { return actualPosition; }
		}
		
		public PhysicalOrderHandler PhysicalOrderHandler {
			get { return physicalOrderHandler; }
		}
		
		public Action<LogicalFillBinary> CreateLogicalFill {
			get { return createLogicalFill; }
			set { createLogicalFill = value; }
		}
	}
}
