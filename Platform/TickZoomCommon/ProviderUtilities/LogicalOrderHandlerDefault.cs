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
	public class LogicalOrderHandlerDefault : LogicalOrderHandler {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(LogicalOrderHandlerDefault));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		SymbolInfo symbol;
		PhysicalOrderHandler brokerOrders;
		List<PhysicalOrder> physicalOrders;
		IList<LogicalOrder> originalLogicals;
		List<LogicalOrder> logicalOrders;
		List<LogicalOrder> extraLogicals = new List<LogicalOrder>();
		double actualPosition;
		double desiredPosition;
		
		public LogicalOrderHandlerDefault(SymbolInfo symbol, PhysicalOrderHandler brokerOrders) {
			this.symbol = symbol;
			this.brokerOrders = brokerOrders;
			this.physicalOrders = new List<PhysicalOrder>();
			this.logicalOrders = new List<LogicalOrder>();
		}
		
		public void ClearPhysicalOrders() {
			physicalOrders.Clear();
		}
		
		public void SetActualPosition(double position) {
			this.actualPosition = position;
		}
		
		public void AddPhysicalOrder( bool isActive, OrderSide side, OrderType type, double price, int size, int logicalOrderId, object brokerOrder, object tag) {
			physicalOrders.Add( new PhysicalOrderDefault(isActive, symbol,side,type,price,size,logicalOrderId,brokerOrder, tag));
		}

		public void AddPhysicalOrder( bool isActive, OrderSide side, OrderType type, double price, int size, int logicalOrderId, object brokerOrder) {
			physicalOrders.Add( new PhysicalOrderDefault(isActive, symbol,side,type,price,size,logicalOrderId,brokerOrder, null));
		}

		public void AddPhysicalOrder( PhysicalOrder order) {
			physicalOrders.Add( order);
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
		
		private void TryCancelBrokerOrder(PhysicalOrder physical) {
			if( physical.IsActive) {
				if( trace) log.Trace("Cancel Broker Order " + physical);
				brokerOrders.OnCancelBrokerOrder(physical);
			}
		}
		
		private void TryChangeBrokerOrder(PhysicalOrder physical) {
			if( physical.IsActive) {
				if( trace) log.Trace("Change Broker Order " + physical);
				brokerOrders.OnChangeBrokerOrder(physical);
			}
		}
		
		private void CreateBrokerOrder(PhysicalOrder physical) {
			if( trace) log.Trace("Create Broker Order " + physical);
			brokerOrders.OnCreateBrokerOrder(physical);
		}
		
		private void ProcessMatchPhysicalEntry( LogicalOrder logical, PhysicalOrder physical) {
			log.Trace("ProcessMatchPhysicalEntry()");
			double difference = logical.Positions - Math.Abs(actualPosition);
			log.Trace("position difference = " + difference);
			if( difference == 0) {
				TryCancelBrokerOrder(physical);
			} else if( difference != physical.Size) {
				if( actualPosition == 0) {
					physicalOrders.Remove(physical);
					physical = new PhysicalOrderDefault(true, symbol,logical,difference,physical.BrokerOrder);
					TryChangeBrokerOrder(physical);
				} else {
					if( actualPosition > 0) {
						if( logical.Type == OrderType.BuyStop || logical.Type == OrderType.BuyLimit) {
							physicalOrders.Remove(physical);
							physical = new PhysicalOrderDefault(true, symbol,logical,difference,physical.BrokerOrder);
							TryChangeBrokerOrder(physical);
						} else {
							TryCancelBrokerOrder(physical);
						}
					}
					if( actualPosition < 0) {
						if( logical.Type == OrderType.SellStop || logical.Type == OrderType.SellLimit) {
							physicalOrders.Remove(physical);
							physical = new PhysicalOrderDefault(true, symbol,logical,difference,physical.BrokerOrder);
							TryChangeBrokerOrder(physical);
						} else {
							TryCancelBrokerOrder(physical);
						}
					}
				}
			} else if( logical.Price != physical.Price) {
				physicalOrders.Remove(physical);
				physical = new PhysicalOrderDefault(true, symbol,logical,difference,physical.BrokerOrder);
				TryChangeBrokerOrder(physical);
			}
		}
		
		private void ProcessMatchPhysicalExit( LogicalOrder logical, PhysicalOrder physical) {
			if( actualPosition == 0) {
				TryCancelBrokerOrder(physical);
			} else if( Math.Abs(actualPosition) != physical.Size || logical.Price != physical.Price) {
				physicalOrders.Remove(physical);
				physical = new PhysicalOrderDefault(true, symbol,logical,Math.Abs(actualPosition),physical.BrokerOrder);
				TryChangeBrokerOrder(physical);
			}
		}
		
		private void ProcessMatch(LogicalOrder logical, PhysicalOrder physical) {
			if( trace) log.Trace("Process Match()");
			if( logical.TradeDirection == TradeDirection.Entry) {
				ProcessMatchPhysicalEntry( logical, physical);
			}
			if( logical.TradeDirection == TradeDirection.Exit) {
				ProcessMatchPhysicalExit( logical, physical);
			}
		}
		
		private void ProcessMissingPhysical(LogicalOrder logical) {
			// When flat, allow entry orders.
			if( actualPosition == 0) {
				if( logical.TradeDirection == TradeDirection.Entry) {
					if(debug) log.Debug("ProcessMissingPhysicalEntry("+logical+")");
					PhysicalOrder physical = new PhysicalOrderDefault(true, symbol,logical,logical.Positions,null);
					CreateBrokerOrder(physical);
				}
			} else {
				if( logical.TradeDirection == TradeDirection.Exit) {
					ProcessMissingPhysicalExit( logical);
				}
			}
		}
		
		private void ProcessMissingPhysicalExit(LogicalOrder logical) {
			if(debug) log.Debug("ProcessMissingPhysicalExit("+logical+")");
			if( actualPosition > 0 ) {
				if( logical.Type == OrderType.SellLimit ||
				  logical.Type == OrderType.SellStop ||
				  logical.Type == OrderType.SellMarket) {
					PhysicalOrder physical = new PhysicalOrderDefault(true, symbol,logical,actualPosition,null);
					CreateBrokerOrder(physical);
				}
			}
			if( actualPosition < 0 ) {
				if( logical.Type == OrderType.BuyLimit ||
				  logical.Type == OrderType.BuyStop ||
				  logical.Type == OrderType.BuyMarket) {
					PhysicalOrder physical = new PhysicalOrderDefault(true, symbol,logical,Math.Abs(actualPosition),null);
					CreateBrokerOrder(physical);
				}
			}
		}
		
		private void ProcessMissingLogical(PhysicalOrder physical) {
			TryCancelBrokerOrder( physical);
		}
		
		private void ComparePosition() {
			double positionDelta = desiredPosition - actualPosition;
			double pendingAdjustments = 0D;
			for( int i=0; i<physicalOrders.Count; i++) {
				var order = physicalOrders[i];
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
					physicalOrders.RemoveAt(i);	i--;
				}
			}
			double delta = positionDelta - pendingAdjustments;
			PhysicalOrder physical;
			if( delta > 0) {
				physical = new PhysicalOrderDefault(true, symbol,OrderSide.Buy,OrderType.BuyMarket,0,delta,0,null,null);
				CreateBrokerOrder(physical);
			}
			if( delta < 0) {
				OrderSide side;
				if( actualPosition > 0) {
					side = OrderSide.Sell;
					delta = actualPosition;
				} else {
					side = OrderSide.SellShort;
				}
				side = (long) actualPosition >= (long) Math.Abs(delta) ? OrderSide.Sell : OrderSide.SellShort;
				physical = new PhysicalOrderDefault(true, symbol,side,OrderType.SellMarket,0,Math.Abs(delta),0,null,null);
				CreateBrokerOrder(physical);
			}
			actualPosition = desiredPosition;
		}
		
		public void SetLogicalOrders( IList<LogicalOrder> originalLogicals) {
			int orderCount = originalLogicals == null ? 0 : originalLogicals.Count;
			if( trace) log.Trace("SetLogicalOrders() order count = " + orderCount);
			this.originalLogicals = originalLogicals;
		}
		
		public void SetDesiredPosition(	double position) {
			this.desiredPosition = position;
		}
		
		private bool CheckForPending() {
			foreach( var order in physicalOrders) {
				if( !order.IsActive) {
					return true;	
				}
			}
			return false;
		}
		
		public void ProcessFill( LogicalFill fill) {
			if( debug) log.Debug( "Considering fill: " + fill );
			actualPosition = fill.Position;		
			bool cancelAllEntries = false;
			bool cancelAllExits = false;
			bool cancelAllExitStrategies = false;
			int orderId = fill.OrderId;
			if( orderId == 0) {
				// This is an adjust-to-position market order.
				// Just set the new position.
				return;
			}
			LogicalOrder filledOrder = null;
			logicalOrders.Clear();
			if( originalLogicals != null) {
				logicalOrders.AddRange(originalLogicals);
			}
			foreach( var order in logicalOrders) {
				if( order.Id == orderId) {
					if( debug) log.Debug( "Matched fill with orderId: " + orderId);
					filledOrder = order;
					desiredPosition = fill.Position;
					if( debug) log.Debug( "Changed desired to: " + desiredPosition);
					break;
				}
			}
			if( filledOrder != null) {
				bool clean = false;
				if( filledOrder.TradeDirection == TradeDirection.Entry ) {
					cancelAllEntries = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.Exit ) {
					cancelAllExits = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.ExitStrategy ) {
					cancelAllExitStrategies = true;
					clean = true;
				}
				if( clean) {
					foreach( var order in logicalOrders) {
						if( order.TradeDirection == TradeDirection.Entry && cancelAllEntries) {
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
		}
		
		public void PerformCompare() {
			int orderCount = originalLogicals == null ? 0 : originalLogicals.Count;
			if( debug) log.Debug( "PerformCompare() for " + symbol + " with " +
			                     actualPosition + " actual " + 
			                     desiredPosition + " desired and " +
			                     orderCount + " logical, " +
			                     physicalOrders.Count + " physical.");
			
			if( CheckForPending()) {
				if( debug) log.Debug("Found pending physical orders. Skipping compare.");
				return;
			}
			
			// First synchronize the position.
			ComparePosition();
				
			// Now synchronize the orders.
			logicalOrders.Clear();
			if(originalLogicals != null) {
				logicalOrders.AddRange(originalLogicals);
			}
			PhysicalOrder physical;
			extraLogicals.Clear();
			while( logicalOrders.Count > 0) {
				var logical = logicalOrders[0];
				if( TryMatchId(logical, out physical)) {
					ProcessMatch(logical,physical);
					physicalOrders.Remove(physical);
				} else {
					extraLogicals.Add(logical);
				}
				logicalOrders.Remove(logical);
			}
			if( debug) log.Debug("Found " + extraLogicals.Count + " extra logicals.");
			while( extraLogicals.Count > 0) {
				var logical = extraLogicals[0];
				ProcessMissingPhysical(logical);
				extraLogicals.Remove(logical);
			}
			if( debug) log.Debug("Found " + physicalOrders.Count + " extra physicals.");
			while( physicalOrders.Count > 0) {
				physical = physicalOrders[0];
				ProcessMissingLogical(physical);
				physicalOrders.Remove(physical);
			}
		}
		
		public double ActualPosition {
			get { return actualPosition; }
		}
	}
}
