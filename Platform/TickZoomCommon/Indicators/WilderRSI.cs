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
	/// Relative Strength Index. Introduced by J. Wells Wilder in his 1978 book, 
	/// �New Concepts in Technical Trading Systems�. The RSI compares a trading 
	/// instrument�s magnitude of recent gains against its magnitude of recent losses 
	/// and quantifies this information into a value that ranges between 0 and 100.
	/// FB 20091230: complete rewrite using Wilder's EMA instead of SMA
	/// </summary>
	public class WilderRSI : IndicatorCommon
	{
		int period = 13;
		IndicatorCommon mom;
		IndicatorCommon gain;
		IndicatorCommon loss;
		IndicatorCommon avgGain;
		IndicatorCommon avgLoss;
		//Wilder avgGain;
		//Wilder avgLoss;
		
		public WilderRSI(object anyPrice, int period)
		{
			this.period = period;
			StartValue = 0;
			AnyInput = anyPrice;
		}
		
		public override void OnInitialize() {
			Name = "Wilder's RSI";
			Drawing.Color = Color.Black;
			Drawing.PaneType = PaneType.Secondary;
			Drawing.IsVisible = true;
			Formula.Line(70, Color.Red);
			Formula.Line(30, Color.Green);
			Drawing.ScaleMax = 100;
			Drawing.ScaleMin = 0;
			
			mom = Formula.Indicator();
			gain = Formula.Indicator();
			loss = Formula.Indicator();
			avgGain = Formula.Indicator();
			avgLoss = Formula.Indicator();
		}
		
		public override void Update() {
			if (Count == 1 ) {
				this[0] = 50;
			} else {
				mom[0] = Input[0] - Input[1];
				if (mom[0] > 0.0) gain[0] = mom[0];
				else if (mom[0] < 0.0) loss[0] = -mom[0];
				avgGain[0] = (gain[0] + (period - 1D) * gain[1]) / period; // Wilder's MA
				avgLoss[0] = (loss[0] + (period - 1D) * loss[1]) / period; // Wilder's MA
				//avgGain = new Wilder(gain, period);
				//avgLoss = new Wilder(loss, period);
				this[0] = 100 - (100 / ((avgGain[0]/avgLoss[0]) + 1D));
			}
		}
		
		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
