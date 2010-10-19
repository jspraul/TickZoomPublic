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
using TickZoom.Interceptors;
using TickZoom.Reports;
using TickZoom.Transactions;

namespace TickZoom.Statistics
{
	/// <summary>
	/// Description of MoneyManagerSupport.
	/// </summary>
	public class Equity : StrategyInterceptor
	{
		TransactionPairs daily;
		TransactionPairs weekly;
		TransactionPairs monthly;
		TransactionPairs yearly;
		TransactionPairsBinary dailyBinary;
		TransactionPairsBinary weeklyBinary;
		TransactionPairsBinary monthlyBinary;
		TransactionPairsBinary yearlyBinary;
		double closedEquity = 0;
		double openEquity = 0;
		Model model;
		Performance performance;
		double startingEquity = 10000;
		bool graphEquity = false;
		IndicatorCommon equity;
		ProfitLoss equityProfitLoss;
		bool isMultiSymbolPortfolio;
		PortfolioInterface portfolio;
		bool enableYearlyStats = false;
		bool enableMonthlyStats = false;
		bool enableWeeklyStats = false;
		bool enableDailyStats = false;
		bool isInitialized = false;
		
		public Equity(Model model, Performance performance) 
		{
			this.model = model;
			this.performance = performance;
			equityProfitLoss = new ProfitLossEquity();
		}
		
		public override void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			if( EventType.Initialize == eventType) {
				OnInitialize();
			}
			context.Invoke();
			if( EventType.Initialize == eventType) {
				OnPostInitialize();
			}
			if( EventType.OpenInterval == eventType) {
				OnIntervalOpen((Interval)eventDetail);
			} else if( EventType.Close == eventType) {
				OnIntervalClose();
			} else if( EventType.CloseInterval == eventType) {
				OnIntervalClose((Interval)eventDetail);
			}
		}

		public void OnInitialize()
		{
			dailyBinary = new TransactionPairsBinary(model.Context.TradeData);
			portfolio = model as PortfolioInterface;
			if( portfolio != null && portfolio.PortfolioType == PortfolioType.MultiSymbol) {
				isMultiSymbolPortfolio = true; // portfolio.PortfolioType == PortfolioType.MultiSymbol;
			}
			model.AddInterceptor( EventType.OpenInterval, this);
			model.AddInterceptor( EventType.Close, this);
			model.AddInterceptor( EventType.CloseInterval, this);
		}
		
		public void OnPostInitialize() {
			closedEquity = startingEquity;
			daily  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,dailyBinary);
			dailyBinary.Name = "Daily";
			weeklyBinary  = new TransactionPairsBinary(model.Context.TradeData);
			weekly  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,weeklyBinary);
			weeklyBinary.Name = "Weekly";
			monthlyBinary = new TransactionPairsBinary(model.Context.TradeData);
			monthly  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,monthlyBinary);
			monthlyBinary.Name = "Monthly";
			yearlyBinary = new TransactionPairsBinary(model.Context.TradeData);
			yearly  = new TransactionPairs(GetCurrentEquity,equityProfitLoss,yearlyBinary);
			yearlyBinary.Name = "Yearly";
			
			if( graphEquity) {
				equity = new IndicatorCommon();
				equity.Drawing.IsVisible = true;
				equity.Drawing.PaneType = PaneType.Secondary;
				equity.Drawing.GraphType = GraphType.FilledLine;
				equity.Drawing.Color = Color.Green;
				equity.Drawing.GroupName = "SimpleEquity";
				equity.Name = "SimpleEquity";
				model.AddIndicator(equity);
			}
			
			if( enableYearlyStats) {
				model.RequestUpdate(Intervals.Year1);
			}
			if( enableMonthlyStats) {
				model.RequestUpdate(Intervals.Month1);
			}
			if( enableWeeklyStats) {
				model.RequestUpdate(Intervals.Week1);
			}
			if( enableDailyStats) {
				model.RequestUpdate(Intervals.Day1);
			}
			
			isInitialized = true;
		}
		
		public void OnChangeClosedEquity(double profitLoss) {
			closedEquity += profitLoss;
		}
		
		public void OnUpdateOpenEquity(double openEquity) {
			this.openEquity = openEquity;
		}
		
		public bool OnIntervalOpen(Interval interval)
		{
			TimeStamp dt = model.Ticks[0].Time;
			if( dailyBinary.Count == 0) CalcNew(dailyBinary);
			if( weeklyBinary.Count == 0) CalcNew(weeklyBinary);
			if( monthlyBinary.Count == 0) CalcNew(monthlyBinary);
			if( yearlyBinary.Count == 0) CalcNew(yearlyBinary);
			return true;
		}

		public bool OnIntervalClose(Interval interval)
		{
			var tick = model.Ticks[0];
			if( interval.Equals(Intervals.Day1)) {
				CalcEnd(dailyBinary);
				CalcNew(dailyBinary);
			} else if( interval.Equals(Intervals.Week1)) {
				CalcEnd(weeklyBinary);
				CalcNew(weeklyBinary);
			} else if( interval.Equals(Intervals.Month1)) {
				CalcEnd(monthlyBinary);
				CalcNew(monthlyBinary);
			} else if( interval.Equals(Intervals.Year1)) {
				CalcEnd(yearlyBinary);
				CalcNew(yearlyBinary);
			}
			return true;
		}
		
		TimeStamp dbgTime = new TimeStamp("2008-01-02 02:58:00");
		public bool OnIntervalClose()
		{
			if( graphEquity) {
				equity[0] = (long) CurrentEquity;
			}
			return true;
		}
		
		public double CurrentPrice(double direction) {
			return CurrentEquity;
		}

		void CalcNew(TransactionPairsBinary periodTrades) {
			TransactionPairBinary trade = TransactionPairBinary.Create();
			trade.Enter(1, CurrentEquity, model.Ticks[0].Time, model.Ticks[0].Time, 0, 0, 0);
			periodTrades.Add(trade);
		}

		void CalcEnd(TransactionPairsBinary periodTrades) {
			if( periodTrades.Count>0) {
				TransactionPairBinary pair = periodTrades[periodTrades.Count - 1];
				pair.Exit(CurrentEquity, model.Ticks[0].Time, model.Ticks[0].Time, 0, 0, 0);
				periodTrades[periodTrades.Count - 1] = pair;
			}
		}
		
		public bool WriteReport(string name, string folder, StrategyStats strategyStats) {
			EquityStatsReport equityStats = new EquityStatsReport(this);
			equityStats.StrategyStats = strategyStats;
			equityStats.WriteReport(name,folder);
			return true;
		}

		[Obsolete("Use Performance.ProfitLossCalculation.Slippage or create your own ProfitLossCalculation instead.",true)]
		public double Slippage {
			get { return 0.0D; }
			set {  }
		}
		
		[Obsolete("Use Performance.ProfitLossCalculation.Slippage or create your own ProfitLossCalculation instead.",true)]
		public double Commission {
			get { return 0.0D; }
			set {  }
		}
		
		public double GetCurrentEquity(double direction) {
			return CurrentEquity;
		}

		[Obsolete("Use Daily instead.",true)]
		public TransactionPairs CompletedDaily {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,dailyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}

		[Obsolete("Use Weekly instead.",true)]
		public TransactionPairs CompletedWeekly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,weeklyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}

		[Obsolete("Use Monthly instead.",true)]
		public TransactionPairs CompletedMonthly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,monthlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
	
		[Obsolete("Use Yearly instead.",true)]
		public TransactionPairs CompletedYearly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,yearlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		public TransactionPairs Daily {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,dailyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		public TransactionPairs Weekly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,weeklyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public TransactionPairs Monthly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,monthlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public TransactionPairs Yearly {
			get { if( model.Ticks.Count>0) {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss,yearlyBinary.GetCompletedList(model.Ticks[0].Time,CurrentEquity,model.Bars.BarCount));
				} else {
					return new TransactionPairs(GetCurrentEquity,equityProfitLoss);
				}
			}
		}
		
		[Browsable(false)]
		public double ProfitToday {
			get { 
				if( dailyBinary.Count > 0) {
			         return CurrentEquity - dailyBinary.Tail.EntryPrice;
				} else {
					 return CurrentEquity;
				}
			}
		}
		
		[Browsable(false)]
		public double ProfitForWeek {
			get { 
				if( weeklyBinary.Count > 0) {
			         return CurrentEquity - weeklyBinary.Tail.EntryPrice;
				} else {
					 return CurrentEquity;
				}
			}
		}

		[Browsable(false)]
		public double ProfitForMonth {
			get { 
				if( monthlyBinary.Count > 0) {
			         return CurrentEquity - monthlyBinary.Tail.EntryPrice;
				} else {
					 return CurrentEquity;
				}
			}
		}
		
		public double StartingEquity {
			get { return startingEquity; }
			set { if( startingEquity <= 0D) {
					throw new ApplicationException("StartingEquity must be greater than zero because otherwiese many of the statistic produce division by zero (NaN or Not a Number) errors.");
				}
				startingEquity = value; }
		}
		
		public double NetProfit {
			get { return CurrentEquity - StartingEquity; }
		}
		
		[Browsable(false)]
		public double CurrentEquity {
			get { 
				return ClosedEquity + OpenEquity;
			}
		}
		
		/// <summary>
		/// ClosedEquity return the running total of profit or loss
		/// from all closed trades.
		/// </summary>
		public double ClosedEquity {
			get {
				return closedEquity;
			}
		}
		
		/// <summary>
		/// OpenEquity returns zero unless there is an open position.
		/// In that case, it returns the amount of open equity.
		/// </summary>
		[Browsable(false)]
		public double OpenEquity {
			get { 
				if( isMultiSymbolPortfolio) {
					return openEquity;
				} else {
					return performance.ComboTrades.OpenProfitLoss;
				}
			}
		}
		
		public EquityStats CalculateStatistics() {
			return new EquityStats(startingEquity,Daily,Weekly,Monthly,Yearly);
		}
		
		public bool GraphEquity {
			get { return graphEquity; }
			set { if( isInitialized) {
					throw new ApplicationException("You must set GraphEquity before initialize event occurs.");
				} else {
					graphEquity = value;
				}
			}
		}
		
		public bool EnableYearlyStats {
			get { return enableYearlyStats; }
			set { enableYearlyStats = value; }
		}
		
		public bool EnableMonthlyStats {
			get { return enableMonthlyStats; }
			set { enableMonthlyStats = value; }
		}
		
		public bool EnableWeeklyStats {
			get { return enableWeeklyStats; }
			set { enableWeeklyStats = value; }
		}
		
		public bool EnableDailyStats {
			get { return enableDailyStats; }
			set { enableDailyStats = value; }
		}
	}
}
