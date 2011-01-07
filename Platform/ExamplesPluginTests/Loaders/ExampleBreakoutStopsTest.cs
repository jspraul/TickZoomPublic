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
	public class ExampleBreakoutStopsTest : StrategyTest
	{
		Log log = Factory.SysLog.GetLogger(typeof(ExampleSimulatedTest));
		#region SetupTest
		ExampleBreakoutStops strategy;
		
		public ExampleBreakoutStopsTest() {
			ShowCharts = false;
			SyncTicks.Enabled = false;
		}
			
		[TestFixtureSetUp]
		public override void RunStrategy() {
			CleanupFiles();
			try {
				Starter starter = new HistoricalStarter();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(2010,1,1);
	    		starter.DataFolder = "Test\\DataCache";
	    		starter.ProjectProperties.Starter.SetSymbols("USD/JPY");
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Minute1;
				
	    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
				
				// Run the loader.
				ExampleBreakoutStopsLoader loader = new ExampleBreakoutStopsLoader();
	    		starter.Run(loader);
	
	    		// Get the stategy
	    		strategy = loader.TopModel as ExampleBreakoutStops;
	    		
	    		LoadTransactions();
	    		LoadTrades();
	    		LoadBarData();
			} catch( Exception ex) {
				log.Error("Setup error.", ex);
				throw;
			}
		}
		#endregion
		
		
		[Test]
		public void VerifyCurrentEquity() {
			Assert.AreEqual( Math.Round(3190D,2),Math.Round(strategy.Performance.Equity.CurrentEquity,2),"current equity");
		}
		[Test]
		public void VerifyOpenEquity() {
			Assert.AreEqual( Math.Round(670D,2),Math.Round(strategy.Performance.Equity.OpenEquity,2),"open equity");
		}
		[Test]
		public void VerifyClosedEquity() {
			Assert.AreEqual( Math.Round(2520D,2),Math.Round(strategy.Performance.Equity.ClosedEquity,2),"closed equity");
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
		
		[Test]
		public void VerifyBarData() {
			VerifyBarData(strategy);
		}
		
		[Test]
		public void VerifyBarDataCount() {
			VerifyBarDataCount(strategy);
		}
		
		[Test]
		public void CompareBars() {
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}
