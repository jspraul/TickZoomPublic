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
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	public class FillSimulatorDefault : FillSimulator
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(FillSimulatorDefault));
		private static readonly bool trace = log.IsTraceEnabled;
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool notice = log.IsNoticeEnabled;
		private Iterable<LogicalOrder> activeOrders;
		private Action<LogicalFillBinary> createLogicalFill;
		private Func<LogicalOrder, double, double, int> drawTrade;
		private bool useSyntheticMarkets = true;
		private bool useSyntheticStops = true;
		private bool useSyntheticLimits = true;
		private bool doStrategyOrders = true;
		private bool doExitStrategyOrders = false;
		private SymbolInfo symbol;
		private bool allowReversal = true;
		private bool graphTrades = false;
		private Func<double> getActualPosition;
		
		public FillSimulatorDefault(Func<double> getActualPosition)
		{
			this.getActualPosition = getActualPosition;
		}

		public FillSimulatorDefault(Func<double> getActualPosition, StrategyInterface strategyInterface)
		{
			Strategy strategy = (Strategy) strategyInterface;
			graphTrades = strategy.Performance.GraphTrades;
		}
		
		public bool ProcessOrders(Tick tick, Iterable<LogicalOrder> orders, double position)
		{
			bool retVal = false;
			if( symbol == null) {
				throw new ApplicationException("Please set the Symbol property for the " + GetType().Name + ".");
			}
			this.activeOrders = orders;
			var next = activeOrders.First;
			for( var node = next; node != null; node = next) {
				next = node.Next;
				LogicalOrder order = node.Value;
				if (order.IsActive) {
					if (doStrategyOrders) {
						switch( order.TradeDirection) {
							case TradeDirection.Entry:
								if( OnProcessEnterOrder(order, order.Strategy.Position, tick)) {
									retVal = true;
								}
								break;
							case TradeDirection.Exit:
								if( OnProcessExitOrder(order, order.Strategy.Position, tick)) {
									retVal = true;
								}
								break;
							case TradeDirection.Reverse:
								if( OnProcessReverseOrder(order, order.Strategy.Position, tick)) {
									retVal = true;
								}
								break;
							case TradeDirection.Change:
								if( OnProcessChangeOrder(order, order.Strategy.Position, tick)) {
									retVal = true;
								}
								break;
						}
					}
					if (doExitStrategyOrders) {
						if( order.TradeDirection == TradeDirection.ExitStrategy) {
							retVal = OnProcessExitOrder(order, order.Strategy.Result.Position, tick);
						}
					}
				}
			}
			return retVal;
		}
		
#region ExitOrder

		private bool OnProcessExitOrder(LogicalOrder order, PositionInterface position, Tick tick)
		{
			bool retVal = false;
			if (trace)
				log.Trace("OnProcessExitOrder()");
			if (position.IsLong) {
				if (order.Type == OrderType.BuyStop || order.Type == OrderType.BuyLimit) {
					order.Status = OrderStatus.Inactive;
				}
			}
			if (position.IsShort) {
				 if (order.Type == OrderType.SellStop || order.Type == OrderType.SellLimit) {
					order.Status = OrderStatus.Inactive;
				}
			}

			if (position.IsLong) {
				switch (order.Type) {
					case OrderType.SellMarket:
						if (useSyntheticMarkets) {
							if( ProcessSellMarket(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.SellStop:
						if (useSyntheticStops) {
							if( ProcessSellStop(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.SellLimit:
						if (useSyntheticLimits) {
							if( ProcessSellLimit(order, tick)) {
								retVal = true;
							}
						}
						break;
				}
			}

			if (position.IsShort) {
				switch (order.Type) {
					case OrderType.BuyMarket:
						if (useSyntheticMarkets) {
							if( ProcessBuyMarket(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.BuyStop:
						if (useSyntheticStops) {
							if( ProcessBuyStop(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.BuyLimit:
						if (useSyntheticLimits) {
							if( ProcessBuyLimit(order, tick)) {
								retVal = true;
							}
						}
						break;
				}
			}
			return retVal;
		}

		private bool OnProcessReverseOrder(LogicalOrder order, PositionInterface position, Tick tick)
		{
			bool retVal = false;
			if (trace)
				log.Trace("OnProcessEnterOrder()");
			if (position.IsLong) {
				if (order.Type == OrderType.BuyStop || order.Type == OrderType.BuyLimit) {
					order.Status = OrderStatus.Inactive;
				}
			}
			if (position.IsShort) {
				 if (order.Type == OrderType.SellStop || order.Type == OrderType.SellLimit) {
					order.Status = OrderStatus.Inactive;
				}
			}

			if (position.IsLong) {
				switch (order.Type) {
					case OrderType.SellMarket:
						if (useSyntheticMarkets) {
							if( ProcessReverseSellMarket(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.SellStop:
						if (useSyntheticStops) {
							if( ProcessReverseSellStop(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.SellLimit:
						if (useSyntheticLimits) {
							if( ProcessReverseSellLimit(order, tick)) {
								retVal = true;
							}
						}
						break;
				}
			}

			if (position.IsShort) {
				switch (order.Type) {
					case OrderType.BuyMarket:
						if (useSyntheticMarkets) {
							if( ProcessReverseBuyMarket(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.BuyStop:
						if (useSyntheticStops) {
							if( ProcessReverseBuyStop(order, tick)) {
								retVal = true;
							}
						}
						break;
					case OrderType.BuyLimit:
						if (useSyntheticLimits) {
							if( ProcessReverseBuyLimit(order, tick)) {
								retVal = true;
							}
						}
						break;
				}
			}
			return retVal;
		}
		
		private void FlattenPosition(double price, Tick tick, LogicalOrder order)
		{
			log.Info("FlattenPosition for: " + order);
			CreateLogicalFillHelper(0,price,tick.Time,order);
			CancelExitOrders(order.Strategy,order.TradeDirection);
		}

		private void ModifyPosition( double position, double price, TimeStamp time, LogicalOrder order) {
			CreateLogicalFillHelper(position, price, time, order);
			CancelEnterOrders(order.Strategy);
		}
		
		private void ReversePosition(double price, Tick tick, LogicalOrder order)
		{
             double position = 0;
             switch (order.Type)
             {
                 case OrderType.BuyLimit:
                 case OrderType.BuyMarket:
                 case OrderType.BuyStop:
                     position = order.Positions;
                     break;
                 case OrderType.SellLimit:
                 case OrderType.SellMarket:
                 case OrderType.SellStop:
                     position = -order.Positions;
                     break;
                 default:
                     throw new ApplicationException("Unexpected order type: " + order.Type);
             }
			CreateLogicalFillHelper(position, price, tick.Time, order);
			CancelReverseOrders(order.Strategy);
		}
		
		public void CancelReverseOrders(StrategyInterface strategy) {
			var next = activeOrders.First;
			for( var node = next; node != null; node = next) {
				next = node.Next;
				LogicalOrder order = node.Value;
				if (object.ReferenceEquals(strategy,order.Strategy) &&
				   (order.TradeDirection == TradeDirection.Exit ||
				     order.TradeDirection == TradeDirection.Reverse)) {
					order.Status = OrderStatus.Inactive;
				}
			}
		}
		
		public void CancelExitOrders(StrategyInterface strategy, TradeDirection tradeDirection)
		{
			var next = activeOrders.First;
			for( var node = next; node != null; node = next) {
				next = node.Next;
				LogicalOrder order = node.Value;
				if (object.ReferenceEquals(strategy,order.Strategy) &&
				    order.TradeDirection == tradeDirection) {
					order.Status = OrderStatus.Inactive;
				}
			}
		}

		private void TryDrawTrade(LogicalOrder order, double price, double position) {
			if (drawTrade != null && graphTrades == true) {
				drawTrade(order, price, position);
			}
		}

		private bool ProcessBuyMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			FlattenPosition(price, tick, order);
			return true;
		}
		
		private bool ProcessSellMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			FlattenPosition(price, tick, order);
			return true;
		}

		private bool ProcessBuyStop(LogicalOrder order, Tick tick)
		{
			bool retVal = false;
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price >= order.Price) {
				FlattenPosition(price, tick, order);
				retVal = true;	
			}
			return retVal;
		}

		private bool ProcessBuyLimit(LogicalOrder order, Tick tick)
		{
			bool isFilled = false;
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			if (price <= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price < order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				FlattenPosition(price, tick, order);
			}
			return isFilled;
		}

		private bool ProcessSellStop(LogicalOrder order, Tick tick)
		{
			bool retVal = false;
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			if (price <= order.Price) {
				FlattenPosition(price, tick, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessSellLimit(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			bool isFilled = false;
			if (price >= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price > order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				FlattenPosition(price, tick, order);
			}
			return isFilled;
		}

		private bool ProcessReverseBuyMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			ReversePosition( price, tick, order);
			return true;
		}
		
		private bool ProcessReverseSellMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			ReversePosition( price, tick, order);
			return true;
		}

		private bool ProcessReverseBuyStop(LogicalOrder order, Tick tick)
		{
			bool retVal = false;
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price >= order.Price) {
				ReversePosition( price, tick, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessReverseBuyLimit(LogicalOrder order, Tick tick)
		{
			bool isFilled = false;
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			if (price <= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price < order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				ReversePosition( price, tick, order);
			}
			return isFilled;
		}

		private bool ProcessReverseSellStop(LogicalOrder order, Tick tick)
		{
			bool retVal = false;
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			if (price <= order.Price) {
				ReversePosition( price, tick, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessReverseSellLimit(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			bool isFilled = false;
			if (price >= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price > order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				ReversePosition( price, tick, order);
			}
			return isFilled;
		}
		
#endregion
		

#region EntryOrders

		private bool OnProcessEnterOrder(LogicalOrder order, PositionInterface position, Tick tick)
		{
			bool retVal = false;
			if (trace) log.Trace("OnProcessEnterOrder()");
			if (position.IsFlat || (allowReversal && position.IsShort)) {
				if (order.Type == OrderType.BuyMarket && useSyntheticMarkets) {
					if( ProcessEnterBuyMarket(order, tick)) {
						retVal = true;
					}
				}
				if (order.Type == OrderType.BuyStop && useSyntheticStops) {
					if( ProcessEnterBuyStop(order, tick)) {
						retVal = true;
					}
				}
				if (order.Type == OrderType.BuyLimit && useSyntheticLimits) {
					if( ProcessEnterBuyLimit(order, tick)) {
						retVal = true;
					}
				}
			}

			if (position.IsFlat || (allowReversal && position.IsLong)) {
				if (order.Type == OrderType.SellMarket && useSyntheticMarkets) {
					if( ProcessEnterSellMarket(order, tick)) {
						retVal = true;
					}
				}
				if (order.Type == OrderType.SellStop && useSyntheticStops) {
					if( ProcessEnterSellStop(order, tick)) {
						retVal = true;
					}
				}
				if (order.Type == OrderType.SellLimit && useSyntheticLimits) {
					if( ProcessEnterSellLimit(order, tick)) {
						retVal = true;
					}
				}
			}
			return retVal;
		}

		private bool ProcessEnterBuyStop(LogicalOrder order, Tick tick)
		{
			bool retVal = true;
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			if (price >= order.Price) {
				CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessEnterSellStop(LogicalOrder order, Tick tick)
		{
			bool retVal = true;
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price <= order.Price) {
				CreateLogicalFillHelper(-order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessEnterBuyMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
			CancelEnterOrders(order.Strategy);
			return true;
		}

		public void CancelEnterOrders(StrategyInterface strategy)
		{
			var next = activeOrders.First;
			for( var node = next; node != null; node = next) {
				next = node.Next;
				LogicalOrder order = node.Value;
				if (object.ReferenceEquals(strategy,order.Strategy) &&
				    order.TradeDirection == TradeDirection.Entry) {
					order.Status = OrderStatus.Inactive;
				}
			}
		}
		
		private void CreateLogicalFillHelper(double position, double price, TimeStamp time, LogicalOrder order) {
			log.Info("Filled: " + order);
			LogicalFillBinary fill = new LogicalFillBinary(position,price,time,order.Id);
			log.Info("Fill price: " + fill);
			createLogicalFill(fill);
		}
		
		private bool ProcessEnterBuyLimit(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			bool isFilled = false;
			if (price <= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price < order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
			}
			return isFilled;
		}

		private bool ProcessEnterSellMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			CreateLogicalFillHelper(-order.Positions, price, tick.Time, order);
			CancelEnterOrders(order.Strategy);
			return true;
		}

		private bool ProcessEnterSellLimit(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			bool isFilled = false;
			if (price >= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price > order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				CreateLogicalFillHelper(-order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
			}
			return isFilled;
		}

#endregion

#region ChangeOrders

		private bool OnProcessChangeOrder(LogicalOrder order, PositionInterface position, Tick tick)
		{
			bool retVal = false;
			if (trace) log.Trace("OnProcessEnterOrder()");
			if (order.Type == OrderType.BuyMarket && useSyntheticMarkets) {
				if( ProcessChangeBuyMarket(order, tick)) {
					retVal = true;
				}
			}
			if (order.Type == OrderType.BuyStop && useSyntheticStops) {
				if( ProcessChangeBuyStop(order, tick)) {
					retVal = true;
				}
			}
			if (order.Type == OrderType.BuyLimit && useSyntheticLimits) {
				if( ProcessChangeBuyLimit(order, tick)) {
					retVal = true;
				}
			}
			if (order.Type == OrderType.SellMarket && useSyntheticMarkets) {
				if( ProcessChangeSellMarket(order, tick)) {
					retVal = true;
				}
			}
			if (order.Type == OrderType.SellStop && useSyntheticStops) {
				if( ProcessChangeSellStop(order, tick)) {
					retVal = true;
				}
			}
			if (order.Type == OrderType.SellLimit && useSyntheticLimits) {
				if( ProcessChangeSellLimit(order, tick)) {
					retVal = true;
				}
			}
			return retVal;
		}

		private bool ProcessChangeBuyStop(LogicalOrder order, Tick tick)
		{
			bool retVal = true;
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			if (price >= order.Price) {
				CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessChangeSellStop(LogicalOrder order, Tick tick)
		{
			bool retVal = true;
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price <= order.Price) {
				CreateLogicalFillHelper(-order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessChangeBuyMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			CreateLogicalFillHelper(getActualPosition() + order.Positions, price, tick.Time, order);
			CancelEnterOrders(order.Strategy);
			return true;
		}

		public void CancelChangeOrders(StrategyInterface strategy)
		{
			var next = activeOrders.First;
			for( var node = next; node != null; node = next) {
				next = node.Next;
				LogicalOrder order = node.Value;
				if (object.ReferenceEquals(strategy,order.Strategy) &&
				    order.TradeDirection == TradeDirection.Entry) {
					order.Status = OrderStatus.Inactive;
				}
			}
		}
		
		private bool ProcessChangeBuyLimit(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			bool isFilled = false;
			if (price <= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price < order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
			}
			return isFilled;
		}

		private bool ProcessChangeSellMarket(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			CreateLogicalFillHelper(getActualPosition() - order.Positions, price, tick.Time, order);
			CancelEnterOrders(order.Strategy);
			return true;
		}

		private bool ProcessChangeSellLimit(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			bool isFilled = false;
			if (price >= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price > order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				CreateLogicalFillHelper(-order.Positions, price, tick.Time, order);
				CancelEnterOrders(order.Strategy);
			}
			return isFilled;
		}

		#endregion
		
		public Func<LogicalOrder, double, double, int> DrawTrade {
			get { return drawTrade; }
			set { drawTrade = value; }
		}
		
		public bool UseSyntheticLimits {
			get { return useSyntheticLimits; }
			set { useSyntheticLimits = value; }
		}
		
		public bool UseSyntheticStops {
			get { return useSyntheticStops; }
			set { useSyntheticStops = value; }
		}
		
		public bool UseSyntheticMarkets {
			get { return useSyntheticMarkets; }
			set { useSyntheticMarkets = value; }
		}
		
		public SymbolInfo Symbol {
			get { return symbol; }
			set { symbol = value; }
		}
		
		public bool DoStrategyOrders {
			get { return doStrategyOrders; }
			set { doStrategyOrders = value; }
		}
		
		public bool DoExitStrategyOrders {
			get { return doExitStrategyOrders; }
			set { doExitStrategyOrders = value; }
		}
		
		public bool GraphTrades {
			get { return graphTrades; }
			set { graphTrades = value; }
		}
		
		public Action<LogicalFillBinary> CreateLogicalFill {
			get { return createLogicalFill; }
			set { createLogicalFill = value; }
		}
	}
}
