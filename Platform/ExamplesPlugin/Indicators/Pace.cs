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
using TickZoom.Api;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of EMA.
	/// </summary>
	public class Pace : IndicatorCommon
	{
		int volume = 0;
		
		public Pace() : base()
		{
//			IntervalDefault = Interval.Range10;
			Drawing.GraphType = GraphType.Histogram;
		}
		
		public override void OnInitialize() {
			Drawing.PaneType = PaneType.Secondary;
			Drawing.GroupName = "Pace";
		}
		
		public override bool OnIntervalOpen()
		{
			if( Bars.Volume.Count>1) {
				volume = Bars.Volume[1];
			}
			return true;
		}

		public override bool OnIntervalClose()
		{
			if( this.Count == 0) { return true; }
			Elapsed ts = Bars.EndTime[0] - Bars.Time[0];
			this[0] = Math.Log10(ts.TotalSeconds);
			return true;
		}
	}
}
