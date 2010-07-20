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
	/// Average Range
	/// FB 20100102: Cleaned up
	/// </summary>
	public class AvgRange : IndicatorCommon
	{
		int period = 13;
		double totalRange = 0;
		int totalCount = 0;
		long totalDiffSquares = 0;
		
		public AvgRange(int period)
		{
			Name = "AvgRange";
			Drawing.Color = Color.Green;
			Drawing.PaneType = PaneType.Secondary;
			Drawing.IsVisible = true;
			Drawing.GroupName = "AvgRange";
			Drawing.GraphType = GraphType.Line;
			
			this.period = period;
		}
		
		double range(int pos) {
			return Bars.High[pos] - Bars.Low[pos];
		}
		
		public override void Update() {
			totalRange += range(0);
			totalCount ++;
			totalDiffSquares += (long) Math.Pow(HistoricalAverage - range(0),2);
			if (Count == 1) {
				this[0] = range(0);
			} else {
				double last = this[1] * Math.Min(Count-1, period);

				if (BarCount > period && Bars.BarCount > period) {
					double x = (last + range(0) - range(period)) / Math.Min(Count, period);
					this[0] = x;
				} else if( Bars.Count > 0 ) {
					this[0] = (last + range(0)) / (Math.Min(Count, period));
				}
			}
		}
		
		public int Period {
			get { return period; }
			set { period = value; }
		}
		
		public int HistoricalAverage {
			get { return (int) (totalRange / totalCount); }
		}
		
		public int HistoricalStdDev {
			get { 
				if( totalCount > 0) {
					return (int) Math.Sqrt(totalDiffSquares / totalCount);
				} else {
					return 0;
				}
			}
		}
	}
}
