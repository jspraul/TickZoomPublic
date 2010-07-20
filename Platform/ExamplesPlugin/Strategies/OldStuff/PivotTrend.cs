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
	public class PivotTrend : Strategy
	{
		PivotLowVs pivotLow;
		PivotHighVs pivotHigh;
		Interval pivotTimeFrame;
		int leftStrength = 2; // Weeks
		int rightStrength = 10; // Days
		int length = 0;
		Color pivotColor = Color.Blue;
		int trendStrength = 5;
		int lastBreakOut = 0;
		
		public PivotTrend()
		{
			// Set defaults here.
			pivotTimeFrame = IntervalDefault;
		}
		
		public override void OnInitialize() {
			pivotLow = new PivotLowVs(leftStrength,rightStrength);
			pivotLow.IntervalDefault = pivotTimeFrame;
			pivotLow.Drawing.Color = pivotColor;
			pivotHigh = new PivotHighVs(leftStrength,rightStrength);
			pivotHigh.IntervalDefault = pivotTimeFrame;
			pivotHigh.Drawing.Color = pivotColor;
			AddIndicator( pivotLow);
			AddIndicator( pivotHigh);
		}
		
		public override bool OnIntervalClose() {
			if( pivotHigh.Count > 1 && pivotLow.Count > 1 ) {
				if( Bars.High[0] > pivotHigh[0]) {
					Orders.Enter.ActiveNow.BuyMarket();
					lastBreakOut = Bars.CurrentBar;
				} else if( Bars.Low[0] < pivotLow[0]) {
					Orders.Enter.ActiveNow.SellMarket();
					lastBreakOut = Bars.CurrentBar;
				}
			}
			// If too long since last break out, go flat.
			if( Bars.CurrentBar - lastBreakOut >= trendStrength ) {
				Orders.Exit.ActiveNow.GoFlat();
			}
			// If child strategy, make sure signal direction matches.
			if( Next != null && Next.Position.Current != Position.Current) {
				Orders.Exit.ActiveNow.GoFlat();
			}
			return true;
		}
		
		public Interval PivotTimeFrame {
			get { return pivotTimeFrame; }
			set { pivotTimeFrame = value; }
		}
		
		public int LeftStrength {
			get { return leftStrength; }
			set { leftStrength = value; }
		}
		
		public int RightStrength {
			get { return rightStrength; }
			set { rightStrength = value; }
		}
		
		public int Length {
			get { return length; }
			set { length = value; }
		}

		public override string ToString()
		{
			return leftStrength + "," + rightStrength + "," + trendStrength;
		}
		
		public Color PivotColor {
			get { return pivotColor; }
			set { pivotColor = value; }
		}
		
		public int TrendStrength {
			get { return trendStrength; }
			set { trendStrength = value; }
		}

	}
}
