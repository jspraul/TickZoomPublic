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
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;

namespace Loaders
{
	[AutoTestFixture]
	public class AutoTests : IAutoTestFixture {
		public AutoTestSettings[] GetAutoTestSettings() {
			var list = new System.Collections.Generic.List<AutoTestSettings>();
			var storeKnownGood = false;			
			var primarySymbol = "USD/JPY";
			try { 
				list.Add( new AutoTestSettings {
				    Mode = AutoTestMode.All,
				    Name = "ApexStrategyTest",
				    Loader = Plugins.Instance.GetLoader("APX_Systems: APX Multi-Symbol Loader"),
					Symbols = primarySymbol + ",EUR/USD,USD/CHF",
					StoreKnownGood = storeKnownGood,
					ShowCharts = false,
					StartTime = new TimeStamp( 1800, 1, 1),
					EndTime = new TimeStamp( 2009, 6, 10),
					IntervalDefault = Intervals.Minute1,
				});
			} catch( ApplicationException ex) {
				if( !ex.Message.Contains("not found")) {
					throw;
				}
			}
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.All,
			    Name = "DualStrategyLimitOrder",
			    Loader = new TestDualStrategyLoader(),
				Symbols = primarySymbol + ",EUR/USD",
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.All,
			    Name = "LimitOrderTest",
			    Loader = new TestLimitOrderLoader(),
				Symbols = primarySymbol,
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.All,
			    Name = "MarketOrderTest",
			    Loader = new MarketOrderLoader(),
				Symbols = primarySymbol,
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.All,
			    Name = "SyntheticMarketOrderTest",
			    Loader = new MarketOrderLoader(),
				Symbols = "USD/JPY_Synthetic",
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.Historical,
			    Name = "ExampleReversalOnSimData",
			    Loader = new ExampleReversalLoader(),
				Symbols = "Daily4Sim",
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp(1800,1,1),
				EndTime = new TimeStamp(1990,1,1),
				IntervalDefault = Intervals.Day1,
			});
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.Historical,
			    Name = "ExampleMixedSimulated",
			    Loader = new ExampleMixedLoader(),
				Symbols = "FullTick,Daily4Sim",
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp(1800,1,1),
				EndTime = new TimeStamp(1990,1,1),
				IntervalDefault = Intervals.Day1,
			});
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.All,
			    Name = "ExampleMixedTest",
			    Loader = new ExampleMixedLoader(),
				Symbols = primarySymbol + ",EUR/USD,USD/CHF",
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			list.Add( new AutoTestSettings {
			    Mode = AutoTestMode.All,
			    Name = "ExampleReversalTest",
			    Loader = new ExampleReversalLoader(),
				Symbols = primarySymbol,
				StoreKnownGood = storeKnownGood,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			return list.ToArray();
		}
	}
}
