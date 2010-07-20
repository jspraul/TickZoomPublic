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
	/// Double Exponential Moving Average (DEMA). Introduced by Patrick Mulloy
	/// in the February 1994 issue of Technical Analysis of Stocks & Commodities magazine.
	/// DEMA was created to induce less lag than a regular EMA. It is a composite of 
	/// a single exponential MA and a double exponential MA that generates less lag than EMA.
	/// FB 20091230: Created
	/// </summary>
	public class DEMA : IndicatorCommon
	{
		EMA E1;
		EMA E2;
		int period = 14;
		
		public DEMA( object anyPrice, int period) {
			AnyInput = anyPrice;
			StartValue = 0;
			this.period = period;
		}

		public override void OnInitialize() {
			Name = "DEMA";
			Drawing.Color = Color.Blue;
			Drawing.PaneType = PaneType.Primary;
			Drawing.IsVisible = true;
			
			E1 = new EMA(Input, Period);
			E2 = new EMA(E1, Period);
		}
		
		public override void Update() {
			if( Count == 1) this[0] = Input[0];
			else this[0] = (2 * E1[0] - E2[0]);
		}

		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
