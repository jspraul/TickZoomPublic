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
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;
using TickZoom.Starters;

namespace Loaders
{

	
	[TestFixture]
	public class ExampleSimulatedTest : StrategyTest
	{
		Log log = Factory.Log.GetLogger(typeof(ExampleSimulatedTest));
		#region SetupTest
		ExampleOrderStrategy strategy;
		
		public ExampleSimulatedTest() {
			StoreKnownGood = false;
		}
			
		[TestFixtureSetUp]
		public override void RunStrategy() {
			base.RunStrategy();
			try {
				Starter starter = new HistoricalStarter();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		starter.DataFolder = "TestData";
	    		starter.ProjectProperties.Starter.SetSymbols("Daily4Sim");
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				
	    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
				
				// Run the loader.
				ExampleLimitOrderLoader loader = new ExampleLimitOrderLoader();
	    		starter.Run(loader);
	
	    		// Get the stategy
	    		strategy = loader.TopModel as ExampleOrderStrategy;
	    		
	    		LoadTrades();
			} catch( Exception ex) {
				log.Error("Setup error.", ex);
				throw;
			}
		}
		
		[Test]
		public void VerifyCurrentEquity() {
			Assert.AreEqual( Math.Round(9992.52,2),Math.Round(strategy.Performance.Equity.CurrentEquity,2),"current equity");
		}
		[Test]
		public void VerifyOpenEquity() {
			Assert.AreEqual( Math.Round(-0.02,2),Math.Round(strategy.Performance.Equity.OpenEquity,2),"open equity");
		}
		[Test]
		public void VerifyClosedEquity() {
			Assert.AreEqual( Math.Round(9992.54,2),Math.Round(strategy.Performance.Equity.ClosedEquity,2),"closed equity");
		}
		[Test]
		public void VerifyStartingEquity() {
			Assert.AreEqual( 10000,strategy.Performance.Equity.StartingEquity,"starting equity");
		}
		
		[Test]
		public void VerifyTrades() {
			VerifyTrades(strategy);
		}

		[Test]
		public void VerifyTradeCount() {
			VerifyTradeCount(strategy);
		}
		
		#endregion
		
		[Test]
		public void CompareTradeCount() {
			Assert.AreEqual(472,strategy.Performance.ComboTrades.Count, "trade count");
		}
		
		[Test]
		public void BuyStopSellStopTest() {
			VerifyPair( strategy, 0, "1983-03-31 09:00:00.001", 29.500,
			                 "1983-03-31 09:00:00.002",29.300);
		}
		
		[Test]
		public void SellLimitBuyLimitTest() {
			VerifyPair( strategy, 1, "1983-04-04 09:00:00.001", 29.570,
			                 "1983-04-06 09:00:00.001", 29.680);
		}
		
		[Test]
		public void BuyStopStopLossTest() {
			VerifyPair( strategy, 2, "1983-04-08 09:00:00.000", 30.650D,
			                 "1983-04-11 09:00:00.001",30.280D);
		}
		
		[Test]
		public void TradeAfterStopLossTest() {
			VerifyPair( strategy, 3, "1983-04-12 09:00:00.002", 30.550D,
			                 "1983-04-12 09:00:00.002",31.000D);
		}
		
		[Test]
		public void SellStopStopLossTest() {
			VerifyPair( strategy, 4, "1983-04-18 09:00:00.000", 30.560D,
			                 "1983-04-18 09:00:00.002",30.710);
		}
		
		[Test]
		public void BuyStopBreakEvenTest() {
			VerifyPair( strategy, 44, "1983-12-16 09:00:00.000", 28.68D,
			                 "1983-12-20 09:00:00.002",28.68d);
		}
		
		[Test]
		public void CompareBars() {
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}
