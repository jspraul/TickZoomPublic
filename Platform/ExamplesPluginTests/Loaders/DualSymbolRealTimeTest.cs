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
using System.Threading;
using NUnit.Framework;
using System.Windows.Forms;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.TickUtil;
using ZedGraph;

namespace Loaders
{
	[TestFixture]
	public class DualSymbolRealTimeTest : StrategyTest
	{
		#region SetupTest
		static readonly Log log = Factory.Log.GetLogger(typeof(DualSymbolRealTimeTest));
		static readonly bool debug = log.IsDebugEnabled;
		ExampleOrderStrategy fourTicksPerBar;
		ExampleOrderStrategy fullTickData;
		StrategyCommon strategy;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
			log.Clear();
			RunDualSymbolStrategy(); 
			RunSingleSymbolStrategy();
		}
		
		public void RunSingleSymbolStrategy() {
			Starter starter = new HistoricalStarter();
			string symbol = "CLN_DB_fromTS";
			TimeStamp historicalStartTime = new TimeStamp(1800,1,1);
			TimeStamp historicalEndTime = new TimeStamp(1990,1,1);
			historicalEndTime = new TimeStamp(1983,6,1);
    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
    		starter.ShowChartCallback = null;
    		starter.ProjectProperties.Engine.RealtimeOutput = false;
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = historicalStartTime;
    		starter.ProjectProperties.Starter.EndTime = historicalEndTime;
    		starter.DataFolder = DataFolder;
    		starter.ProjectProperties.Starter.Symbols = symbol;
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
//			starter.DataFeeds = new TickQueue[] { realTimeQueue };
			
			// Run the loader.
			ExampleOrdersLoader loader = new ExampleOrdersLoader();
    		starter.Run(loader);
    		starter.Wait();
			
			HistoricalShowChart();
			
    		// Get the stategy
    		strategy = loader.TopModel as ExampleOrderStrategy;
		}
		
		
		public void RunDualSymbolStrategy() {
			Starter starter = new RealTimeStarter();
			
			// Multi / real time stuff.
			string symbol1 = "FullTick";
			string symbol2 = "Daily4Ticks";
			TimeStamp historicalStartTime = new TimeStamp(1800,1,1);
			TimeStamp realTimeStartTime = new TimeStamp(1983,5,28);
			TimeStamp realTimeEndTime = new TimeStamp(1983,6,1);
//			DataFeed dataFeed = LoadHistoricalDataAsFakeRealTimeData(
//				new string[] { symbol1, symbol2 }, realTimeStartTime, realTimeEndTime);
			
			// Set up chart
	    	starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
    		starter.ShowChartCallback = null; // new ShowChartCallback(HistoricalShowChart);
    		starter.ProjectProperties.Engine.RealtimeOutput = false;
//    		starter.ProjectProperties.Engine.BreakAtBar = 41;

    		// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = historicalStartTime;
    		starter.ProjectProperties.Starter.EndTime = realTimeEndTime;
    		starter.DataFolder = "TestData";
    		starter.ProjectProperties.Starter.Symbols = symbol1+","+symbol2;
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
//			starter.AddDataFeed(dataFeed);
			
			// Run the loader.
			ExampleDualSymbolLoader loader = new ExampleDualSymbolLoader();
    		starter.Run(loader);
    		starter.Wait();
			
			HistoricalShowChart();
			
    		// Get the stategy
    		PortfolioCommon portfolio = loader.TopModel as PortfolioCommon;
    		fullTickData = portfolio.Strategies[0] as ExampleOrderStrategy;
    		fourTicksPerBar = portfolio.Strategies[1] as ExampleOrderStrategy;
			if( debug) {
    			log.Debug("Full Tick Trades");
				for( int i=0; i<fullTickData.Performance.Trades.Count; i++) {
					log.Debug(fullTickData.Performance.Trades[i]);
				}
    			log.Debug("Four Ticks Per Bar Trades");
				for( int i=0; i<fourTicksPerBar.Performance.Trades.Count; i++) {
					log.Debug(fourTicksPerBar.Performance.Trades[i]);
				}
			}
		}
		#endregion
		
		[Test]
		public void CompareTradeCount() {
			RoundTurns fourTicksRTs = fourTicksPerBar.Performance.Trades;
			RoundTurns fullTicksRTs = fullTickData.Performance.Trades;
			Assert.AreEqual(fourTicksRTs.Count,fullTicksRTs.Count, "trade count");
			TickEngine engine = Factory.Engine.TickEngine;
			Assert.AreEqual(13,fullTicksRTs.Count, "trade count");
		}
			
		[Test]
		public void CompareAllRoundTurns() {
			TickEngine engine = Factory.Engine.TickEngine;
			RoundTurns fourTicksRTs = fourTicksPerBar.Performance.Trades;
			RoundTurns fullTicksRTs = fullTickData.Performance.Trades;
			
			for( int i=0; i<fourTicksRTs.Count && i<fullTicksRTs.Count; i++) {
				if( engine.IsFree && i>=12) {
					break;
				}
				RoundTurn fourRT = fourTicksRTs[i];
				RoundTurn fullRT = fullTicksRTs[i];
				Assert.AreEqual(fourRT.EntryPrice,fullRT.EntryPrice,"Entry Price for Trade #" + i);
				Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
			}
		}
		
		[Test]
		public void CompareSingleToDualRoundTurns() {
			TickEngine engine = Factory.Engine.TickEngine;
			RoundTurns singleRTs = strategy.Performance.Trades;
			RoundTurns dualRTs = fullTickData.Performance.Trades;
			for( int i=0; i<singleRTs.Count && i<dualRTs.Count; i++) {
				if( engine.IsFree && i>=11) {
					break;
				}
				RoundTurn singleRT = singleRTs[i];
				RoundTurn dualRT = dualRTs[i];
				double singleEntryPrice = Math.Round(singleRT.EntryPrice,2).Round();
				double dualEntryPrice = Math.Round(dualRT.EntryPrice,2).Round();
				Assert.AreEqual(singleEntryPrice,dualEntryPrice,"Entry Price for Trade #" + i);
				Assert.AreEqual(singleRT.ExitPrice,dualRT.ExitPrice,"Exit Price for Trade #" + i);
			}
//			Assert.AreEqual(singleRTs.Count,dualRTs.Count);
		}
		
		[Test]
		public void CompareSingleToFullTickDaily() {
			Bars daysSingle = strategy.Days;
			Bars daysFull = fullTickData.Days;
			int firstMisMatch = int.MaxValue;
			int i;
    		for( i=0; i<daysSingle.Count; i++) {
				if( i >= daysFull.Count) {
					log.Debug("bar " + i + " is missing");
				} else {
	    			string match = "NOT match";
	    			if( daysSingle.Open[i] == daysFull.Open[i] &&
	    			   daysSingle.High[i] == daysFull.High[i] &&
	    			   daysSingle.Low[i] == daysFull.Low[i] &&
	    			   daysSingle.Close[i] == daysFull.Close[i]) {
	    			    match = "matches";
	    			} else {
	    				if( firstMisMatch == int.MaxValue) {
	    					firstMisMatch = i;
	    				}
	    			}
	    			if (debug) log.Debug("single: " + i + " " + match + " days:"+daysSingle.Open[i]+","+daysSingle.High[i]+","+daysSingle.Low[i]+","+daysSingle.Close[i]+" => "+
	    			              " full: " + daysFull.Open[i]+","+daysFull.High[i]+","+daysFull.Low[i]+","+daysFull.Close[i]);
				}
    		}
			if( firstMisMatch != int.MaxValue) {
				i=firstMisMatch;
    			Assert.AreEqual(daysSingle.Open[i],daysFull.Open[i],"Open for bar " + i );
    			Assert.AreEqual(daysSingle.High[i],daysFull.High[i],"High for bar " + i );
    			Assert.AreEqual(daysSingle.Low[i],daysFull.Low[i],"Low for bar " + i );
    			Assert.AreEqual(daysSingle.Close[i],daysFull.Close[i],"Close for bar " + i );
			}
//			Assert.AreEqual(daysSingle.Count,daysFull.Count);
		}

		[Test]
		public void CompareSingleToFourTickDaily() {
			Bars daysSingle = strategy.Days;
			Bars daysFourTick = fourTicksPerBar.Days;
			Assert.AreEqual(daysSingle.BarCount,daysFourTick.BarCount);
			int firstMisMatch = int.MaxValue;
			int i;
    		for( i=0; i<daysSingle.Count; i++) {
				if( i >= daysFourTick.Count) {
					log.WriteFile("bar " + i + " is missing");
				} else {
	    			string match = "NOT match";
	    			if( daysSingle.Open[i] == daysFourTick.Open[i] &&
	    			   daysSingle.High[i] == daysFourTick.High[i] &&
	    			   daysSingle.Low[i] == daysFourTick.Low[i] &&
	    			   daysSingle.Close[i] == daysFourTick.Close[i]) {
	    			    match = "matches";
	    			} else {
	    				if( firstMisMatch == int.MaxValue) {
	    					firstMisMatch = i;
	    				}
	    			}
	    			log.WriteFile("single: " + i + " " + match + " days:"+daysSingle.Open[i]+","+daysSingle.High[i]+","+daysSingle.Low[i]+","+daysSingle.Close[i]+" => "+
	    			              " full: " + daysFourTick.Open[i]+","+daysFourTick.High[i]+","+daysFourTick.Low[i]+","+daysFourTick.Close[i]);
				}
				if( firstMisMatch != int.MaxValue) {
					i=firstMisMatch;
	    			Assert.AreEqual(daysSingle.Open[i],daysFourTick.Open[i],"Open for bar " + i );
	    			Assert.AreEqual(daysSingle.High[i],daysFourTick.High[i],"High for bar " + i );
	    			Assert.AreEqual(daysSingle.Low[i],daysFourTick.Low[i],"Low for bar " + i );
	    			Assert.AreEqual(daysSingle.Close[i],daysFourTick.Close[i],"Close for bar " + i );
				}
    			
    		}
		}
		
		[Test]
		public void RoundTurn1() {
			RoundTurns fourTicksRTs = fourTicksPerBar.Performance.Trades;
			RoundTurns fullTicksRTs = fullTickData.Performance.Trades;
			int i=1;
			RoundTurn fourRT = fourTicksRTs[i];
			RoundTurn fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
		}
		
		[Test]
		public void RoundTurn2() {
			RoundTurns fourTicksRTs = fourTicksPerBar.Performance.Trades;
			RoundTurns fullTicksRTs = fullTickData.Performance.Trades;
			int i=2;
			RoundTurn fourRT = fourTicksRTs[i];
			RoundTurn fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
		}
		
		[Test]
		public void RoundTurn10() {
			VerifyRoundTurn( strategy, 10, "1983-05-19 15:59:00.010", 30.0d,
			                 "1983-05-24 15:59:00.001", 30.05d);
		}
		
		[Test]
		public void RoundTurn11() {
			VerifyRoundTurn( strategy, 11, "1983-05-26 15:59:00.006", 30.35d,
			                 "1983-05-27 15:59:00.051", 30.21d);
		}

		[Test]
		public void VerifySingleChartCount() {
			TickEngine engine = Factory.Engine.TickEngine;
			if( engine.IsFree) {
				VerifyChartBarCount("CLN_DB_fromTS",43);
			} else {
				VerifyChartBarCount("CLN_DB_fromTS",43);
			}
		}
		
		[Test]
		public void VerifyFullTickChartCount() {
			TickEngine engine = Factory.Engine.TickEngine;
			VerifyChartBarCount(fullTickData.SymbolDefault,43);
		}
		
		[Test]
		public void VerifyChartData() {
			TickEngine engine = Factory.Engine.TickEngine;
			VerifyChartBarCount(fourTicksPerBar.SymbolDefault,43);
		}
		
		[Test]
		public void CompareSingleToChart() {
			CompareChart(strategy);
		}
		
		[Test]
		public void CompareFourTickToChart() {
			CompareChart(fourTicksPerBar);
		}
		
		[Test]
		public void CompareFourTickChartCount() {
			CompareChartCount(fourTicksPerBar);
		}
		
		[Test]
		public void CompareFullTickToChart() {
			CompareChart(fullTickData);
		}
		
	}
}
