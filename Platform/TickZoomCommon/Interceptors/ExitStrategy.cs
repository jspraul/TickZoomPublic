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
using System.ComponentModel;
using System.Drawing;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	public class ExitStrategy : StrategySupport
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(ExitStrategy));
		private bool controlStrategy = false;
		private double strategySignal = 0;
		private LogicalOrder buyStopLossOrder;
		private LogicalOrder sellStopLossOrder;
		private LogicalOrder breakEvenBuyStopOrder;
		private LogicalOrder breakEvenSellStopOrder;
		private LogicalOrder marketOrder;
		private double stopLoss = 0;
		private double targetProfit = 0;
		private double breakEven = 0;
		private double entryPrice = 0;
		private double trailStop = 0;
		private double dailyMaxProfit = 0;
		private double dailyMaxLoss = 0;
		private double weeklyMaxProfit = 0;
		private double weeklyMaxLoss = 0;
		private double monthlyMaxProfit = 0;
		private double monthlyMaxLoss = 0;
		private double breakEvenStop = 0;
		private double pnl = 0;
		private double maxPnl = 0;
		bool stopTradingToday = false;
		bool stopTradingThisWeek = false;
		bool stopTradingThisMonth = false;
		PositionCommon position;
		
		public ExitStrategy(Strategy strategy) : base( strategy) {
			position = new PositionCommon(strategy);
			int OptimizeTickEvent = 0;
			strategy.RequestEvent(EventType.Tick);
		}
		
		EventContext context;
		public override void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			if( eventType == EventType.Initialize) {
				Strategy.AddInterceptor( EventType.Tick, this);
				Strategy.AddInterceptor( EventType.LogicalFill, this);
				OnInitialize();
			}
			context.Invoke();
			this.context = context;
			if( eventType == EventType.Tick ||
			    eventType == EventType.LogicalFill) {
				OnProcessPosition();
			}
		}
				
		public void OnInitialize()
		{
			marketOrder = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			marketOrder.TradeDirection = TradeDirection.ExitStrategy;
			marketOrder.Tag = "ExitStrategy" ;
			Strategy.AddOrder(marketOrder);
			breakEvenBuyStopOrder = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			breakEvenBuyStopOrder.TradeDirection = TradeDirection.ExitStrategy;
			breakEvenBuyStopOrder.Type = OrderType.BuyStop;
			breakEvenBuyStopOrder.Tag = "ExitStrategy" ;
			Strategy.AddOrder(breakEvenBuyStopOrder);
			breakEvenSellStopOrder = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			breakEvenSellStopOrder.TradeDirection = TradeDirection.ExitStrategy;
			breakEvenSellStopOrder.Type = OrderType.SellStop;
			breakEvenSellStopOrder.Tag = "ExitStrategy" ;
			Strategy.AddOrder(breakEvenSellStopOrder);
			buyStopLossOrder = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			buyStopLossOrder.TradeDirection = TradeDirection.ExitStrategy;
			buyStopLossOrder.Type = OrderType.BuyStop;
			buyStopLossOrder.Tag = "ExitStrategy" ;
			Strategy.AddOrder(buyStopLossOrder);
			sellStopLossOrder = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			sellStopLossOrder.TradeDirection = TradeDirection.ExitStrategy;
			sellStopLossOrder.Type = OrderType.SellStop;
			sellStopLossOrder.Tag = "ExitStrategy" ;
			Strategy.AddOrder(sellStopLossOrder);
//			log.WriteFile( LogName + " chain = " + Chain.ToChainString());
			if( IsTrace) Log.Trace(Strategy.FullName+".Initialize()");
			Strategy.Drawing.Color = Color.Black;
		}
		
		public void OnProcessPosition() {
			Tick tick = Strategy.Data.Ticks[0];
			// Handle ActiveNow orders.
			
			if( stopTradingToday || stopTradingThisWeek || stopTradingThisMonth ) {
				return; 
			}
			
			if( (strategySignal>0) != context.Position.IsLong || (strategySignal<0) != context.Position.IsShort ) {
				strategySignal = context.Position.Current;
				entryPrice = context.Position.Price;
				maxPnl = 0;
				position.Copy(context.Position);
				trailStop = 0;
				breakEvenStop = 0;
				CancelOrders();
			} 
			
			if( position.HasPosition ) {
				// copy signal in case of increased position size
				double exitPrice;
				if( strategySignal > 0) {
					exitPrice = tick.IsQuote ? tick.Bid : tick.Price;
					pnl = (exitPrice - entryPrice).Round();
				} else {
					exitPrice = tick.IsQuote ? tick.Ask : tick.Price;
					pnl = (entryPrice - exitPrice).Round();
				}
				maxPnl = pnl > maxPnl ? pnl : maxPnl;
				if( stopLoss > 0) processStopLoss(tick);
				if( trailStop > 0) processTrailStop(tick);
				if( breakEven > 0) processBreakEven(tick);
				if( targetProfit > 0) processTargetProfit(tick);
				if( dailyMaxProfit > 0) processDailyMaxProfit(tick);
				if( dailyMaxLoss > 0) processDailyMaxLoss(tick);
				if( weeklyMaxProfit > 0) processWeeklyMaxProfit(tick);
				if( weeklyMaxLoss > 0) processWeeklyMaxLoss(tick);
				if( monthlyMaxProfit > 0) processMonthlyMaxProfit(tick);
				if( monthlyMaxLoss > 0) processMonthlyMaxLoss(tick);
			}
			
			context.Position.Copy(position);
		}
	
		private void processDailyMaxProfit(Tick tick) {
			if( Strategy.Performance.Equity.ProfitToday >= dailyMaxProfit) {
				stopTradingToday = true;
				LogExit("DailyMaxProfit Exit at " + dailyMaxProfit);
				flattenSignal(tick,"Daily Profit Target");
			}
		}
		
		private void processDailyMaxLoss(Tick tick) {
			if( Strategy.Performance.Equity.ProfitToday >= dailyMaxLoss) {
				stopTradingToday = true;
				LogExit("DailyMaxLoss Exit at " + dailyMaxLoss);
				flattenSignal(tick,"Daily Stop Loss");
			}
		}
		
		private void processWeeklyMaxProfit(Tick tick) {
			if( Strategy.Performance.Equity.ProfitForWeek >= weeklyMaxProfit) {
				stopTradingThisWeek = true;
				LogExit("WeeklyMaxProfit Exit at " + weeklyMaxProfit);
				flattenSignal(tick,"Weekly Profit Target");
			}
		}
		
		private void processWeeklyMaxLoss(Tick tick) {
			if( - Strategy.Performance.Equity.ProfitForWeek >= weeklyMaxLoss) {
				stopTradingThisWeek = true;
				LogExit("WeeklyMaxLoss Exit at " + weeklyMaxLoss);
				flattenSignal(tick,"Weekly Stop Loss");
			}
		}
		
		private void processMonthlyMaxProfit(Tick tick) {
			if( Strategy.Performance.Equity.ProfitForMonth >= monthlyMaxProfit) {
				stopTradingThisMonth = true;
				LogExit("MonthlyMaxProfit Exit at " + monthlyMaxProfit);
				flattenSignal(tick,"Monthly Profit Target");
			}
		}
		
		private void processMonthlyMaxLoss(Tick tick) {
			if( - Strategy.Performance.Equity.ProfitForMonth >= monthlyMaxLoss) {
				stopTradingThisMonth = true;
				LogExit("MonthlyMaxLoss Exit at " + monthlyMaxLoss);
				flattenSignal(tick,"Monthly Stop Loss");
			}
		}
		
		private void CancelOrders() {
			marketOrder.Status = OrderStatus.AutoCancel;
			breakEvenBuyStopOrder.Status = OrderStatus.AutoCancel;
			breakEvenSellStopOrder.Status = OrderStatus.AutoCancel;
			buyStopLossOrder.Status = OrderStatus.AutoCancel;
			sellStopLossOrder.Status = OrderStatus.AutoCancel;
		}
		
		private void flattenSignal(Tick tick, string tag) {
			marketOrder.Tag = tag;
			flattenSignal(marketOrder,tick);
		}
		
		private void flattenSignal(LogicalOrder order, Tick tick) {
// Actual fills of exit strategy orders now handles via the OrderManager interceptor.
//            if (Strategy.Performance.GraphTrades)
//            {
//				double fillPrice = 0;
//				if( position.IsLong) {
//					order.Positions = context.Position.Size;
//					fillPrice = tick.Bid;
//				}
//				if( position.IsShort) {
//					order.Positions = context.Position.Size;
//					fillPrice = tick.Ask;
//				}
//                Strategy.Chart.DrawTrade(order, fillPrice, 0);
//            }
//            position.Change(0);
//			CancelOrders();
//			if( controlStrategy) {
//				Strategy.Orders.Exit.ActiveNow.GoFlat();
//				strategySignal = 0;
//			}
		}
	
		private void processTargetProfit(Tick tick) {
			if( pnl >= targetProfit) {
				LogExit("TargetProfit Exit at " + targetProfit);
				flattenSignal(tick,"Target Profit");
			}
		}
		
		private void processStopLoss(Tick tick) {
			if( position.IsShort) {
				buyStopLossOrder.Price = entryPrice + stopLoss;
				buyStopLossOrder.Status = OrderStatus.Active;
			} else {
				buyStopLossOrder.Status = OrderStatus.Inactive;
			}
			if( position.IsLong) {
				sellStopLossOrder.Price = entryPrice - stopLoss;
				sellStopLossOrder.Status = OrderStatus.Active;
			} else {
				sellStopLossOrder.Status = OrderStatus.Inactive;
			}
		}
		
		private void processTrailStop(Tick tick) {
			if( maxPnl - pnl >= trailStop) {
				LogExit("TailStop Exit at " + trailStop);
				flattenSignal(tick,"Trail Stop");
			}
		}
		
		public bool OnIntervalOpen(Interval interval) {
			if( interval.Equals(Intervals.Day1)) {
				stopTradingToday = false;
			}
			if( interval.Equals(Intervals.Week1)) {
				stopTradingThisWeek = false;
			}
			if( interval.Equals(Intervals.Month1)) {
				stopTradingThisMonth = false;
			}
			return true;
		}
	
		private void processBreakEven(Tick tick) {
			if( pnl >= breakEven) {
				if( position.IsLong ) {
					if( !breakEvenSellStopOrder.IsActive) {
						breakEvenSellStopOrder.Price = entryPrice + breakEvenStop;
						breakEvenSellStopOrder.Status = OrderStatus.Active;
					}
				} else {
					breakEvenSellStopOrder.Status = OrderStatus.Inactive;
				}
					
				if( position.IsShort ) {
					if( !breakEvenBuyStopOrder.IsActive) {
						breakEvenBuyStopOrder.Price = entryPrice - breakEvenStop;
						breakEvenBuyStopOrder.Status = OrderStatus.Active;
					}
				} else {
					breakEvenBuyStopOrder.Status = OrderStatus.Inactive;
				}
			}
			if( breakEvenBuyStopOrder.IsActive && pnl <= breakEvenStop) {
				LogExit("Break Even Exit at " + breakEvenStop);
				flattenSignal(breakEvenBuyStopOrder,tick);
			}
			if( breakEvenSellStopOrder.IsActive && pnl <= breakEvenStop) {
				LogExit("Break Even Exit at " + breakEvenStop);
				flattenSignal(breakEvenSellStopOrder,tick);
			}
		}
		
		private void LogExit(string description) {
			if( Strategy.Chart.IsDynamicUpdate) {
				if( IsDebug) Log.Debug(Strategy.Ticks[0].Time + ", Bar="+Strategy.Chart.ChartBars.CurrentBar+", " + description);
			} else if( !Strategy.IsOptimizeMode) {
				if( IsDebug) Log.Debug(Strategy.Ticks[0].Time + ", Bar="+Strategy.Chart.ChartBars.CurrentBar+", " + description);
			}
		}

        #region Properties
        
        [DefaultValue(0d)]
        public double StopLoss
        {
            get { return stopLoss; }
            set { // log.WriteFile(GetType().Name+".StopLoss("+value+")");
            	  stopLoss = Math.Max(0, value); }
        }		

        [DefaultValue(0d)]
		public double TrailStop
        {
            get { return trailStop; }
            set { trailStop = Math.Max(0, value); }
        }		
		
        [DefaultValue(0d)]
		public double TargetProfit
        {
            get { return targetProfit; }
            set { if( IsTrace) Log.Trace(GetType().Name+".TargetProfit("+value+")");
            	  targetProfit = Math.Max(0, value); }
        }		
		
        [DefaultValue(0d)]
		public double BreakEven
        {
            get { return breakEven; }
            set { breakEven = Math.Max(0, value); }
        }	
		
        [DefaultValue(false)]
		public bool ControlStrategy {
			get { return controlStrategy; }
			set { controlStrategy = value; }
		}
		
        [DefaultValue(0d)]
		public double WeeklyMaxProfit {
			get { return weeklyMaxProfit; }
			set { weeklyMaxProfit = value; }
		}
		
        [DefaultValue(0d)]
		public double WeeklyMaxLoss {
			get { return weeklyMaxLoss; }
			set { weeklyMaxLoss = value; }
		}
		
        [DefaultValue(0d)]
		public double DailyMaxProfit {
			get { return dailyMaxProfit; }
			set { dailyMaxProfit = value; }
		}
		
        [DefaultValue(0d)]
		public double DailyMaxLoss {
			get { return dailyMaxLoss; }
			set { dailyMaxLoss = value; }
		}
		
        [DefaultValue(0d)]
		public double MonthlyMaxLoss {
			get { return monthlyMaxLoss; }
			set { monthlyMaxLoss = value; }
		}
		
        [DefaultValue(0d)]
		public double MonthlyMaxProfit {
			get { return monthlyMaxProfit; }
			set { monthlyMaxProfit = value; }
		}
		#endregion
	
//		public override string ToString()
//		{
//			return Strategy.FullName;
//		}
		
		public PositionCommon Position {
			get { return position; }
			set { position = value; }
		}
	}

	[Obsolete("Please use ExitStrategy instead.",true)]
	public class ExitStrategyCommon : ExitStrategy
	{
		public ExitStrategyCommon(Strategy strategy) : base( strategy) {
			
		}
	}
		
}
