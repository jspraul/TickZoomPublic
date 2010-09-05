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
using System.Text;

using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Interceptors;
using TickZoom.Reports;
using TickZoom.Transactions;

namespace TickZoom.Statistics
{
	public class Performance : StrategyInterceptor
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(Performance));
		private static readonly bool trace = log.IsTraceEnabled;
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly Log barDataLog = Factory.SysLog.GetLogger("BarDataLog");
		private static readonly bool barDataDebug = barDataLog.IsDebugEnabled;
		private static readonly Log tradeLog = Factory.SysLog.GetLogger("TradeLog");
		private static readonly bool tradeDebug = tradeLog.IsDebugEnabled;
		private static readonly Log statsLog = Factory.SysLog.GetLogger("StatsLog");
		private static readonly bool statsDebug = statsLog.IsDebugEnabled;
		TransactionPairs comboTrades;
		TransactionPairsBinary comboTradesBinary;
		bool graphTrades = true;
		Equity equity;
		ProfitLoss profitLoss;
		PositionCommon position;
		Model model;
		
		public Performance(Model model)
		{
			this.model = model;
			equity = new Equity(model,this);
			position = new PositionCommon(model);
		}
		
		public double GetCurrentPrice( double direction) {
			System.Diagnostics.Debug.Assert(direction!=0);
			Tick tick = model.Ticks[0];
			if( direction > 0) {
				return tick.IsQuote ? tick.Bid : tick.Price;
			} else {
				return tick.IsQuote ? tick.Ask : tick.Price;
			}
		}
		
		EventContext context;
		
		public override void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			this.context = context;
			if( EventType.Initialize == eventType) {
				model.AddInterceptor( EventType.Close, this);
				model.AddInterceptor( EventType.Tick, this);
				model.AddInterceptor( EventType.LogicalFill, this);
				OnInitialize();
			}
			if( EventType.Close == eventType) {
				OnIntervalClose();
			}
			context.Invoke();
			if( eventType == EventType.LogicalFill) {
				OnProcessFill((LogicalFill) eventDetail);
			}
			if( eventType == EventType.Tick
			  && model is PortfolioInterface
// Uncommenting this will boost performance but the
// tick event is still needed by a few parts of TZ.
// After they are resolved, we can uncomment this.
			 ) {
			    
				OnProcessFill();
			}
		}
		
		public void OnInitialize()
		{ 
			comboTradesBinary  = new TransactionPairsBinary(model.Context.TradeData);
			comboTradesBinary.Name = "ComboTrades";
			profitLoss = model.Data.SymbolInfo.ProfitLoss;
			comboTrades  = new TransactionPairs(GetCurrentPrice,profitLoss,comboTradesBinary);
			profitLoss.Symbol = model.Data.SymbolInfo;

		}
		
		public bool OnProcessFill(LogicalFill fill)
		{
			if( fill.Position != position.Current) {
				if( position.IsFlat) {
					EnterComboTrade(fill);
				} else if( fill.Position == 0) {
					ExitComboTrade(fill);
				} else if( (fill.Position > 0 && position.IsShort) || (fill.Position < 0 && position.IsLong)) {
					// The signal must be opposite. Either -1 / 1 or 1 / -1
					ExitComboTrade(fill);
					EnterComboTrade(fill);
				} else {
					// Instead it has increased or decreased position size.
					ChangeComboSize(fill);
				}
			} 
			position.Change(model.Data.SymbolInfo,fill);
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.Result.Position.Copy(position);
			}

			if( model is Portfolio) {
				Portfolio portfolio = (Portfolio) model;
				double tempNetPortfolioEquity = 0;
				tempNetPortfolioEquity += portfolio.Performance.Equity.ClosedEquity;
				tempNetPortfolioEquity -= portfolio.Performance.Equity.StartingEquity;
			}
			return true;
		}
		
		public bool OnProcessFill()
		{
			if( context.Position.Current != position.Current) {
				if( position.IsFlat) {
					EnterComboTrade(context.Position);
				} else if( context.Position.IsFlat) {
					ExitComboTrade(context.Position);
				} else if( (context.Position.IsLong && position.IsShort) || (context.Position.IsShort && position.IsLong)) {
					// The signal must be opposite. Either -1 / 1 or 1 / -1
					ExitComboTrade(context.Position);
					EnterComboTrade(context.Position);
				} else {
					// Instead it has increased or decreased position size.
					ChangeComboSize();
				}
			} 
			position.Copy(context.Position);
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.Result.Position.Copy(position);
			}

			if( model is Portfolio) {
				Portfolio portfolio = (Portfolio) model;
				double tempNetPortfolioEquity = 0;
				tempNetPortfolioEquity += portfolio.Performance.Equity.ClosedEquity;
				tempNetPortfolioEquity -= portfolio.Performance.Equity.StartingEquity;
			}
			return true;
		}
		
		public void EnterComboTrade(LogicalFill fill) {
			TransactionPairBinary pair = TransactionPairBinary.Create();
			pair.Enter(fill.Position, fill.Price, fill.Time, model.Chart.ChartBars.BarCount);
			comboTradesBinary.Add(pair);
			if( trace) {
				log.Trace( "Enter trade: " + pair);
			}
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.OnEnterTrade();
			}
		}

		public void EnterComboTrade(PositionInterface position) {
			TransactionPairBinary pair = TransactionPairBinary.Create();
			pair.Enter(position.Current,position.Price, position.Time, model.Chart.ChartBars.BarCount);
			comboTradesBinary.Add(pair);
			if( trace) {
				log.Trace( "Enter trade: " + pair);
			}
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.OnEnterTrade();
			}
		}
		
		private void ChangeComboSize(LogicalFill fill) {
			TransactionPairBinary combo = comboTradesBinary.Tail;
			combo.ChangeSize(fill.Position,fill.Price);
			comboTradesBinary.Tail = combo;
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.OnChangeTrade();
			}
		}
		
		private void ChangeComboSize() {
			TransactionPairBinary combo = comboTradesBinary.Tail;
			combo.ChangeSize(context.Position.Current,context.Position.Price);
			comboTradesBinary.Tail = combo;
		}
		
		public void ExitComboTrade(LogicalFill fill) {
			TransactionPairBinary comboTrade = comboTradesBinary.Tail;
			comboTrade.Exit( fill.Price, fill.Time, model.Chart.ChartBars.BarCount);
			comboTradesBinary.Tail = comboTrade;
			double pnl = profitLoss.CalculateProfit(comboTrade.Direction,comboTrade.EntryPrice,comboTrade.ExitPrice);
			Equity.OnChangeClosedEquity( pnl);
			if( trace) {
				log.Trace( "Exit Trade: " + comboTrade);
			}
			if( tradeDebug && !model.QuietMode) tradeLog.Debug( model.Name + "," + Equity.ClosedEquity + "," + pnl + "," + comboTrade);
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.OnExitTrade();
			}
		}
		
		public void ExitComboTrade(PositionInterface position) {
			TransactionPairBinary comboTrade = comboTradesBinary.Tail;
			comboTrade.Exit(position.Price, position.Time, model.Chart.ChartBars.BarCount);
			comboTradesBinary.Tail = comboTrade;
			double pnl = profitLoss.CalculateProfit(comboTrade.Direction,comboTrade.EntryPrice,comboTrade.ExitPrice);
			Equity.OnChangeClosedEquity( pnl);
			if( trace) {
				log.Trace( "Exit Trade: " + comboTrade);
			}
			if( tradeDebug && !model.QuietMode) tradeLog.Debug( model.Name + "," + Equity.ClosedEquity + "," + pnl + "," + comboTrade);
			if( model is Strategy) {
				Strategy strategy = (Strategy) model;
				strategy.OnExitTrade();
			}
		}
		
		public bool OnIntervalClose()
		{
			if( barDataDebug && !model.QuietMode) {
				Bars bars = model.Bars;
				TimeStamp time = bars.Time[0];
				StringBuilder sb = new StringBuilder();
				sb.Append(model.Name);
				sb.Append(",");
				sb.Append(time);
				sb.Append(",");
				sb.Append(bars.Open[0]);
				sb.Append(",");
				sb.Append(bars.High[0]);
				sb.Append(",");
				sb.Append(bars.Low[0]);
				sb.Append(",");
				sb.Append(bars.Close[0]);
				barDataLog.Debug( sb.ToString());
			}
			if( statsDebug && !model.QuietMode) {
				Bars bars = model.Bars;
				TimeStamp time = bars.Time[0];
				StringBuilder sb = new StringBuilder();
				sb.Append(model.Name);
				sb.Append(",");
				sb.Append(time);
				sb.Append(",");
				sb.Append(equity.ClosedEquity);
				sb.Append(",");
				sb.Append(equity.OpenEquity);
				sb.Append(",");
				sb.Append(equity.CurrentEquity);
				statsLog.Debug( sb.ToString());
			}
			return true;
		}
		
		public Equity Equity {
			get { return equity; }
			set { equity = value; }
		}
		
		public bool WriteReport(string name, string folder) {
			name = name.StripInvalidPathChars();
//			TradeStatsReport tradeStats = new TradeStatsReport(this);
//			tradeStats.WriteReport(name, folder);
			StrategyStats stats = new StrategyStats(ComboTrades);
			Equity.WriteReport(name,folder,stats);
			IndexForReport index = new IndexForReport(this);
			index.WriteReport(name, folder);
			return true;
		}
			
		/// <summary>
		/// Obsolete. Please use the ProfitLoss interface
		/// </summary>
		[Obsolete("Please use the ProfitLoss interface instead.",true)]
		public double Slippage {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		
		/// <summary>
		/// Obsolete. Please use the ProfitLoss interface instead.
		/// </summary>
		[Obsolete("Please use the ProfitLoss interface instead.",true)]
		public double Commission {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException();  }
		}
		
		public PositionCommon Position {
			get { return position; }
			set { position = value; }
		}
		
#region Obsolete Methods		
		
		[Obsolete("Use WriteReport(name,folder) instead.",true)]
		public void WriteReport(string name,StreamWriter writer) {
			throw new NotImplementedException();
		}

		[Obsolete("Please use ComboTrades instead.",true)]
    	public TransactionPairs TransactionPairs {
			get { return null; }
		}
		
		[Obsolete("Please use Performance.Equity.Daily instead.",true)]
		public TransactionPairs CompletedDaily {
			get { return Equity.Daily; }
		}

		[Obsolete("Please use Performance.Equity.Weekly instead.",true)]
		public TransactionPairs CompletedWeekly {
			get { return Equity.Weekly; }
		}

		public TransactionPairs ComboTrades {
			get { 
				if( comboTradesBinary.Count > 0) {
					TransactionPairBinary binary = comboTradesBinary.Tail;
					binary.TryUpdate(model.Ticks[0]);
					comboTradesBinary.Tail = binary;
				}
				return comboTrades;
			}
		}
		
		[Obsolete("Please use Performance.Equity.Monthly instead.",true)]
		public TransactionPairs CompletedMonthly {
			get { return Equity.Monthly; }
		}
	
		[Obsolete("Please use Performance.Equity.Yearly instead.",true)]
		public TransactionPairs CompletedYearly {
			get { return Equity.Yearly; }
		}
		
		[Obsolete("Please use Performance.ComboTrades instead.",true)]
		public TransactionPairs CompletedComboTrades {
			get { return ComboTrades; }
		}
		
		[Obsolete("Please use TransactionPairs instead.",true)]
		public TransactionPairs Trades {
			get { return TransactionPairs; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Daily {
			get { return Equity.Daily; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Weekly {
			get { return Equity.Weekly; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Monthly {
			get { return Equity.Monthly; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public TransactionPairs Yearly {
			get { return Equity.Yearly; }
		}
		
		public bool GraphTrades {
			get { return graphTrades; }
			set { graphTrades = value; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ProfitToday {
			get { return Equity.CurrentEquity; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ProfitForWeek {
			get { return Equity.ProfitForWeek; }
		}

		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ProfitForMonth {
			get { return Equity.ProfitForMonth; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double StartingEquity {
			get { return Equity.StartingEquity; }
			set { Equity.StartingEquity = value; }
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double CurrentEquity {
			get { 
				return Equity.CurrentEquity;
			}
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double ClosedEquity {
			get {
				return Equity.ClosedEquity;
			}
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public double OpenEquity {
			get {
				return Equity.OpenEquity;
			}
		}
		
		public StrategyStats CalculateStatistics() {
			return new StrategyStats(ComboTrades);
		}
		
		[Obsolete("Please use the same property at Performance.Equity.* instead.",true)]
		public bool GraphEquity {
			get { return Equity.GraphEquity; }
			set { Equity.GraphEquity = value; }
		}
		
#endregion		

	}

	[Obsolete("Please user Performance instead.",true)]
	public class PerformanceCommon : Performance {
		public PerformanceCommon(Strategy strategy) : base(strategy)
		{
			
		}
	}
	
}