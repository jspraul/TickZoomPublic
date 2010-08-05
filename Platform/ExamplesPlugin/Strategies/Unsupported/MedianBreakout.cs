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
	public class MedianBreakout : Strategy
	{
		int breakoutLength = 0;
		int averageLength = 0;
		Color indicatorColor = Color.Blue;
		double entryPrice;
		int stopLoss = 400;
//		int riskLimit = 400;

		SMA sma;
		
		public MedianBreakout()
		{
		}
		
		public override void OnInitialize() {
			sma = new SMA(Bars.Close,averageLength);
			AddIndicator( sma);
		}
		
		public override bool OnIntervalClose() {
			if( Bars.BarCount > 1) {
				int risk = (int) (Ticks[0].Bid - sma[0]);
				if( Position.IsFlat && Bars.Typical[1] > Formula.Highest(Bars.High,breakoutLength,2)) {
//					risk < riskLimit &&	risk > 0) {
					
					Orders.Enter.ActiveNow.BuyMarket();
					entryPrice = Ticks[0].Bid;
				}
				
			}
			return true;
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( timeFrame.BarUnit == BarUnit.Hour && timeFrame.Period == 1 && Bars.Count > 1 ) {
				if( Position.IsLong) {
					if( Hours.Typical[1] < sma[0] - stopLoss) {
						Orders.Exit.ActiveNow.GoFlat();
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
