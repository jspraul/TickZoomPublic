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
using System.IO;
using TickZoom.Common;
using TickZoom.Api;

namespace TickZoom
{
	/// <summary>
	/// Description of SMA.
	/// </summary>
	public class DetrendOSC : IndicatorCommon
	{
		int period = 5;
		Prices price = null;
		SMA sma;
		
		public DetrendOSC()
		{
		}
		
		public DetrendOSC(int period)
		{
			this.period = period;
		}
		
		public DetrendOSC(Prices price, int period)
		{
			this.price = price;
			this.period = period;
		}
		
		public override void OnInitialize() {
			if( price == null) { 
				price = Bars.Close;
			}
			sma = new SMA(price, period);
			sma.IntervalDefault = IntervalDefault;
			sma.Drawing.IsVisible = false;
			AddIndicator(sma);
		}
		
		public override bool OnIntervalClose() {
			if (Count < period/2+1) {
				this[0] = 0;
			} else {
				this[0] = price[0] - sma[period/2+1];
			}
			return true;
		}
		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
