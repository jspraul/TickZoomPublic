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
using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Developed by Larry Williams, the Williams %R (pronounced "percent R") indicator 
	/// is a momentum oscillator used to measure overbought and oversold levels. It's very 
	/// similar to the Stochastic Oscillator except that the %R is plotted upside-down on 
	/// a negative scale from 0 to -100 and has no internal smoothing.
	/// </summary>
	public class PercentR : IndicatorCommon
	{
		int period = 13;
		
		public PercentR(int period)
		{
			this.period = period;
		}
		
		public override void OnInitialize() {
			Drawing.Color = Color.Blue;
			Drawing.PaneType = PaneType.Secondary;
			Drawing.IsVisible = true;
			Drawing.ScaleMax = 0;
			Drawing.ScaleMin = -100;
		}
		
		public override void Update() {
			double highestH = Formula.Highest(Bars.High, period);
			double lowestL = Formula.Lowest(Bars.Low, period);
			double last = Bars.Close[1];
			this[0] = -100 * ((highestH - last) / (highestH - lowestL));
		}
		
		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
