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

namespace Loaders
{
	[AutoTestFixture]
	public class AutoTests : IAutoTestFixture {
		public AutoTestSettings[] GetAutoTestSettings() {
			var list = new System.Collections.Generic.List<AutoTestSettings>();
			
			list.Add( new AutoTestSettings {
			    TestName = "ApexStrategyTest",
				LoaderName = "APX_Systems: APX Multi-Symbol Loader",
				Symbols = "USD/JPY",
				StoreKnownGood = false,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			list.Add( new AutoTestSettings {
			    TestName = "DualStrategyLimitOrder",
				LoaderName = "Test: Dual Strategy",
				Symbols = "USD/JPY,EUR/USD",
				StoreKnownGood = true,
				ShowCharts = false,
				StartTime = new TimeStamp( 1800, 1, 1),
				EndTime = new TimeStamp( 2009, 6, 10),
				IntervalDefault = Intervals.Minute1,
			});
			
			return list.ToArray();
		}
	}
}
