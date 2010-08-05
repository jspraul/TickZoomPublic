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
	public class SRLongHourly : Strategy
	{
		IndicatorCommon retrace;
		IndicatorCommon reboundPercent;
		int maximumStretch = 1700;
		int stretchFloor = 1000;
		int stretchDivisor = 95;
		public SRLongHourly()
		{
			ExitStrategy.ControlStrategy  = false;
			IntervalDefault = Intervals.Minute10;
		}
		
		public override void OnInitialize()
		{
			base.OnInitialize();
			retrace = new IndicatorCommon();
			retrace.Drawing.Color = Color.Red;
			retrace.IntervalDefault = Intervals.Hour1;
			AddIndicator(retrace);
			
			reboundPercent = new IndicatorCommon();
			reboundPercent.Drawing.Color = Color.Red;
			reboundPercent.IntervalDefault = Intervals.Hour1;
			reboundPercent.Drawing.PaneType = PaneType.Secondary;
			AddIndicator(reboundPercent);
		}
		
		double lowest;
		double highest;
		int maxSize = 0;
		public override bool OnIntervalOpen(Interval timeFrame) {
			if( timeFrame.Equals(Intervals.Month1)) {
				highest = Hours.High[0];
				lowest = Hours.Low[0];
				retrace[0] = (highest + lowest)/2;
			}
			return true;
		}
		public void reset() {
			maxSize = 0;
			highest = Hours.High[0];
			lowest = Hours.Low[0];
			retrace[0] = (highest + lowest)/2;
		}
		public override bool OnIntervalClose(Interval timeFrame) {
			base.OnIntervalClose(timeFrame);
			if( Days.Count < 3) { return true; }
			
			if( timeFrame.Equals(Intervals.Hour1)) {
				
				if( double.IsNaN(retrace[0]	) ) {
					lowest = highest = retrace[0] = (Hours.Low[0] + Hours.High[0])/2;
					
				} else {
					if( Hours.High[0] > highest) {
						retrace[0] += Math.Max( Hours.High[0] - highest,0)/2;
						highest = Hours.High[0];
					}
					if( Hours.Low[0] < lowest) {
						retrace[0] -= Math.Max( lowest - Hours.Low[0],0)/2;
						lowest = Hours.Low[0];
					}
					if( Formula.CrossesUnder(Hours.Low,retrace[0]) || Formula.CrossesOver(Hours.High,retrace[0])) {
						highest = Hours.High[0];
						lowest = Hours.Low[0];
					}
				}
			}

			if( timeFrame.Equals(Intervals.Minute1)) {
				int stretch = (int) (highest - lowest)/2;
				int rebound = (int) Math.Max(0,stretch-Math.Abs(Bars.Close[0] - retrace[0]));
				reboundPercent[0] = Math.Min(100,rebound*100/(highest - lowest));
				int positionsize = Math.Max(0,(stretch/100)-2);
				int reboundThreshold = Math.Max(0,45 - Math.Max(0,stretch-stretchFloor)/stretchDivisor*5);
				
				if( stretch > maximumStretch) {
					Orders.Exit.ActiveNow.GoFlat();
					reset();
					return true;
				}

				if( Minutes.Close[0] < retrace[0]) {
					if( positionsize > maxSize) {
						Orders.Enter.ActiveNow.BuyMarket( Position.Size+1);
						maxSize = positionsize;
					}
				} 
				if( Position.IsLong && reboundPercent[0] > reboundThreshold) {
					Orders.Exit.ActiveNow.GoFlat();
					reset();
					return true;
				}
				
				if( Minutes.Close[0] > retrace[0]) {
					if( positionsize > maxSize) {
						Orders.Enter.ActiveNow.SellMarket( Position.Size+1);
						maxSize = positionsize;
					}
				} 
				if( Position.IsShort && reboundPercent[0] > reboundThreshold) {
					Orders.Exit.ActiveNow.GoFlat();
					reset();
					return true;
				}
				
				if( Position.IsFlat) { maxSize = 0; }
			}
			return true;
		}
		
		public int MaximumStretch {
			get { return maximumStretch; }
			set { maximumStretch = value; }
		}
		
		public int StretchFloor {
			get { return stretchFloor; }
			set { stretchFloor = value; }
		}
		
		public int StretchDivisor {
			get { return stretchDivisor; }
			set { stretchDivisor = value; }
		}
	}
}
