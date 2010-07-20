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
	public class ShortBreakout : Strategy
	{
		int breakoutLength = 5;
		int averageLength = 15;
		Color indicatorColor = Color.Blue;
		int stopLoss = 400;

		SMA sma;
		
		public ShortBreakout()
		{
		}
		
		public override void OnInitialize() {
			sma = new SMA(Bars.Close,averageLength);
			AddIndicator( sma);
		}
		
		public override bool OnIntervalClose() {
			if( Bars.Count > 1) {
				if( Position.IsShort && Bars.Typical[1] > Formula.Highest(Bars.High,breakoutLength,2)) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
			return true;
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( timeFrame.Equals(Intervals.Hour1) && Bars.Count > 1 ) {
				if( Position.IsFlat) {
					if( Hours.Typical[1] < sma[0]) {
						Orders.Enter.ActiveNow.SellMarket();
					}
				}
			}
			return true;
		}
		
		public int BreakoutLength {
			get { return breakoutLength; }
			set { breakoutLength = value; }
		}

		public override string ToString()
		{
			return breakoutLength + "," + averageLength + "," + stopLoss;
		}
		
		public int AverageLength {
			get { return averageLength; }
			set { averageLength = value; }
		}
		
		public int StopLoss {
			get { return stopLoss; }
			set { stopLoss = value; }
		}
		
	}
}
