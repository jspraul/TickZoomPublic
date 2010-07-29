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
		private static readonly Log Log = Factory.SysLog.GetLogger(typeof(FillSimulatorDefault));
		private static readonly bool IsTrace = Log.IsTraceEnabled;
		private static readonly bool IsDebug = Log.IsDebugEnabled;
		private static readonly bool IsNotice = Log.IsNoticeEnabled;
		private IList<LogicalOrder> activeOrders;
		private double position;
		private Action<SymbolInfo, LogicalFill> changePosition;
		private Action<LogicalFillBinary> createLogicalFill;
		private Func<LogicalOrder, double, double, int> drawTrade;
		private bool useSyntheticMarkets = true;
		private bool useSyntheticStops = true;
		private bool useSyntheticLimits = true;
		private bool doReverseOrders = true;
		private bool doEntryOrders = true;
		private bool doExitOrders = true;
		private bool doExitStrategyOrders = false;
		private SymbolInfo symbol;
		private bool allowReversal = true;
		private bool graphTrades = false;
		
		public FillSimulatorDefault()
		{
		}

		public FillSimulatorDefault(StrategyInterface strategyInterface)
		{
			Strategy strategy = (Strategy) strategyInterface;
			graphTrades = strategy.Performance.GraphTrades;
		}
		
		public void ProcessOrders(Tick tick, IList<LogicalOrder> orders, double position)
		{
			if( changePosition == null) {
				throw new ApplicationException("Please set the ChangePosition property with your callback method for simulated fills.");
			}
			if( symbol == null) {
				throw new ApplicationException("Please set the Symbol property for the " + GetType().Name + ".");
			}
			this.position = position;
			this.activeOrders = orders;
			for(int i=0; i<activeOrders.Count; i++) {
				LogicalOrder order = activeOrders[i];
				if (order.IsActive) {
					if (doEntryOrders && order.TradeDirection == TradeDirection.Entry) {
						OnProcessEnterOrder(order, tick);
					}
					if (doExitOrders && order.TradeDirection == TradeDirection.Exit) {
						OnProcessExitOrder(order, tick);
					}
					if (doReverseOrders && order.TradeDirection == TradeDirection.Reverse) {
						OnProcessReverseOrder(order, tick);
					}
					if (doExitStrategyOrders && order.TradeDirection == TradeDirection.ExitStrategy) {
						OnProcessExitOrder(order, tick);
					}
				}
			}
		}
		
#region ExitOrder

		private void OnProcessExitOrder(LogicalOrder order, Tick tick)
		{
			if (IsTrace)
				Log.Trace("OnProcessEnterOrder()");
			if (IsLong) {
				if (order.Type == OrderType.BuyStop || order.Type == OrderType.BuyLimit) {
					order.IsActive = false;
				}
			}
			if (IsShort) {
				 if (order.Type == OrderType.SellStop || order.Type == OrderType.SellLimit) {
					order.IsActive = false;
				}
			}

			if (IsLong) {
				switch (order.Type) {
					case OrderType.SellMarket:
						if (useSyntheticMarkets) {
							ProcessSellMarket(order, tick);
						}
						break;
					case OrderType.SellStop:
						if (useSyntheticStops) {
							ProcessSellStop(order, tick);
						}
						break;
					case OrderType.SellLimit:
						if (useSyntheticLimits) {
							ProcessSellLimit(order, tick);
						}
						break;
				}
			}

			if (IsShort) {
				switch (order.Type) {
					case OrderType.BuyMarket:
						if (useSyntheticMarkets) {
							ProcessBuyMarket(order, tick);
						}
						break;
					case OrderType.BuyStop:
						if (useSyntheticStops) {
							ProcessBuyStop(order, tick);
						}
						break;
					case OrderType.BuyLimit:
						if (useSyntheticLimits) {
							ProcessBuyLimit(order, tick);
						}
						break;
				}
			}
		}

		private void OnProcessReverseOrder(LogicalOrder order, Tick tick)
		{
			if (IsTrace)
				Log.Trace("OnProcessEnterOrder()");
			if (IsLong) {
				if (order.Type == OrderType.BuyStop || order.Type == OrderType.BuyLimit) {
					order.IsActive = false;
				}
			}
			if (IsShort) {
				 if (order.Type == OrderType.SellStop || order.Type == OrderType.SellLimit) {
					order.IsActive = false;
				}
			}

			if (IsLong) {
				switch (order.Type) {
					case OrderType.SellMarket:
						if (useSyntheticMarkets) {
							ProcessReverseSellMarket(order, tick);
						}
						break;
					case OrderType.SellStop:
						if (useSyntheticStops) {
							ProcessReverseSellStop(order, tick);
						}
						break;
					case OrderType.SellLimit:
						if (useSyntheticLimits) {
							ProcessReverseSellLimit(order, tick);
						}
						break;
				}
			}

			if (IsShort) {
				switch (order.Type) {
					case OrderType.BuyMarket:
						if (useSyntheticMarkets) {
							ProcessReverseBuyMarket(order, tick);
						}
						break;
					case OrderType.BuyStop:
						if (useSyntheticStops) {
							ProcessReverseBuyStop(order, tick);
						}
						break;
					case OrderType.BuyLimit:
						if (useSyntheticLimits) {
							ProcessReverseBuyLimit(order, tick);
						}
						break;
				}
			}
		}
		
		private void FlattenPosition(double price, Tick tick, LogicalOrder order)
		{
			CreateLogicalFillHelper(0,price,tick.Time,order);
			CancelExitOrders(order.TradeDirection);
		}

		private void ModifyPosition( double position, double price, TimeStamp time, LogicalOrder order) {
			CreateLogicalFillHelper(position, price, time, order);
			CancelEnterOrders();
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
		}
		
		public void CancelReverseOrders() {
			for (int i = activeOrders.Count - 1; i >= 0; i--) {
				LogicalOrder order = activeOrders[i];
				if (order.TradeDirection == TradeDirection.Exit ||
				   order.TradeDirection == TradeDirection.Reverse) {
					order.IsActive = false;
				}
			}
		}
		
		public void CancelExitOrders(TradeDirection tradeDirection)
		{
			for (int i = activeOrders.Count - 1; i >= 0; i--) {
				LogicalOrder order = activeOrders[i];
				if (order.TradeDirection == tradeDirection) {
					order.IsActive = false;
				}
			}
		}

		private void TryDrawTrade(LogicalOrder order, double price, double position) {
			if (drawTrade != null && graphTrades == true) {
				drawTrade(order, price, position);
			}
		}

		private void ProcessBuyMarket(LogicalOrder order, Tick tick)
		{
			LogMsg("Buy Market Exit at " + tick);
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			FlattenPosition(price, tick, order);
//			TryDrawTrade(order, price, position);
		}
		
		private void ProcessSellMarket(LogicalOrder order, Tick tick)
		{
			LogMsg("Sell Market Exit at " + tick);
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			FlattenPosition(price, tick, order);
//			TryDrawTrade(order, price, position);
		}

		private void ProcessBuyStop(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price >= order.Price) {
				LogMsg("Buy Stop Exit at " + tick);
				FlattenPosition(price, tick, order);
//				TryDrawTrade(order, price, position);
			}
		}

		private void ProcessBuyLimit(LogicalOrder order, Tick tick)
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
				LogMsg("Buy Limit Exit at " + tick);
				FlattenPosition(price, tick, order);
//				TryDrawTrade(order, price, position);
			}
		}

		private void ProcessSellStop(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			if (price <= order.Price) {
				LogMsg("Sell Stop Exit at " + tick);
				FlattenPosition(price, tick, order);
//				TryDrawTrade(order, price, position);
			}
		}

		private void ProcessSellLimit(LogicalOrder order, Tick tick)
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
				LogMsg("Sell Stop Limit at " + tick);
				FlattenPosition(price, tick, order);
//				TryDrawTrade(order, price, position);
			}
		}

		private void ProcessReverseBuyMarket(LogicalOrder order, Tick tick)
		{
			LogMsg("Buy Market Exit at " + tick);
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			ReversePosition( price, tick, order);
		}
		
		private void ProcessReverseSellMarket(LogicalOrder order, Tick tick)
		{
			LogMsg("Sell Market Exit at " + tick);
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			ReversePosition( price, tick, order);
		}

		private void ProcessReverseBuyStop(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price >= order.Price) {
				LogMsg("Buy Stop Exit at " + tick);
				ReversePosition( price, tick, order);
			}
		}

		private void ProcessReverseBuyLimit(LogicalOrder order, Tick tick)
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
				LogMsg("Buy Limit Reverse at " + tick);
				ReversePosition( price, tick, order);
			}
		}

		private void ProcessReverseSellStop(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Bid;
			if (price <= order.Price) {
				LogMsg("Sell Stop Exit at " + tick);
				ReversePosition( price, tick, order);
			}
		}

		private void ProcessReverseSellLimit(LogicalOrder order, Tick tick)
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
				LogMsg("Sell Stop Limit at " + tick);
				ReversePosition( price, tick, order);
			}
		}
		
#endregion
		

#region EntryOrders

		private void OnProcessEnterOrder(LogicalOrder order, Tick tick)
		{
			if (IsTrace)
				Log.Trace("OnProcessEnterOrder()");
			if (IsFlat || (allowReversal && IsShort)) {
				if (order.Type == OrderType.BuyMarket && useSyntheticMarkets) {
					ProcessEnterBuyMarket(order, tick);
				}
				if (order.Type == OrderType.BuyStop && useSyntheticStops) {
					ProcessEnterBuyStop(order, tick);
				}
				if (order.Type == OrderType.BuyLimit && useSyntheticLimits) {
					ProcessEnterBuyLimit(order, tick);
				}
			}

			if (IsFlat || (allowReversal && IsLong)) {
				if (order.Type == OrderType.SellMarket && useSyntheticMarkets) {
					ProcessEnterSellMarket(order, tick);
				}
				if (order.Type == OrderType.SellStop && useSyntheticStops) {
					ProcessEnterSellStop(order, tick);
				}
				if (order.Type == OrderType.SellLimit && useSyntheticLimits) {
					ProcessEnterSellLimit(order, tick);
				}
			}
		}

		private void ProcessEnterBuyStop(LogicalOrder order, Tick tick)
		{
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			if (price >= order.Price) {
				LogMsg("Long Stop Entry at " + tick);
				
				CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
//				TryDrawTrade(order, price, order.Positions);
				CancelEnterOrders();
			}
		}

		private void ProcessEnterSellStop(LogicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price <= order.Price) {
				LogMsg("Short Stop Entry at " + tick);
				CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
//				TryDrawTrade(order, price, order.Positions);
				CancelEnterOrders();
			}
		}

		private void LogMsg(string description)
		{
		}

		private void ProcessEnterBuyMarket(LogicalOrder order, Tick tick)
		{
			LogMsg("Long Market Entry at " + tick);
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
//			TryDrawTrade(order, price, order.Positions);
			CancelEnterOrders();
		}

		public void CancelEnterOrders()
		{
			for (int i = activeOrders.Count - 1; i >= 0; i--) {
				LogicalOrder order = activeOrders[i];
				if (order.TradeDirection == TradeDirection.Entry) {
					order.IsActive = false;
				}
			}
		}
		
		private void CreateLogicalFillHelper(double position, double price, TimeStamp time, LogicalOrder order) {
			LogicalFillBinary fill = new LogicalFillBinary(position,price,time,order.Id);
			createLogicalFill(fill);
		}
		
		private void ProcessEnterBuyLimit(LogicalOrder order, Tick tick)
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
				LogMsg("Long Limit Entry at " + tick);
				CreateLogicalFillHelper(order.Positions, price, tick.Time, order);
//				TryDrawTrade(order, price, order.Positions);
				CancelEnterOrders();
			}
		}

		private void ProcessEnterSellMarket(LogicalOrder order, Tick tick)
		{
			LogMsg("Short Market Entry at " + tick);
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			CreateLogicalFillHelper(-order.Positions, price, tick.Time, order);
//			TryDrawTrade(order, price, -order.Positions);
			CancelEnterOrders();
		}

		private void ProcessEnterSellLimit(LogicalOrder order, Tick tick)
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
				LogMsg("Short Limit Entry at " + tick);
				CreateLogicalFillHelper(-order.Positions, price, tick.Time, order);
//				TryDrawTrade(order, price, -order.Positions);
				CancelEnterOrders();
			}
		}
		
		public void ProcessFill(StrategyInterface strategyInterface, LogicalFill fill) {
			if( IsDebug) Log.Debug( "Considering fill: " + fill + " for strategy " + strategyInterface);
			Strategy strategy = (Strategy) strategyInterface;
			bool cancelAllEntries = false;
			bool cancelAllExits = false;
			bool cancelAllExitStrategies = false;
			int orderId = fill.OrderId;
			LogicalOrder filledOrder = null;
			foreach( var order in strategy.ActiveOrders) {
				if( order.Id == orderId) {
					if( IsDebug) Log.Debug( "Matched fill with orderId: " + orderId);
					if( order.TradeDirection == TradeDirection.Entry && !doEntryOrders) {
						if( IsDebug) Log.Debug( "Skipping fill, entry order fills disabled.");
						return;
					}
					if( order.TradeDirection == TradeDirection.Exit && !doExitOrders) {
						if( IsDebug) Log.Debug( "Skipping fill, exit order fills disabled.");
						return;
					}
					if( order.TradeDirection == TradeDirection.Reverse && !doExitOrders) {
						if( IsDebug) Log.Debug( "Skipping fill, reverse order fills disabled.");
						return;
					}
					if( order.TradeDirection == TradeDirection.ExitStrategy && !doExitStrategyOrders) {
						if( IsDebug) Log.Debug( "Skipping fill, exit strategy orders fills disabled.");
						return;
					}
					filledOrder = order;
					TryDrawTrade(order, fill.Price, fill.Position);
					if( IsDebug) Log.Debug( "Changing position because of fill");
					changePosition(strategy.Data.SymbolInfo,fill);
				}
			}
			if( filledOrder != null) {
				bool clean = false;
				if( filledOrder.TradeDirection == TradeDirection.Entry &&
				   doEntryOrders ) {
					cancelAllEntries = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.Exit &&
				   doExitOrders ) {
					cancelAllExits = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.ExitStrategy &&
				   doExitStrategyOrders ) {
					cancelAllExitStrategies = true;
					clean = true;
				}
				if( clean) {
					strategy.RefreshActiveOrders();
					foreach( var order in strategy.ActiveOrders) {
						if( order.TradeDirection == TradeDirection.Entry && cancelAllEntries) {
							order.IsActive = false;
						}
						if( order.TradeDirection == TradeDirection.Exit && cancelAllExits) {
							order.IsActive = false;
						}
						if( order.TradeDirection == TradeDirection.ExitStrategy && cancelAllExitStrategies) {
							order.IsActive = false;
						}
					}
				}
			}
		}
		

		#endregion

		private bool IsFlat {
			get { return position == 0; }
		}

		private bool IsShort {
			get { return position < 0; }
		}

		private bool IsLong {
			get { return position > 0; }
		}
		
		public Func<LogicalOrder, double, double, int> DrawTrade {
			get { return drawTrade; }
			set { drawTrade = value; }
		}
		
		public Action<SymbolInfo, LogicalFill> ChangePosition {
			get { return changePosition; }
			set { changePosition = value; }
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
		
		public bool DoEntryOrders {
			get { return doEntryOrders; }
			set { doEntryOrders = value; }
		}
		
		public bool DoExitOrders {
			get { return doExitOrders; }
			set { doExitOrders = value; }
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
