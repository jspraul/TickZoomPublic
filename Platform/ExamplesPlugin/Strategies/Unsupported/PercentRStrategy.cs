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
	public class PercentRStrategy : Strategy
	{
		int slow=14;
		int fast=14;
		TEMA tema;
		SMA sma;
		IndicatorCommon percentR;
		Color indicatorColor = Color.Blue;
		int threshhold = 20;
		
		public PercentRStrategy()
		{
		}
		
		public override void OnInitialize() {
			sma = new SMA(Bars.Close,slow);
			sma.IntervalDefault = IntervalDefault;
			percentR = new IndicatorCommon();
			tema = new TEMA(Bars.Close,slow);
			AddIndicator(tema);
			AddIndicator(percentR);
			AddIndicator(sma);
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( timeFrame.Equals(IntervalDefault)) {
				double high = Formula.Highest(Bars.High,slow+1);
				double low = Formula.Lowest(Bars.Low,slow+1);
				int current = (int) tema[0];
				if( sma.Count > slow) {
					if( Next.Position.Current > 0) low = (int) Math.Min(sma[0],Bars.Low[0]);
					if( Next.Position.Current < 0) high = (int) Math.Max(sma[0],Bars.High[0]);
				}
				if( percentR.Count > 5) {
					percentR[0] = (current - low) * 100 / (high - low + 1);
				} else {
					percentR[0] = 50;
				}
				if( Next.Position.Current == 0) { 
					if( percentR[0] > 100 - threshhold) { Orders.Enter.ActiveNow.SellMarket(); }
					if( percentR[0] < threshhold) { Orders.Enter.ActiveNow.BuyMarket(); }
				} else {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
			return true;
		}
		
		public int Slow {
			get { return slow; }
			set { slow = value; }
		}

		public override string ToString()
		{
			return slow + "," + threshhold;
		}
		
		public Color IndicatorColor {
			get { return indicatorColor; }
			set { indicatorColor = value; }
		}
		
		public int Fast {
			get { return fast; }
			set { fast = value; }
		}
		
		public int Threshhold {
			get { return threshhold; }
			set { threshhold = value; }
		}
	}
}
