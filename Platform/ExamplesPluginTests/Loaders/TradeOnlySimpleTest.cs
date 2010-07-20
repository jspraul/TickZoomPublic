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
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;
using TickZoom.Starters;
using ZedGraph;

namespace Loaders
{
	[TestFixture]
	public class TradeOnlySimpleTest : StrategyTest
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(TradeOnlySimpleTest));
		#region SetupTest
		Strategy strategy;
			
		[TestFixtureSetUp]
		public override void RunStrategy() {
			base.RunStrategy();
			Starter starter = new HistoricalStarter();
			
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = new TimeStamp(2009,8,3);
    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(2009,8,4);
    		starter.DataFolder = "TestData";
    		starter.ProjectProperties.Starter.SetSymbols("TXF.Test");
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Minute1;
			
    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
    		
    		// Run the loader.
			ExampleReversalLoader loader = new ExampleReversalLoader();
    		starter.Run(loader);

    		// Get the stategy
    		strategy = loader.TopModel as ExampleReversalStrategy;

    		IList<DiagramAttribute> aspects = DiagramHelper.GetAspectsByCalls();
    		
    		for( int i=0; i<aspects.Count && i<100; i++) {
    			var aspect = aspects[i];
    			log.Notice( aspect.TypeName + "." + aspect.MethodSignature + ": " + aspect.CallCount);
    		}
		}
		#endregion
		
		[Test]
		public void CompareTradeCount() {
			Assert.AreEqual( 21,strategy.Performance.ComboTrades.Count, "trade count");
		}
		
		[Test]
		public void BuyStopSellStopTest() {
			VerifyPair( strategy, 0, "2009-08-02 23:14:01.001", 6950.000,
			                 "2009-08-02 23:21:01.001",6945.000);
		}

		
		[Test]
		public void SellLimitBuyLimitTest() {
			VerifyPair( strategy, 1, "2009-08-02 23:21:01.001", 6945.000,
			                 "2009-08-02 23:29:01.001", 6955.000);
		}
		
		[Test]
		public void VerifyBarData() {
			Bars bars = strategy.Bars;
			Assert.AreEqual( 153, bars.BarCount);
		}
		
		[Test]
		public void VerifyChartData() {
			Assert.AreEqual(1,ChartCount);
			ChartControl chart = GetChart(0);
     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
    		Assert.IsNotNull(pane.CurveList);
    		Assert.Greater(pane.CurveList.Count,0);
    		Assert.AreEqual(153,pane.CurveList[0].Points.Count);
		}
		
		[Test]
		public void CompareChart() {
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}
