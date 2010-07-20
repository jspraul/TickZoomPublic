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
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Introduced by Welles Wilder in his book "New Concepts in Technical Trading Systems" (1978), 
	/// the Average True Range (ATR) is a measure of a trading instrument's volatility. It measures 
	/// the degree of price movement, not the direction or duration of the price movement.
	/// </summary>
	public class ATR : IndicatorCommon
	{
		int period;
		SMA atr;
		
		public ATR(int period)
		{
			this.period = period;
			Drawing.PaneType = PaneType.Secondary;
		}
		
		public override void OnInitialize()
		{
			Name = "ATR";
			Drawing.Color = Color.Red;
			Drawing.PaneType = PaneType.Secondary;
			Drawing.IsVisible = true;
		}
		
		public override void Update() {
			double trueRange = Math.Max(Bars.High[0] - Bars.Low[0], Math.Max(Math.Abs(Bars.High[0] - Bars.Close[1]), Math.Abs(Bars.Close[1] - Bars.Low[0])));
			atr = new SMA(trueRange, period);
			this[0] = atr[0];
		}
		
		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
