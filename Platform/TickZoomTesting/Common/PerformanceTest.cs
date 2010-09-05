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
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Properties;
using TickZoom.Starters;
using TickZoom.Statistics;
using TickZoom.Transactions;

#if TESTING
namespace TickZoom.Common
{
	[TestFixture]
	public class PerformanceTest
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(PerformanceTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		PerformanceInner performance;
		
		[Test]
		public void Constructor()
		{
			Strategy strategy = new Strategy();
			performance = new PerformanceInner(strategy);
			Assert.IsNotNull(performance,"MoneyManagerSupport constructor");
		}
		
		public class PerformanceInner : Performance {
			Log log = Factory.SysLog.GetLogger(typeof(PerformanceInner));
			public List<Tick> signalChanges = new List<Tick>();
			public List<double> signalDirection = new List<double>();
			double prevSignal = 0;
			public TradingSignalTest tradingSignalTest;
			public PerformanceInner( Model model) : base(model) {
				Position = tradingSignalTest = new TradingSignalTest(this,model);
			}
			public new void OnInitialize()
			{
				base.OnInitialize();
				signalChanges = new List<Tick>();
				List<int> signalDirection = new List<int>();
			}
			public class TradingSignalTest : PositionCommon {
				PerformanceInner test;
				public TradingSignalTest( PerformanceInner test, ModelInterface formula ) : base(formula) {
				 	this.test = test;
				}
				public override double Current  {
					get { return base.Current; }
				}
				public override void Change( double position, double price, TimeStamp time) {
					base.Change( position,price,time);
					if( base.Current != test.prevSignal) {
							test.signalChanges.Add(model.Data.Ticks[0]);
							test.signalDirection.Add(base.Current);
							test.prevSignal = base.Current;
					}
				}
			}
			public void TickConsoleWrite() {
				for( int i = 0; i< signalChanges.Count; i++) {
					Tick tick = signalChanges[i];
					double signal = signalDirection[i];
					log.Debug( i + ": " + tick + " Direction: " + signal);
				}
			}
		}

		public PerformanceInner TradeTickProcessing(int stop, int target, int count) {

			// Creation.
			RandomCommon random = new RandomCommon();
			PerformanceInner performance = new PerformanceInner(random);
			ProfitLossDefault profitLossLogic = new ProfitLossDefault();
			profitLossLogic.Slippage = 0.0140;
			profitLossLogic.Commission = 0.010;
			random.Performance = performance;
			random.Performance.Equity.EnableMonthlyStats = true;
			random.Performance.Equity.EnableWeeklyStats = true;

			// Stops
			random.ExitStrategy.StopLoss = stop;
			random.ExitStrategy.TargetProfit = target;
			
			Starter starter = new HistoricalStarter();
			starter.StartCount = 0;
			starter.EndCount = starter.StartCount + count + 1;
			starter.ProjectProperties.Starter.SetSymbols("USD_JPY_YEARS");
			starter.ProjectProperties.Starter.SymbolProperties[0].ProfitLoss = profitLossLogic;
			starter.DataFolder = "TestData";
			starter.Run(random);
			
			Assert.AreEqual(performance,random.Performance);
			Assert.AreEqual(performance.tradingSignalTest,random.Performance.Position);
			
//			Signal Times BEFORE applying stops or target
//			0: 1: time: 2004-07-22 08:02:08.757 bid: 109440 ask: 109440
//			1: 0: time: 2004-07-22 08:17:22.292 bid: 109500 ask: 109500
//			2: 1: time: 2004-07-22 08:53:46.250 bid: 109500 ask: 109500
//			3: 0: time: 2004-07-22 09:13:55.649 bid: 109440 ask: 109440
//			4: 1: time: 2004-07-22 09:52:09.466 bid: 109510 ask: 109510
//			5: 0: time: 2004-07-22 10:15:41.148 bid: 109480 ask: 109480
//			6: 1: time: 2004-07-22 10:17:15.545 bid: 109460 ask: 109460
//			7: 0: time: 2004-07-22 10:25:51.115 bid: 109430 ask: 109430
//			8: -1: time: 2004-07-22 10:39:44.278 bid: 109300 ask: 109300
//			9: 0: time: 2004-07-22 10:50:38.874 bid: 109440 ask: 109440
//			10: -1: time: 2004-07-22 11:56:52.119 bid: 109650 ask: 109650
//			11: 0: time: 2004-07-22 12:00:06.510 bid: 109640 ask: 109640
//			12: -1: time: 2004-07-23 08:01:04.984 bid: 110140 ask: 110140
//			13: 0: time: 2004-07-23 08:59:47.208 bid: 110060 ask: 110060
//			14: 1: time: 2004-07-23 09:15:11.005 bid: 110040 ask: 110040
//			15: -1: time: 2004-07-23 09:27:48.083 bid: 110030 ask: 110030
//			16: 1: time: 2004-07-23 09:50:06.263 bid: 110040 ask: 110040
//			17: -1: time: 2004-07-23 10:06:45.265 bid: 110030 ask: 110030
//			18: 1: time: 2004-07-23 10:12:18.754 bid: 110010 ask: 110010
//			19: 0: time: 2004-07-23 10:13:04.936 bid: 109980 ask: 109980
//			20: -1: time: 2004-07-23 10:22:03.758 bid: 110030 ask: 110030
//			21: 1: time: 2004-07-23 10:27:12.674 bid: 110020 ask: 110020
//			22: 0: time: 2004-07-23 10:40:50.220 bid: 109990 ask: 109990
//			23: 1: time: 2004-07-23 10:46:57.644 bid: 109990 ask: 109990
//			24: -1: time: 2004-07-23 11:34:50.614 bid: 110060 ask: 110060
//			25: 1: time: 2004-07-23 11:40:56.003 bid: 109970 ask: 109970
//			26: -1: time: 2004-07-23 11:57:13.950 bid: 110050 ask: 110050
//			27: 1: time: 2004-07-23 11:59:46.275 bid: 110080 ask: 110080
//			28: 0: time: 2004-07-23 12:00:06.336 bid: 110100 ask: 110100
//			29: -1: time: 2004-07-27 08:14:04.988 bid: 109800 ask: 109800
//			30: 1: time: 2004-07-27 08:17:55.551 bid: 109820 ask: 109820
//			31: -1: time: 2004-07-27 08:30:10.289 bid: 109860 ask: 109860
//			32: 1: time: 2004-07-27 08:33:25.932 bid: 109930 ask: 109930
//			33: -1: time: 2004-07-27 08:48:41.252 bid: 109940 ask: 109940
//			34: 1: time: 2004-07-27 08:58:40.535 bid: 109960 ask: 109960
//			35: 0: time: 2004-07-27 09:19:58.806 bid: 110240 ask: 110240
//			36: -1: time: 2004-07-27 09:48:02.851 bid: 110310 ask: 110310
//			37: 1: time: 2004-07-27 09:53:33.746 bid: 110330 ask: 110330
//			38: -1: time: 2004-07-27 10:10:49.768 bid: 110600 ask: 110600
//			39: 0: time: 2004-07-27 10:23:21.270 bid: 110660 ask: 110660
//			40: -1: time: 2004-07-27 10:31:54.263 bid: 110700 ask: 110700
//			41: 1: time: 2004-07-27 10:36:49.036 bid: 110810 ask: 110810
//			42: -1: time: 2004-07-27 10:44:22.552 bid: 110850 ask: 110850
//			43: 1: time: 2004-07-27 11:16:57.432 bid: 110910 ask: 110910
//			44: -1: time: 2004-07-27 11:26:19.378 bid: 110930 ask: 110930
//			45: 1: time: 2004-07-27 11:38:49.009 bid: 110970 ask: 110970
//			46: 0: time: 2004-07-27 11:48:44.955 bid: 111040 ask: 111040
//			47: -1: time: 2004-07-28 08:01:47.851 bid: 111410 ask: 111410
//			48: 1: time: 2004-07-28 08:28:33.049 bid: 111610 ask: 111610
//			49: -1: time: 2004-07-28 08:29:08.191 bid: 111600 ask: 111600
//			50: 1: time: 2004-07-28 08:45:35.197 bid: 111200 ask: 111200
//			51: -1: time: 2004-07-28 08:49:55.338 bid: 111270 ask: 111270
//			52: 1: time: 2004-07-28 09:23:09.808 bid: 111350 ask: 111350
//			53: -1: time: 2004-07-28 09:41:02.790 bid: 111330 ask: 111330
//			54: 1: time: 2004-07-28 09:52:19.965 bid: 111460 ask: 111460
//			55: -1: time: 2004-07-28 11:05:27.532 bid: 111650 ask: 111650
//			56: 1: time: 2004-07-28 11:13:15.674 bid: 111870 ask: 111870
//			57: 0: time: 2004-07-28 12:00:00.645 bid: 111740 ask: 111740
//			58: -1: time: 2004-07-29 08:05:02.825 bid: 112200 ask: 112200
//			59: 0: time: 2004-07-29 08:17:55.727 bid: 112230 ask: 112230
//			60: -1: time: 2004-07-29 08:56:59.034 bid: 112110 ask: 112110
//			61: 0: time: 2004-07-29 08:57:16.116 bid: 112130 ask: 112130
//			62: 1: time: 2004-07-29 09:05:49.077 bid: 112210 ask: 112210
//			63: -1: time: 2004-07-29 09:19:57.343 bid: 112270 ask: 112270
//			64: 1: time: 2004-07-29 09:43:32.387 bid: 112220 ask: 112220
//			65: 0: time: 2004-07-29 09:46:14.921 bid: 112220 ask: 112220

//			performance.TickConsoleWrite();
//			
//			for( int i = 0; i< manager.Trades.Count; i++) {
//				TickConsole.WriteLine(i + ": " + manager.Trades[i]);
//			}
//			for( int i=0; i< random.Ticks.Count; i++) {
//				log.Info(random.Ticks[i]);
//			}
			return (PerformanceInner) random.Performance;
		}
		
		[Test]
		public void TradeTesting() {
			PerformanceInner manager = TradeTickProcessing(0,0,30000);
			TransactionPairBinary expected = TransactionPairBinary.Create();
			expected.Enter(1,105.660,new TimeStamp(2005,2,8,11,57,22,429),1);
			expected.Exit(105.670,new TimeStamp(2005,2,8,11,57,51,479),1);
			TransactionPairs rts = manager.ComboTrades;
			Assert.AreEqual(-.034,Math.Round(rts.CalcProfitLoss(0),3),"First Trade PnL");
		}
		
		[Test]
		public void TradeTesting7() {
			Performance manager = TradeTickProcessing(0,0,30000);
			TransactionPairBinary expected = TransactionPairBinary.Create();
			expected.Enter(-1,105.650,new TimeStamp(2005,2,8,11,59,52,745),1);
			expected.Exit(105.660,new TimeStamp(2005,2,08,11,59,58,763),1);
		}
		
		[Test]
		public void TradeTesting8() {
			Performance manager = TradeTickProcessing(0,0,30000);
			TransactionPairBinary expected = TransactionPairBinary.Create();
			expected.Enter(1,105.66,new TimeStamp(2005,2,08,11,59,58,763),1);
			expected.Exit(105.75,new TimeStamp(2005,2,9,8,51,10,816),1);
		}
		[Test]
		public void DailyTesting() {
			Performance manager = TradeTickProcessing(0,0,6000);
			TransactionPairs daily = manager.Equity.Daily;
			if( debug) {
				for( int i = 0; i< daily.Count; i++) {
					log.Debug(i + ": " + daily[i]);
				}
			}
			Assert.AreEqual(6,daily.Count,"Daily Count");
			Assert.AreEqual(0.2080,Math.Round(daily.OpenProfitLoss,3),"Final trade completion");
		}
		[Test]
		public void WeeklyTesting() {
			Performance manager = TradeTickProcessing(0,0,30000);
			TransactionPairs weekly = manager.Equity.Weekly;
			for( int i = 0; i< weekly.Count; i++) {
				log.Notice(i + ": " + weekly[i]);
			}
			Assert.AreEqual(5,weekly.Count,"Weekly Count");
			Assert.AreEqual(-0.782,Math.Round(weekly.OpenProfitLoss,3),"Final trade completion");
		}
		[Test]
		public void MonthlyTesting() {
			Performance manager = TradeTickProcessing(0,0,80000);
			TransactionPairs monthly = manager.Equity.Monthly;
			for( int i = 0; i< monthly.Count; i++) {
				log.Notice(i + ": " + monthly[i]);
			}
			Assert.AreEqual(4,monthly.Count,"Monthly Count");
			Assert.AreEqual(0.406,Math.Round(monthly.OpenProfitLoss,3),"Final trade completion");
		}
	}
}
#endif