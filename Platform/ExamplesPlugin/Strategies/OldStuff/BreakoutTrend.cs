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
	/// Description of RandomStrategy.
	/// </summary>
	public class BreakoutTrend : Strategy
	{
		Interval indicatorTimeFrame;
		int length = 0;
		Color indicatorColor = Color.Blue;
		int trendStrength = 5;
		int lastBreakOut = 0;
		
		public BreakoutTrend()
		{
			// Set defaults here.
			indicatorTimeFrame = IntervalDefault;
		}
		
		public override bool OnIntervalClose() {
			if( Bars.High[0] > Formula.Highest(Bars.High,length)) {
				Orders.Enter.ActiveNow.SellMarket();
				lastBreakOut = Bars.CurrentBar;
			} else if( Bars.Low[0] < Formula.Lowest(Bars.Low,length)) {
				Orders.Enter.ActiveNow.BuyMarket();
				lastBreakOut = Bars.CurrentBar;
			}
			double median = (Bars.High[0] + Bars.Low[0])/2;
			if( Position.IsLong) {
				if( median < Position.Price) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
			if( Position.IsShort) {
				if( median > Position.Price) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
		
			// If too long since last break out, go flat.
			if( Bars.CurrentBar - lastBreakOut >= trendStrength ) {
				Orders.Exit.ActiveNow.GoFlat();
			}
			
			
//			// If child strategy, make sure signal direction matches.
			if( Next != null && Next.Position.Current != Position.Current) {
				Orders.Exit.ActiveNow.GoFlat();
			}
			return true;
		}
		
		public int Length {
			get { return length; }
			set { length = value; }
		}

		public override string ToString()
		{
			return length + "," + trendStrength;
		}
		
		public Color PivotColor {
			get { return indicatorColor; }
			set { indicatorColor = value; }
		}
		
		public int TrendStrength {
			get { return trendStrength; }
			set { trendStrength = value; }
		}

	}
}
