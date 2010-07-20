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
using TickZoom.TickUtil;
using TickZoom.Transactions;

namespace Loaders
{
	[TestFixture]
	public class CompareSimulatedTest
	{
		#region SetupTest
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Starter fullTickStarter;
		Starter fourTickStarter;
		ExampleOrderStrategy fourTicksPerBar;
		ExampleOrderStrategy fullTickData;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
			try {
				fourTickStarter = new HistoricalStarter(false);
				
				// Set run properties as in the GUI.
				fourTickStarter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		fourTickStarter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		fourTickStarter.DataFolder = "TestData";
	    		fourTickStarter.ProjectProperties.Starter.SetSymbols("Daily4Sim");
				fourTickStarter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				
				// Run the loader.
				ExampleLimitOrderLoader simulatedLoader = new ExampleLimitOrderLoader();
	    		fourTickStarter.Run(simulatedLoader);
	
	    		// Get the stategy
	    		fourTicksPerBar = simulatedLoader.TopModel as ExampleOrderStrategy;

	    		/// <summary>
	    		/// Now run the other strategy to compare results.
	    		/// </summary>
	    		
	    		fullTickStarter = new HistoricalStarter(false);
				
				// Set run properties as in the GUI.
				fullTickStarter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		fullTickStarter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		fullTickStarter.DataFolder = "TestData";
	    		fullTickStarter.ProjectProperties.Starter.SetSymbols("FullTick");
				fullTickStarter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				
				// Run the loader.
				ExampleOrdersLoader loader = new ExampleOrdersLoader();
	    		fullTickStarter.Run(loader);
	
	    		// Get the stategy
	    		fullTickData = loader.TopModel as ExampleOrderStrategy;
	    		
			} catch( Exception ex) {
				Console.Out.WriteLine(ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
				throw;
			}
    		
		}
		
		[TestFixtureTearDown]
		public void FixtureTearDown() {
			fourTickStarter.Release();
			fullTickStarter.Release();
		}
		#endregion
		
		[Test]
		public void CompareTradeCount() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			Assert.AreEqual(fourTicksRTs.Count,fullTicksRTs.Count, "trade count");
		}
			
		[Test]
		public void CompareTrades() {
			TransactionPairs fourTicksRTs = fourTicksPerBar.Performance.ComboTrades;
			TransactionPairs fullTicksRTs = fullTickData.Performance.ComboTrades;
			for( int i=0; i<fourTicksRTs.Count && i<fullTicksRTs.Count; i++) {
				TransactionPair fourRT = fourTicksRTs[i];
				TransactionPair fullRT = fullTicksRTs[i];
				double fourEntryPrice = Math.Round(fourRT.EntryPrice,2).Round();
				double fullEntryPrice = Math.Round(fullRT.EntryPrice,2).Round();
				Assert.AreEqual(fullEntryPrice,fourEntryPrice,"Entry Price for Trade #" + i);
				Assert.AreEqual(fullRT.ExitPrice,fourRT.ExitPrice,"Exit Price for Trade #" + i);
			}
		}
		
	}
}
