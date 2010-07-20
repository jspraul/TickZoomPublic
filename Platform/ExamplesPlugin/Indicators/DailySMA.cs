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
	public class DailySMA : IndicatorCommon
	{
		int period = 5;
		int displace = 0;
		Doubles price;
		Prices externalPrice;
		
		IndicatorCommon top;
		IndicatorCommon bottom;
		IndicatorCommon center;		
		
		public DailySMA()
		{
			this.price = Doubles();
		}
		
		public DailySMA(int period, int displace)
		{
			this.price = Doubles();
			this.period = period;
			this.displace = displace;

		}
		
		public DailySMA(Prices price, int period, int displace) : this(period,displace)
		{
			this.externalPrice = price;
		}
		
		public override void OnInitialize() {
			if( externalPrice == null) { 
				externalPrice = Bars.Close;
			}
			PaneType original = Drawing.PaneType;
			top = new IndicatorCommon();
			top.Drawing.PaneType = original;
			AddIndicator(top);
			bottom = new IndicatorCommon();
			bottom.Drawing.PaneType = original;
			AddIndicator(bottom);
			center = new IndicatorCommon();
			center.Drawing.PaneType = original;
			AddIndicator(center);
			Drawing.IsVisible = false;
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			if( Chart.IsDynamicUpdate) {
				UpdateAverage();
			}
			return true;
		}
		
		public double Width {
			get { return top[0] - bottom[0]; }
		}

		public bool IsUpper(Tick tick) {
			return tick.Ask < top[0] && tick.Bid > top[0] - Width/3;
		}
		
		public bool IsLower(Tick tick) {
			return tick.Ask < bottom[0]+Width/3 && tick.Bid > bottom[0];
		}
		
		int countBars;
		public void Reset() {
			if( Count > 1) {
				double level = price[0];
				for( int i=1; i<price.Count && i<period; i++) {
					price[i] = (int) level;
				}
				this[1] = this[0] = level;
			}
		}
		
		
		public override bool OnIntervalClose()
		{
			price.Add( externalPrice[0]);
			UpdateAverage();
			double y = top[5];
			if( Count > 2) {
				top[0] = this[displace] + IntervalDefault.Period*2;
				bottom[0] = this[displace] - IntervalDefault.Period*2;
				center[0] = this[displace];
			}
			return true;
		}

		
		public void UpdateAverage() {
			if (Count == 1) {
				this[0] = price[0];
				countBars=1;
			} else {
				countBars++;
				
				double last = this[1];
				double sum = last * Math.Min(countBars-1, period);

				if (countBars > period && price.BarCount > period) {
					double b = price[0];
					double p = price[period];
					int d = Math.Min(countBars, period);
					double x = (sum + price[0] - price[period]) / Math.Min(countBars, period);
					this[0] = x;
				} else if( price.BarCount > 0 ) {
					this[0] = (sum + price[0]) / (Math.Min(countBars, period));
				}
			}
		}
		
		public int Period {
			get { return period; }
			set { period = value; }
		}
		
		public IndicatorCommon Top {
			get { return top; }
		}
		
		public IndicatorCommon Bottom {
			get { return bottom; }
		}
		
		public IndicatorCommon Center {
			get { return center; }
		}
	}
}
