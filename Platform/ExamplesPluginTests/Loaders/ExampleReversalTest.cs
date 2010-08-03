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
	public class ExampleReversalTest : StrategyTest
	{
		
		#region SetupTest
		Log log = Factory.SysLog.GetLogger(typeof(ExampleReversalTest));
		protected Strategy strategy;
		
		public ExampleReversalTest() {
			Symbols = "Daily4Sim";
		}
		
		[TestFixtureSetUp]
		public override void RunStrategy() {
			base.RunStrategy();
			try {
				Starter starter = CreateStarterCallback();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		starter.DataFolder = "TestData";
	    		starter.ProjectProperties.Starter.SetSymbols( Symbols);
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
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
			Assert.AreEqual( Math.Round(9977.70,2),Math.Round(strategy.Performance.Equity.CurrentEquity,2),"current equity");
		}
		
		[Test]
		public void VerifyOpenEquity() {
			Assert.AreEqual( Math.Round(-0.15,2),Math.Round(strategy.Performance.Equity.OpenEquity,2),"open equity");
		}
		
		[Test]
		public void VerifyClosedEquity() {
			Assert.AreEqual( Math.Round(9977.85,2),Math.Round(strategy.Performance.Equity.ClosedEquity,2),"open equity");
		}
		
		[Test]
		public void VerifyStartingEquity() {
			Assert.AreEqual( 10000,strategy.Performance.Equity.StartingEquity,"starting equity");
		}
		
		[Test]
		public void CompareTradeCount() {
			Assert.AreEqual( 378,strategy.Performance.ComboTrades.Count, "trade count");
		}
		
		[Test]
		public void BuyStopSellStopTest() {
			VerifyPair( strategy, 0, "1983-04-06 09:00:00.000", 29.90,
			                 "1983-04-18 09:00:00.000",30.560);

		}

		[Test]
		public void LastTradeTest() {
			VerifyPair( strategy, strategy.Performance.ComboTrades.Count-1, "1989-12-29 09:00:00.000", 21.67,
			                 "1989-12-29 09:00:00.003",21.82);
		}
		
		[Test]
		public void SellLimitBuyLimitTest() {
			VerifyPair( strategy, 1, "1983-04-18 09:00:00.000", 30.56,
			                 "1983-04-19 09:00:00.000", 30.700);
		}
		
		[Test]
		public void VerifyBarData() {
			Bars days = strategy.Data.Get(Intervals.Day1);
			Assert.AreEqual( 1696, days.BarCount);
		}
		
		[Test]
		public void VerifyChartData() {
			Assert.AreEqual(1,ChartCount);
			ChartControl chart = GetChart(0);
     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
    		Assert.IsNotNull(pane.CurveList);
    		Assert.Greater(pane.CurveList.Count,0);
    		Assert.AreEqual(1696,pane.CurveList[0].Points.Count);
		}
		
		[Test]
		public void CompareBars() {
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}
