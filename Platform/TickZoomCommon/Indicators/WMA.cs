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
	/// Weighted Moving Average (WMA). The Weighted Moving Average weights the more recent value greater
	/// that prior value. For 5 bar WMA the current bar is multiplied by 5, the previous
	/// one by 4, and so on, down to 1 for the final value. WMA divides the result by the
	/// total of the multipliers for the average.
	/// </summary>
	public class WMA : IndicatorCommon
	{
		int period = 14;
		
		public WMA(object anyPrice, int period)
		{
			AnyInput = anyPrice;
			StartValue = 0;
			this.period = period;
		}
		
		public override void OnInitialize() {
			Name = "WMA_New";
			Drawing.Color = Color.Brown;
			Drawing.PaneType = PaneType.Primary;
			Drawing.IsVisible = true;
		}

		public override void Update() {
			double sum = 0;
			int count = 0;
			if( Count == 1) {
				this[0] = Input[0];
			} else {
				for( int i = 0; i< period; i++) {
					int mult = period - i;
					sum += Input[i] * (period - i);
					count += period - i;
				}
				this[0] = sum / count;
			}
		}
		
		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
