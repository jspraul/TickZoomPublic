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
	/// This strategy checks once per day to setup a trade when
	/// the typical daily price crosses over last week's high.
	/// 
	/// It checks every minute to trigger a long when the minute low
	/// crosses below last weeks low.  That gives a better entry for a long.
	/// 
	/// It exits when the daily typical price crosses below last week's low.
	/// 
	/// And it has a stop loss.
	/// 
	/// 	/// </summary>
	public class SRShortChannel : Strategy
	{
		int profitTarget = 1000;
		IndicatorCommon weekHigh;
		IndicatorCommon halfLine;
		IndicatorCommon weekLow;
		int length = 4;
		
		public SRShortChannel()
		{
		}
		
		public override void OnInitialize() {
			weekHigh = new WeekHigh();
			AddIndicator(weekHigh);
			
			weekLow = new WeekLow();
			AddIndicator(weekLow);
			
			halfLine = new WeekMiddle();
			AddIndicator(halfLine);
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			weekHigh[0] = Weeks.High[1];
			weekLow[0] = Weeks.Low[1];
			halfLine[0] = (Math.Max(Weeks.High[0],Weeks.High[1]) + Math.Min(Weeks.Low[0],Weeks.Low[1]))/2;
			if( timeFrame.Equals(Intervals.Minute1)) {
				if( Position.IsFlat && Hours.High[length] < weekHigh[length] &&
				    Formula.CrossesUnder( Hours.High, 1, Weeks.High[1])) {
					Orders.Enter.ActiveNow.SellMarket();
				}
				if( Position.HasPosition && Formula.CrossesUnder( Minutes.Low, 1, Weeks.Low[1])) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
			return true;
		}
		
		public int ProfitTarget {
			get { return profitTarget; }
			set { profitTarget = value; }
		}
		
		public int Length {
			get { return length; }
			set { length = value; }
		}
	}
}
