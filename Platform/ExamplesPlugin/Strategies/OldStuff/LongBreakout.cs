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
	public class LongBreakout : Strategy
	{
		Interval indicatorTimeFrame;
		SMA sma;
		int indicatorTimePeriod;
		int breakoutLength = 4;
		Color indicatorColor = Color.White;
		int averageLength = 32;
		int displace = 2;
		int rewardFactor = 2;
		int hourPeriod = 1;
		int historicalReward = 4500;
		
		public LongBreakout()
		{
			// Set defaults here.
			indicatorTimeFrame = IntervalDefault;
	    	Drawing.Color = Color.Green;
	    	IndicatorColor = Color.Green;
	    	IntervalDefault = Intervals.Day1;
		}
		
		public override void OnInitialize() {
			sma = new SMA(Bars.Close,averageLength);
			sma.IntervalDefault = IntervalDefault;
			AddIndicator(sma);
		}
		int barCount = 0;
		public override bool OnIntervalClose(Interval timeFrame)
		{
			if( timeFrame.BarUnit == BarUnit.Hour && timeFrame.Period == hourPeriod ) {
				barCount++;
				if( Bars.BarCount > 10) {
					HandleEntry();
					HandleExit();
				}
			}
			return true;
		}
		
		void HandleEntry() {
			double resistance = Formula.Highest(Bars.High,breakoutLength,displace+1);
			if( Bars.Typical[1] > resistance && Ticks[0].Bid > resistance ) {
				Orders.Enter.ActiveNow.BuyMarket();
			}
		}
		
		void HandleExit() {
			if( Position.IsLong) {
				if( Hours.Typical[0] < sma[0]) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
		}
		
		public int IndicatorTimePeriod {
			get { return indicatorTimePeriod; }
			set { indicatorTimePeriod = value; }
		}
		
		public int BreakoutLength {
			get { return breakoutLength; }
			set { breakoutLength = value; }
		}

		public override string ToString()
		{
			return rewardFactor + "," + displace + "," + breakoutLength + "," + averageLength + "," + barCount;
		}
		
		public Color IndicatorColor {
			get { return indicatorColor; }
			set { indicatorColor = value; }
		}
		
		public int AverageLength {
			get { return averageLength; }
			set { averageLength = value; }
		}

		public int Displace {
			get { return displace; }
			set { displace = value; }
		}
		
		public int RewardFactor {
			get { return rewardFactor; }
			set { rewardFactor = value; }
		}
		
		public int HistoricalReward {
			get { return historicalReward; }
			set { historicalReward = value; }
		}
		
		public int HourPeriod {
			get { return hourPeriod; }
			set { hourPeriod = value; }
		}
	}
}
