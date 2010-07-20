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
	public class SRLongBreakout2 : SRBreakoutSupport
	{
		public SRLongBreakout2()
		{
			ExitStrategy.ControlStrategy = true;
			IntervalDefault = Intervals.Day1;
			MinimumMove = 370;
			ExitStrategy.StopLoss = 300;
			ExitStrategy.TrailStop = 2500;
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			base.OnIntervalClose(timeFrame);
			
			if( timeFrame.Equals(Intervals.Minute1)) {
				
				// Long Trend
				if( LongTrend ) {
					if( Formula.CrossesOver(Minutes.High,Resistance[0])) {
						Orders.Enter.ActiveNow.BuyMarket();
					}
				} else {
					Orders.Exit.ActiveNow.GoFlat();
				}

				if( Position.IsLong) {
					//Low Exit
					if( Minutes.Low[0] < Support[0]) {
						Orders.Exit.ActiveNow.GoFlat();
					}
				}
				
			}
			return true;
		}
	}
}
