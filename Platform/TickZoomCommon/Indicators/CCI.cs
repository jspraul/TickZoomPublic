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
	/// The Commodity Channel Index (CCI) examines the variation
	/// of a price from the mean. High values indicate that prices
	/// are very high compared to the average whereas low values
	/// point out that prices are considerably lower than average.
	/// </summary>
	public class CCI : IndicatorCommon
	{
		int	period = 14;
		SMA sma;

		public CCI(object anyPrice, int period)
		{
			AnyInput = anyPrice;
			StartValue = 0;
			this.period = period;
		}
		
		public override void OnInitialize()
		{
			Name = "CCI_New";
			Drawing.Color = Color.Orange;
			Drawing.PaneType = PaneType.Secondary;
			Drawing.IsVisible = true;
			
			Formula.Line(200, Color.DarkGray, "Level 2");
			Formula.Line(100, Color.DarkGray, "Level 1");
			Formula.Line(0,   Color.DarkGray, "Zero line");
			Formula.Line(-100, Color.DarkGray, "Level -1");
			Formula.Line(-200, Color.DarkGray, "Level -2");
			sma = Formula.SMA(Input, Period);
		}
		
		public override void Update()
		{
			double mean = 0;
			for (int idx = Math.Min(Input.CurrentBar, Period - 1); idx >= 0; idx--)
				mean += Math.Abs(Input[idx] - sma[0]);
			this[0] = (Input[0] - sma[0]) / (mean == 0 ? 1 : (0.015 * (mean / Math.Min(Period, Input.CurrentBar + 1))));
			double result = this[0];
		}

		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
