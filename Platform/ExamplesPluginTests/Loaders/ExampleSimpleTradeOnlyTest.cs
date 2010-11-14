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
using ZedGraph;

namespace Loaders
{
	[TestFixture]
	public class ExampleReversalTradeOnlyTest : StrategyTest
	{
		#region SetupTest
		Log log = Factory.SysLog.GetLogger(typeof(ExampleReversalTradeOnlyTest));
		protected Strategy strategy;
		public ExampleReversalTradeOnlyTest() {
			Symbols = "/ESH0";
		}
		
		[TestFixtureSetUp]
		public override void RunStrategy() {
    		Assert.Ignore();

			CleanupFiles();
			try {
				Starter starter = CreateStarterCallback();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp("1800/1/1");
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp("2010/2/17");
	    		starter.DataFolder = "Test\\DataCache";
	    		starter.ProjectProperties.Starter.SetSymbols( Symbols);
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Minute1;
				starter.ProjectProperties.Engine.RealtimeOutput = false;
				
	    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
	    		
	    		// Run the loader.
				ExampleReversalLoader loader = new ExampleReversalLoader();
	    		starter.Run(loader);
	    		
	    		// Get the stategy
	    		strategy = loader.TopModel as ExampleReversalStrategy;
			} catch( Exception ex) {
				log.Error( "Setup failed.", ex);
				throw;
			}
		}
		#endregion
		
		[Test]
		public void VerifyCurrentEquity() {
			Assert.AreEqual( Math.Round(9975.00,2),Math.Round(strategy.Performance.Equity.CurrentEquity,2),"current equity");
		}
		[Test]
		public void VerifyOpenEquity() {
			Assert.AreEqual( Math.Round(12.5000,2),Math.Round(strategy.Performance.Equity.OpenEquity,2),"open equity");
		}
		[Test]
		public void VerifyClosedEquity() {
			Assert.AreEqual( Math.Round(9962.50,2),Math.Round(strategy.Performance.Equity.ClosedEquity,2),"open equity");
		}
		[Test]
		public void VerifyStartingEquity() {
			Assert.AreEqual( 10000,strategy.Performance.Equity.StartingEquity,"starting equity");
		}
		[Test]
		public void CompareTradeCount() {
			Assert.AreEqual( 2,strategy.Performance.ComboTrades.Count, "trade count");
		}
		
		[Test]
		public void BuyStopSellStopTest() {
			VerifyPair( strategy, 0, "2010-02-16 16:52:00.060", 1061.00d,
			                 "2010-02-16 16:54:00.183",1061.75d);
	
		}
	
		[Test]
		public void LastTradeTest() {
			VerifyPair( strategy, strategy.Performance.ComboTrades.Count-1, "2010-02-16 16:54:00.183", 1061.75d,
			                 "2010-02-16 16:59:56.140",1062.0d);
		}
		
		[Test]
		public void SellLimitBuyLimitTest() {
			VerifyPair( strategy, 1, "2010-02-16 16:54:00.183", 1061.75,
			                 "2010-02-16 16:59:56.140", 1062.0d);
		}
		
		[Test]
		public void VerifyBarData() {
			Bars days = strategy.Data.Get(Intervals.Minute1);
			Assert.AreEqual( 11, days.BarCount);
		}
		
		[Test]
		public void VerifyChartData() {
			Assert.AreEqual(1,ChartCount);
			ChartControl chart = GetChart(0);
	     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
	    		Assert.IsNotNull(pane.CurveList);
	    		Assert.Greater(pane.CurveList.Count,0);
	    		Assert.AreEqual(11,pane.CurveList[0].Points.Count);
		}
		
		[Test]
		public void CompareBars() {
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}
