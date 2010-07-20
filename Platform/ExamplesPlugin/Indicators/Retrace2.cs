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
using TickZoom.Api;
using System.Drawing;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of SMA.
	/// </summary>
	public class Retrace2 : IndicatorCommon
	{
		double lowest;
		double highest;
		double stretch;
		int rebound;
		
		public Retrace2()
		{
		}
		
		public override void OnInitialize() {
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( Bars.BarCount <= 2) { Reset(); return true; }
			if( timeFrame.Equals(Intervals.Minute1)) {
				if( double.IsNaN(this[0]) ) {
					this[0] = (Bars.Low[0] + Bars.High[0])/2;
					lowest = highest = (int) this[0];
				} else {
					if( Bars.High[0] > highest) {
						this[0] += Math.Max( Bars.High[0] - highest,0)/2;
						highest = Bars.High[0];
					}
					if( Bars.Low[0] < lowest) {
						this[0] -= Math.Max( lowest - Bars.Low[0],0)/2;
						lowest = Bars.Low[0];
					}
					if( Formula.CrossesUnder(Bars.Low,this[0]) || Formula.CrossesOver(Bars.High,this[0])) {
						highest = Bars.High[0];
						lowest = Bars.Low[0];
					}
				}
				stretch = highest - lowest;
				rebound = (int) Math.Max(0,stretch-Math.Abs(Bars.Close[0] - this[0]));
			}
			return true;
		}

		public int Rebound(int price) {
			rebound = (int) Math.Max(0,stretch-Math.Abs(price - this[0]));
			return rebound;
		}

		public double Stretch {
			get { return stretch; }
		}

		public void Reset() {
			highest = Bars.High[0];
			lowest = Bars.Low[0];
			this[0] = (highest + lowest)/2;
		}
		
	}
}
