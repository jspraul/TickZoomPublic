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
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;
using TickZoom.Transactions;

namespace Loaders
{
	[TestFixture]
	public class ExampleDualSymbolTest : StrategyTest
	{
		#region SetupTest
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		ExampleOrderStrategy fourTicksStrategy;
		ExampleOrderStrategy fullTicksStrategy;
		Portfolio portfolio;
		public ExampleDualSymbolTest() {
			SyncTicks.Enabled = false;
			Symbols = "FullTick,Daily4Sim";
		}
		
		[TestFixtureSetUp]
		public override void RunStrategy() {
			CleanupFiles();
			try {
				Starter starter = CreateStarterCallback();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		starter.DataFolder = "Test\\DataCache";
	    		starter.ProjectProperties.Starter.SetSymbols( Symbols);
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				starter.ProjectProperties.Engine.RealtimeOutput = false;
				
				// Set up chart
		    	starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = null;
	
				// Run the loader.
				ExampleDualSymbolLoader loader = new ExampleDualSymbolLoader();
	    		starter.Run(loader);
	
	 			ShowChartCallback showChartCallback = new ShowChartCallback(HistoricalShowChart);
	 			showChartCallback();
	 
	 			// Get the stategy
	    		portfolio = loader.TopModel as Portfolio;
	    		fullTicksStrategy = portfolio.Strategies[0] as ExampleOrderStrategy;
	    		fourTicksStrategy = portfolio.Strategies[1] as ExampleOrderStrategy;
			} catch( Exception ex) {
				log.Error("Setup error.",ex);
				throw;
			}
		}
		#endregion
		
		[Test]
		public void CheckPortfolio() {
			double expected = fourTicksStrategy.Performance.Equity.CurrentEquity;
			expected -= fourTicksStrategy.Performance.Equity.StartingEquity;
			expected += fullTicksStrategy.Performance.Equity.CurrentEquity;
			expected -= fullTicksStrategy.Performance.Equity.StartingEquity;
			double portfolioTotal = portfolio.Performance.Equity.CurrentEquity;
			portfolioTotal -= portfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(-96.60,2), Math.Round(portfolioTotal,2));
			Assert.AreEqual(Math.Round(expected,2), Math.Round(portfolioTotal,2));
		}
		
		[Test]
		public void CheckPortfolioClosedEquity() {
			double expected = fourTicksStrategy.Performance.Equity.ClosedEquity;
			expected -= fourTicksStrategy.Performance.Equity.StartingEquity;
			expected += fullTicksStrategy.Performance.Equity.ClosedEquity;
			expected -= fullTicksStrategy.Performance.Equity.StartingEquity;
			double portfolioTotal = portfolio.Performance.Equity.ClosedEquity;
			portfolioTotal -= portfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(expected, portfolioTotal);
			Assert.AreEqual(Math.Round(-96.20,2), Math.Round(portfolioTotal,2));
		}
		
		[Test]
		public void CheckPortfolioOpenEquity() {
			double expected = fourTicksStrategy.Performance.Equity.OpenEquity;
			expected += fullTicksStrategy.Performance.Equity.OpenEquity;
			Assert.AreEqual(expected, portfolio.Performance.Equity.OpenEquity);
			Assert.AreEqual(Math.Round(-0.40,2), Math.Round(portfolio.Performance.Equity.OpenEquity,2));
		}
		
		[Test]
		public void CompareTradeCount() {
			TransactionPairs fourTicksRTs = fourTicksStrategy.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTicksStrategy.Performance.ComboTrades;
			Assert.AreEqual(fourTicksRTs.Count,fullTicksRTs.Count, "trade count");
			Assert.AreEqual(471,fullTicksRTs.Count, "trade count");
		}
			
		[Test]
		public void CompareAllRoundTurns() {
			try {
				var result = true;
				TransactionPairs fourTicksRTs = fourTicksStrategy.Performance.ComboTrades;
				TransactionPairs fullTicksRTs = fullTicksStrategy.Performance.ComboTrades;
				for( int i=0; i<fourTicksRTs.Count && i<fullTicksRTs.Count; i++) {
					TransactionPair fourRT = fourTicksRTs[i];
					TransactionPair fullRT = fullTicksRTs[i];
					double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
					double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
					if( fourEntryPrice != fullEntryPrice || fourRT.ExitPrice != fullRT.ExitPrice) {
						log.Error("Expected " + fullRT + " but was " + fourRT);
						result = false;
					}
				}
				Assert.IsTrue(result,"trade mismatches. See log file.");
			} catch {
				testFailed = true;
				throw;
			}
		}
		
		[Test]
		public void RoundTurn1() {
			TransactionPairs fourTicksRTs = fourTicksStrategy.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTicksStrategy.Performance.ComboTrades;
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
			TransactionPairs fourTicksRTs = fourTicksStrategy.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTicksStrategy.Performance.ComboTrades;
			int i=2;
			TransactionPair fourRT = fourTicksRTs[i];
			TransactionPair fullRT = fullTicksRTs[i];
			double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
			double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
			Assert.AreEqual(fourEntryPrice,fullEntryPrice,"Entry Price for Trade #" + i);
			Assert.AreEqual(fourRT.ExitPrice,fullRT.ExitPrice,"Exit Price for Trade #" + i);
		}
		
		
		[Test]
		public void CompareBars0() {
			CompareChart(fullTicksStrategy,GetChart(fullTicksStrategy.SymbolDefault));
		}
		
		[Test]
		public void CompareBars1() {
			CompareChart(fourTicksStrategy,GetChart(fourTicksStrategy.SymbolDefault));
		}
	}

}
