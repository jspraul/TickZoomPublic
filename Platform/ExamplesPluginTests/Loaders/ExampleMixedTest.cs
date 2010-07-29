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
using TickZoom.Common;
using TickZoom.Examples;
using TickZoom.Starters;
using TickZoom.Transactions;

namespace Loaders
{

	[TestFixture]
	public class ExampleMixedTest : StrategyTest
	{
		List<StrategyInterface> strategies = new List<StrategyInterface>();
		
		#region SetupTest
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		ExampleOrderStrategy fourTicksPerBar;
		ExampleOrderStrategy fullTickData;
		
		Portfolio multiSymbolPortfolio;			
		Portfolio singleSymbolPortfolio;			
   		Strategy exampleReversal;
   		
   		public ExampleMixedTest() {
			ShowCharts = false;
			StoreKnownGood = false;
   		}
			
		[TestFixtureSetUp]
		public override void RunStrategy() {
			base.RunStrategy();
			Starter starter = new HistoricalStarter();
			
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    	starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    	starter.DataFolder = "TestData";
	    	starter.ProjectProperties.Starter.SetSymbols("FullTick,Daily4Sim");
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
			
			// Set up chart
	    	starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    	starter.ShowChartCallback = null;
	
			// Run the loader.
			ExampleMixedLoader loader = new ExampleMixedLoader();
    		starter.Run(loader);

 			ShowChartCallback showChartCallback = new ShowChartCallback(HistoricalShowChart);
 			showChartCallback();
 
 			// Get the stategy
    		multiSymbolPortfolio = loader.TopModel as Portfolio;
    		fullTickData = multiSymbolPortfolio.Strategies[0] as ExampleOrderStrategy;
    		singleSymbolPortfolio = multiSymbolPortfolio.Portfolios[0] as Portfolio;
    		fourTicksPerBar = singleSymbolPortfolio.Strategies[0] as ExampleOrderStrategy;
    		exampleReversal = singleSymbolPortfolio.Strategies[1] as ExampleReversalStrategy;
    		strategies.Add(multiSymbolPortfolio);
    		strategies.Add(fullTickData);
    		strategies.Add(singleSymbolPortfolio);
    		strategies.Add(fourTicksPerBar);
    		LoadTrades();
    		LoadBarData();
    		LoadStats();
		}
		
		#endregion
		
		[Test] 
		public void TestSingleSymbolPortfolio() {
			double expectedEquity = 0;
			double fourTicksEquity = 0;
			fourTicksEquity += fourTicksPerBar.Performance.Equity.CurrentEquity;
			fourTicksEquity -= fourTicksPerBar.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(-7.4800,2),Math.Round(fourTicksEquity,2),"four ticks");
			double exampleEquity = 0;
			exampleEquity += exampleReversal.Performance.Equity.CurrentEquity;
			exampleEquity -= exampleReversal.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(-22.3000,2),Math.Round(exampleEquity,2),"example simple");
			
			expectedEquity = fourTicksEquity + exampleEquity;
			
			double portfolioEquity = singleSymbolPortfolio.Performance.Equity.CurrentEquity;
			portfolioEquity -= singleSymbolPortfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(expectedEquity,4),Math.Round(portfolioEquity,4));
			Assert.AreEqual(Math.Round(-29.7800,4),Math.Round(portfolioEquity,4));
		}
		
		[Test] 
		public void TestMultiSymbolPortfolio() {
			double expectedEquity = 0;
			expectedEquity += fullTickData.Performance.Equity.CurrentEquity;
			expectedEquity -= fullTickData.Performance.Equity.StartingEquity;
			expectedEquity += singleSymbolPortfolio.Performance.Equity.CurrentEquity;
			expectedEquity -= singleSymbolPortfolio.Performance.Equity.StartingEquity;
			
			double portfolioEquity = multiSymbolPortfolio.Performance.Equity.CurrentEquity;
			portfolioEquity -= multiSymbolPortfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(expectedEquity,4),Math.Round(portfolioEquity,4));
			Assert.AreEqual(Math.Round(-37.2600,4),Math.Round(portfolioEquity,4));
		}
		
		[Test]
		public void CompareTradeCount() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			Assert.AreEqual(fourTicksRTs.Count,fullTicksRTs.Count, "trade count");
			Assert.AreEqual(472,fullTicksRTs.Count, "trade count");
		}
			
		[Test]
		public void CompareAllPairs() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			for( int i=0; i<fourTicksRTs.Count && i<fullTicksRTs.Count; i++) {
				TransactionPair fourRT = fourTicksRTs[i];
				TransactionPair fullRT = fullTicksRTs[i];
				double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
				double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
				Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
				Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
			}
		}
		
		[Test]
		public void RoundTurn1() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			int i=1;
			TransactionPair fourRT = fourTicksRTs[i];
			TransactionPair fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
		}
		
		[Test]
		public void RoundTurn2() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			int i=2;
			TransactionPair fourRT = fourTicksRTs[i];
			TransactionPair fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
		}
		
		[Test]
		public void LastRoundTurn() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			int i = fourTicksRTs.Current;
			TransactionPair fourRT = fourTicksRTs[i];
			TransactionPair fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			TimeStamp fourExitTime = fourRT.ExitTime;
			TimeStamp fullExitTime = fullRT.ExitTime;
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
			Assert.AreEqual(fourExitTime,fourRT.ExitTime,"Exit Time for Trade #" + i);
			Assert.AreEqual(fullExitTime,fullRT.ExitTime,"Exit Time for Trade #" + i);
			Assert.AreEqual(new TimeStamp("1989-12-29 15:59:00.050"),fullRT.ExitTime,"Exit Time for Trade #" + i);
		}
		
		[Test]
		public void CompareBars0() {
			CompareChart(fullTickData,GetChart(fullTickData.SymbolDefault));
		}
		
		[Test]
		public void CompareBars1() {
			CompareChart(fourTicksPerBar,GetChart(fourTicksPerBar.SymbolDefault));
		}
		
		[Test]
		public void VerifyFullTickTrades() {
			VerifyTrades(fullTickData);
		}
		
		[Test]
		public void VerifyFullTickTradeCount() {
			VerifyTradeCount(fullTickData);
		}
		
		[Test]
		public void VerifyFullTickBarDataCount() {
			VerifyBarDataCount(fullTickData);
		}
		
		[Test]
		public void VerifyFullTickBarData() {
			VerifyBarData(fullTickData);
		}
	
		[Test]
		public void VerifyFullTickStatsCount() {
			VerifyStatsCount(fullTickData);
		}
		
		[Test]
		public void VerifyFullTickStats() {
			VerifyStats(fullTickData);
		}
		
		[Test]
		public void VerifyFourTicksTrades() {
			VerifyTrades(fourTicksPerBar);
		}
		
		// FourTicks
		[Test]
		public void VerifyFourTicksTradeCount() {
			VerifyTradeCount(fourTicksPerBar);
		}
		
		[Test]
		public void VerifyFourTicksBarData() {
			VerifyBarData(fourTicksPerBar);
		}
		
		[Test]
		public void VerifyFourTicksBarDataCount() {
			VerifyBarDataCount(fourTicksPerBar);
		}

		[Test]
		public void VerifyFourTickStatsCount() {
			VerifyStatsCount(fourTicksPerBar);
		}
		
		[Test]
		public void VerifyFourTickStats() {
			VerifyStats(fourTicksPerBar);
		}
		
		// Portfolio
		[Test]
		public void VerifySingleSymbolTrades() {
			VerifyTrades(singleSymbolPortfolio);
		}
		
		[Test]
		public void VerifySingleSymbolTradeCount() {
			VerifyTradeCount(singleSymbolPortfolio);
		}
		
		[Test]
		public void VerifySingleSymbolBarData() {
			VerifyBarData(singleSymbolPortfolio);
		}
		
		[Test]
		public void VerifySingleSymbolBarDataCount() {
			VerifyBarDataCount(singleSymbolPortfolio);
		}
		
		[Test]
		public void VerifySingleSymbolStatsCount() {
			VerifyStatsCount(singleSymbolPortfolio);
		}
		
		[Test]
		public void VerifySingleSymbolStats() {
			VerifyStats(singleSymbolPortfolio);
		}
		
		[Test]
		public void VerifyMultiSymbolStatsCount() {
			VerifyStatsCount(multiSymbolPortfolio);
		}
		
		[Test]
		public void VerifyMultiSymbolStats() {
			VerifyStats(multiSymbolPortfolio);
		}
		
	}
}
