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
	public class Retrace : IndicatorCommon
	{
		double lowest;
		double highest;
		double stretch;
		double adjustPercent = 0.50;
//		int rebound;
		
		public Retrace()
		{
		}
		
		public override void OnInitialize() {
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			Tick prevTick = Ticks[1];
			double middle = (tick.Ask + tick.Bid)/2;
			if( Hours.BarCount <= 2) { Reset(); return true; }
			if( double.IsNaN(this[0]) ) {
				this[0] = middle;
				lowest = highest = (int) this[0];
			} else {
				if( middle > highest) {
					this[0] += Math.Max( middle - highest,0)*adjustPercent;
					highest = middle;
				}
				if( middle < lowest) {
					this[0] -= Math.Max( lowest - middle,0)*adjustPercent;
					lowest = middle;
				}
				if( (tick.Ask < this[0] && prevTick.Ask >= this[0]) ||
				    (tick.Bid > this[0] && prevTick.Bid <= this[0])) {
					highest = middle;
					lowest = middle;
				}
			}
			stretch = highest - lowest;
//			rebound = (int) Math.Max(0,stretch-Math.Abs(middle - this[0]));
			return true;
		}

		public int Rebound(int price) {
			int rebound = (int) Math.Max(0,stretch-Math.Abs(price - this[0]));
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
		
		public double RetracePercent {
			get { return 1 - adjustPercent; }
			set { adjustPercent = 1 - value; }
		}
	}
}
