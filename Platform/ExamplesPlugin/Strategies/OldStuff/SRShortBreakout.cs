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
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// This strategy checks every 10 minutes to setup a trade when
	/// the typical daily price falls below last weeks low.
	/// 
	/// It triggers a short if that typical daily price is STILL
	/// below 10 minutes later.
	/// 
	/// It exits when the daily typical price crosses over last weeks high.
	/// 
	/// And it has a stop loss.
	/// 
	/// </summary>
	public class SRShortBreakout : Strategy
	{
		IndicatorCommon weekMiddle;
		public SRShortBreakout()
		{
		}
		
		public override void OnInitialize() {
			IntervalDefault = Intervals.Day1;
			
			weekMiddle = new WeekMiddle();
			AddIndicator(weekMiddle);
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( Weeks.Low.Count < 2) return true;
			if( timeFrame.Equals(Intervals.Month1)) {
				if( Position.HasPosition) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}			
			
			if( timeFrame.Equals(Intervals.Minute1)) {
				if( Position.IsFlat && Formula.CrossesUnder( Bars.Typical,1,Weeks.Low[1])) {
					Orders.Enter.ActiveNow.SellMarket();
				}
				if( Position.HasPosition && Formula.CrossesOver( Bars.Typical,1, Weeks.High[1])) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
			return true;
		}
	}
}
