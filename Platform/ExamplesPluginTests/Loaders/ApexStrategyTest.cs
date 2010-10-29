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

namespace Loaders
{
	[TestFixture]
	public class ApexStrategyTest : StrategyTest
	{
		Log log = Factory.SysLog.GetLogger(typeof(ApexStrategyTest));
		Strategy strategy;
		private bool isIgnore = false;
		
		public ApexStrategyTest() {
			Symbols = "USD/JPY";
			StoreKnownGood = true;
			ShowCharts = false;
		}
			
		[TestFixtureSetUp]
		public override void RunStrategy() {
			base.RunStrategy();
			try {
				Starter starter = CreateStarterCallback();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(2009,06,10);
	    		
	    		starter.DataFolder = "Test\\DataCache";
	    		starter.ProjectProperties.Starter.SetSymbols( Symbols);
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Minute1;
	    		starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = new ShowChartCallback(HistoricalShowChart);
				// Run the loader.
				try { 
					var loader = Plugins.Instance.GetLoader("APX_Systems: APX Multi-Symbol Loader");
		    		starter.Run(loader);
		    		var portfolio = loader.TopModel as Portfolio;
		    		strategy = portfolio.Strategies[1];
				} catch( ApplicationException ex) {
					if( ex.Message.Contains("not found")) {
						isIgnore = true;
						return;
					}
				}
	
	    		// Get the stategy
	    		LoadTransactions();
	    		LoadTrades();
	    		LoadBarData();
			} catch( Exception ex) {
				log.Error("Setup error.", ex);
				throw;
			}
		}
		
		[Test]
		public void VerifyCurrentEquity() {
			if( isIgnore) return;
			Assert.AreEqual( Math.Round(9970.24,4),Math.Round(strategy.Performance.Equity.CurrentEquity,4),"current equity");
		}
		[Test]
		public void VerifyOpenEquity() {
			if( isIgnore) return;
			Assert.AreEqual( 0.00,strategy.Performance.Equity.OpenEquity,"open equity");
		}
		[Test]
		public void VerifyClosedEquity() {
			if( isIgnore) return;
			Assert.AreEqual( Math.Round(9970.24,4),Math.Round(strategy.Performance.Equity.ClosedEquity,4),"closed equity");
		}
		[Test]
		public void VerifyStartingEquity() {
			if( isIgnore) return;
			Assert.AreEqual( 10000.00,strategy.Performance.Equity.StartingEquity,"starting equity");
		}
		
		[Test]
		public void VerifyBarData() {
			if( isIgnore) return;
			VerifyBarData(strategy);
		}
		
		[Test]
		public void VerifyTrades() {
			if( isIgnore) return;
			VerifyTrades(strategy);
		}
	
		[Test]
		public void VerifyTradeCount() {
			if( isIgnore) return;
			VerifyTradeCount(strategy);
		}
		
		[Test]
		public void CompareBars() {
			if( isIgnore) return;
			CompareChart(strategy,GetChart(strategy.SymbolDefault));
		}
	}
}
