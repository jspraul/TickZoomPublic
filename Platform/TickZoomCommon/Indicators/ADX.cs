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
	/// The Average Directional Movement Index (ADX) is a 
	/// momentum indicator developed by J. Welles Wilder and described in his 
	/// book "New Concepts in Technical Trading Systems", written in 1978. 
	/// The ADX is constructed from two other Wilders' indicators: the Positive Directional indicator (+DI) 
	/// and the Negative Directional Indicator (-DI). The +DI and -DI indicators are commonly referred to 
	/// as the Directional Movement Index. Combining the +/-DI and applying a Wilders() smoothing 
	/// filter results in the final ADX value.
	/// </summary>
	public class ADX : IndicatorCommon
	{
		double period = 14;
		double prevBarOpen;
		double prevBarHigh;
		double prevBarLow;
		double prevBarClose;

		IndicatorCommon dmPlus;
		IndicatorCommon dmMinus;
		IndicatorCommon sumDmPlus;
		IndicatorCommon sumDmMinus;
		IndicatorCommon sumTr;
		IndicatorCommon tr;
		
		public ADX(int period)
		{
			Drawing.Color = Color.Green;
 			Drawing.PaneType = PaneType.Secondary;
			Drawing.IsVisible = true;
			Drawing.GroupName = "ADX";
			Drawing.GraphType = GraphType.Line;

			dmPlus = Formula.Indicator();
			dmMinus	= Formula.Indicator();
			sumDmPlus = Formula.Indicator();
			sumDmMinus = Formula.Indicator();
			sumTr = Formula.Indicator();
			tr = Formula.Indicator();
			this.period = period;
		}
		
		public override void Update() {
			double trueRange = Bars.High[0] - Bars.Low[0];
			if (Count == 1)
			{
				tr.Add(trueRange);
				dmPlus.Add(0);
				dmMinus.Add(0);
				sumTr.Add(tr[0]);
				sumDmPlus.Add(dmPlus[0]);
				sumDmMinus.Add(dmMinus[0]);
				Add(50.0);
			}
			else
			{
				tr.Add(Math.Max(Math.Abs(Bars.Low[0] - prevBarClose), Math.Max(trueRange, Math.Abs(Bars.High[0] - prevBarClose))));
				dmPlus.Add(Bars.High[0] - prevBarHigh > prevBarLow - Bars.Low[0] ? Math.Max(Bars.High[0] - prevBarHigh, 0) : 0);
				dmMinus.Add(prevBarLow - Bars.Low[0] > Bars.High[0] - prevBarHigh ? Math.Max(prevBarLow - Bars.Low[0], 0) : 0);
				sumTr.Add(0);
				sumDmPlus.Add(0);
				sumDmMinus.Add(0);

				if (Count < period )
				{
					sumTr[0] = (sumTr[1] + tr[0]);
					sumDmPlus[0] = (sumDmPlus[1] + dmPlus[0]);
					sumDmMinus[0] = (sumDmMinus[1] + dmMinus[0]);
				}
				else
				{
					sumTr[0] = (sumTr[1] - sumTr[1] / period + tr[0]);
					sumDmPlus[0] = (sumDmPlus[1] - sumDmPlus[1] / period + dmPlus[0]);
					sumDmMinus[0] = (sumDmMinus[1] - sumDmMinus[1] / period + dmMinus[0]);
				}

				double diPlus	= 100.0 * (sumTr[0] == 0 ? 0 : sumDmPlus[0] / sumTr[0]);
				double diMinus	= 100.0 * (sumTr[0] == 0 ? 0 : sumDmMinus[0] / sumTr[0]);
				double diff		= Math.Abs(diPlus - diMinus);
				double sum		= diPlus + diMinus;

				Add(sum == 0 ? 50.0 : ((period - 1.0) * this[0] + 100.0 * diff / sum) / period);
			}
			prevBarOpen = Bars.Open[0];
			prevBarHigh = Bars.High[0];
			prevBarLow = Bars.Low[0];
			prevBarClose = Bars.Close[0];
		}
				
		public double Period {
			get { return period; }
			set { period = value; }
		}
	}
}
