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
	/// T3 Moving Average (T3Average). Introduced by Tim Tillson in the January 1998 issue of 
	/// Technical Analysis of Stocks & Commodities article "Smoothing Techniques for More Accurate Signals". 
	/// Compared to TEMA or other EMA derivatives it is much smoother and exhibits less lag.
	/// FB 20100104: created
	/// </summary>
	public class T3Average : IndicatorCommon
	{
		int period = 13;
		double hot = 0.7;
		double halfPeriod;

		EMA E1, E2, E3, E4, E5, E6;

		public T3Average(object anyPrice, int period, double hot) {
			AnyInput = anyPrice;
			StartValue = 0;
			this.period = period;
			this.hot = hot;
		}
		
		public override void OnInitialize()	{
			Name = "T3Average";
			Drawing.Color = Color.Blue;
			Drawing.PaneType = PaneType.Primary;
			Drawing.GroupName = "T3Average";
			Drawing.GraphType = GraphType.Line;
			
			halfPeriod = (Math.Ceiling(Convert.ToDouble(Period*0.5)) - (Convert.ToDouble(Period*0.5)) <= 0.5) ? Math.Ceiling(Convert.ToDouble(Period*0.5)) : Math.Floor(Convert.ToDouble(Period*0.5));
			E1 = Formula.EMA(Input, Convert.ToInt32(halfPeriod));
			E2 = Formula.EMA(E1, Convert.ToInt32(halfPeriod));
			E3 = Formula.EMA(E2, Convert.ToInt32(halfPeriod));
			E4 = Formula.EMA(E3, Convert.ToInt32(halfPeriod));
			E5 = Formula.EMA(E4, Convert.ToInt32(halfPeriod));
			E6 = Formula.EMA(E5, Convert.ToInt32(halfPeriod));
			
		}

		public override void Update() {
			double b = Hot; 
	        double b2 = b*b; 
	        double b3 = b2*b; 
	        double c1 = -b3; 
	        double c2 = 3*b2 + 3*b3;
	        double c3 = -6*b2 - 3*b - 3*b3; 
	        double c4 = 1D + 3*b + b3 + 3*b2;

			if(Count == 1) this[0] = Input[0];
			else this[0] = c1*E6[0] + c2*E5[0] + c3*E4[0] + c4*E3[0];
		}
		
		public int Period {
			get { return period; }
			set { period = value; }
		}
		
		public double Hot {
			get { return hot; }
			set { hot = value; }
		}
	}
}
