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
using System.ComponentModel;
using NUnit.Framework;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;
using TickZoom.Starters;

namespace Loaders
{
	public class PacketFailureTest
	{
		StrategyTest strategyTest = new StrategyTest();
		Log log = Factory.SysLog.GetLogger(typeof(MarketOrderTest));
		
		public PacketFailureTest() {
			strategyTest.Symbols = "USDJPYBenchMark";
			strategyTest.StoreKnownGood = false;
			strategyTest.ShowCharts = false;
			strategyTest.CreateStarterCallback = CreateStarter;
		}
		public virtual Starter CreateStarter()
		{
			return new HistoricalStarter();
		}
			
		public void RunStrategy(BackgroundWorker bw) {
			strategyTest.RunStrategy();
			try {
				Starter starter = strategyTest.CreateStarterCallback();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(2010,06,10);
	    		
	    		starter.DataFolder = "TestData";
	    		starter.BackgroundWorker = bw;
	    		starter.ProjectProperties.Starter.SetSymbols(strategyTest.Symbols);
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Minute1;
	    		starter.CreateChartCallback = new CreateChartCallback(strategyTest.HistoricalCreateChart);
	    		starter.ShowChartCallback = new ShowChartCallback(strategyTest.HistoricalShowChart);
				// Run the loader.
				MarketOrderLoader loader = new MarketOrderLoader();
	    		starter.Run(loader);
	
			} catch( Exception ex) {
				log.Error("Setup error.", ex);
				throw;
			} finally {
				strategyTest.EndStrategy();
			}
		}
		
	}
}
