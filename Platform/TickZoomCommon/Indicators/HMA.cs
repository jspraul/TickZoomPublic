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
	/// The Hull Moving Average (HMA). Introduced by Alan Hull, is an extremely 
	/// fast and smooth moving average. HMA significantly decreases lag 
	/// and improve smoothing at the same time. The price we 
	/// pay for this performance is serious overshot.
	/// FB 20091230: Created
	/// </summary>
	public class HMA : IndicatorCommon
	{
		WMA W1;
		WMA W2;
		WMA W3;
		int period = 14;
		double halfPeriod;
		double sqrtPeriod;

		
		public HMA(object anyPrice, int period) {
			AnyInput = anyPrice;
			StartValue = 0;
			this.period = period;
		}

		public override void OnInitialize() {
			Name = "HMA";
			Drawing.Color = Color.Green;
			Drawing.PaneType = PaneType.Primary;
			Drawing.IsVisible = true;
			
			halfPeriod = (Math.Ceiling(Convert.ToDouble(Period*0.5)) - (Convert.ToDouble(Period*0.5)) <= 0.5) ? Math.Ceiling(Convert.ToDouble(Period*0.5)) : Math.Floor(Convert.ToDouble(Period*0.5));
			sqrtPeriod = (Math.Ceiling(Math.Sqrt(Convert.ToDouble(Period))) - Math.Sqrt(Convert.ToDouble(Period)) <= 0.5) ? Math.Ceiling(Math.Sqrt(Convert.ToDouble(Period))) : Math.Floor(Math.Sqrt(Convert.ToDouble(Period)));
			W1 = Formula.WMA(Input, Convert.ToInt32(halfPeriod));
			W2 = Formula.WMA(Input, Period);
			W3 = Formula.WMA(2D * W1[0] - W2[0], Convert.ToInt32(sqrtPeriod));
		}
				
		public override void Update() {
			if (Count == 1 ) {
				this[0] = Input[0];
			} else {
				this[0] = W3[0];
			}
		}

		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
