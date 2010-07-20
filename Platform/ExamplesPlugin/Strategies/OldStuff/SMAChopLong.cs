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
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of RandomStrategy.
	/// </summary>
	public class SMAChopLong : Strategy
	{
		int slow = 14;
		int lowTrigger = 220;
		int highTrigger = 450;
		SMA slowAvg;
		IndicatorCommon diff;
		int maxHours = 5;
		
		public SMAChopLong()
		{
	    	Drawing.Color = Color.Orange;
	    	IntervalDefault = Intervals.Hour4;
		}
		
		public override void OnInitialize()
		{
			slowAvg = new SMA(Bars.Close,slow);
			diff = new IndicatorCommon();
			diff.Drawing.PaneType = PaneType.Secondary;

			AddIndicator(slowAvg);
			AddIndicator(diff);
		}
		
		public override string ToString() {
			return slow + "," + lowTrigger + "," + highTrigger;
		}
		
		public override bool OnIntervalClose() {
			if( Next.Position.IsFlat || Next.Position.IsLong) {
				diff[0] = Bars.Typical[0] - slowAvg[0];
				if( diff[0] < -lowTrigger) {
					Orders.Enter.ActiveNow.BuyMarket();
				}
				if( diff[0] > highTrigger) {
					Orders.Exit.ActiveNow.GoFlat();
				}
				Elapsed span = Position.Time - Bars.Time[0];
				if( span.TotalHours > maxHours) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			} else {
				Orders.Exit.ActiveNow.GoFlat();
			}
			return true;
		}

		public int Slow {
			get { return slow; }
			set { slow = value; }
		}		
		public int LowTrigger {
			get { return lowTrigger; }
			set { lowTrigger = value; }
		}
		
		public int HighTrigger {
			get { return highTrigger; }
			set { highTrigger = value; }
		}
		
		public int MaxHours {
			get { return maxHours; }
			set { maxHours = value; }
		}
	}
}
