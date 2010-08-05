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

namespace TickZoom.Common
{
	/// <summary>
	/// Description of Intervals.
	/// </summary>
	public class Intervals
	{
		public readonly static Interval Default = Factory.Engine.DefineInterval(BarUnit.Default,0);
		public readonly static Interval Tick1 = Factory.Engine.DefineInterval(BarUnit.Tick,1);
		public readonly static Interval Tick10 = Factory.Engine.DefineInterval(BarUnit.Tick,10);
		public readonly static Interval Tick20 = Factory.Engine.DefineInterval(BarUnit.Tick,20);
		public readonly static Interval Tick50 = Factory.Engine.DefineInterval(BarUnit.Tick,50);
		public readonly static Interval Tick100 = Factory.Engine.DefineInterval(BarUnit.Tick,100);
		public readonly static Interval Tick200 = Factory.Engine.DefineInterval(BarUnit.Tick,200);
		public readonly static Interval Week1 = Factory.Engine.DefineInterval(BarUnit.Week,1);
		public readonly static Interval Session1 = Factory.Engine.DefineInterval(BarUnit.Session,1);
		public readonly static Interval Year1 = Factory.Engine.DefineInterval(BarUnit.Year,1);
		public readonly static Interval Month1 = Factory.Engine.DefineInterval(BarUnit.Month,1);
		public readonly static Interval Quarter1 = Factory.Engine.DefineInterval(BarUnit.Month,3);
		public readonly static Interval Second1 = Factory.Engine.DefineInterval(BarUnit.Second,1);
		public readonly static Interval Second10 = Factory.Engine.DefineInterval(BarUnit.Second,10);
		public readonly static Interval Second30 = Factory.Engine.DefineInterval(BarUnit.Second,30);
		public readonly static Interval Second60 = Factory.Engine.DefineInterval(BarUnit.Second,60);
		public readonly static Interval Second75 = Factory.Engine.DefineInterval(BarUnit.Second,75);
		public readonly static Interval Second150 = Factory.Engine.DefineInterval(BarUnit.Second,150);
		public readonly static Interval Minute1 = Factory.Engine.DefineInterval(BarUnit.Minute,1);
		public readonly static Interval Minute2 = Factory.Engine.DefineInterval(BarUnit.Minute,2);
		public readonly static Interval Minute5 = Factory.Engine.DefineInterval(BarUnit.Minute,5);
		public readonly static Interval Minute10 = Factory.Engine.DefineInterval(BarUnit.Minute,10);
		public readonly static Interval Minute30 = Factory.Engine.DefineInterval(BarUnit.Minute,30);
		public readonly static Interval Day1 = Factory.Engine.DefineInterval(BarUnit.Day,1);
		public readonly static Interval Hour1 = Factory.Engine.DefineInterval(BarUnit.Hour,1);
		public readonly static Interval Hour4 = Factory.Engine.DefineInterval(BarUnit.Hour,4);
		public readonly static Interval Range40 = Factory.Engine.DefineInterval(BarUnit.Range,40);
		public readonly static Interval Range30 = Factory.Engine.DefineInterval(BarUnit.Range,30);
		public readonly static Interval Range20 = Factory.Engine.DefineInterval(BarUnit.Range,20);
		public readonly static Interval Range10 = Factory.Engine.DefineInterval(BarUnit.Range,10);
		public readonly static Interval Range5 = Factory.Engine.DefineInterval(BarUnit.Range,5);
		public static Interval Custom(BarLogic logic) {
			var interval = Factory.Engine.DefineInterval(BarUnit.Custom,1);
			interval.Set("Instance",logic);
			return interval;
		}
		public static Interval Define(BarUnit unit, int period) { return Factory.Engine.DefineInterval(unit,period); }
		public static Interval Define(BarUnit unit, int period, BarUnit unit2, int period2) { return Factory.Engine.DefineInterval(unit,period,unit2,period2); }	}
}
