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
using NUnit.Framework;
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.TickUtil;

#if TESTING
namespace TickZoom.TradingFramework
{
	public class StrategySupportMock : Strategy {
		public BarsList openMinuteBars = new BarsList();
		public BarsList openMinute30Bars = new BarsList();
		public BarsList openHourBars = new BarsList();
		public BarsList openDayBars = new BarsList();
		public BarsList openSessionBars = new BarsList();
		public BarsList openWeekBars = new BarsList();
		public BarsList openMonthBars = new BarsList();
		public BarsList openYearBars = new BarsList();
		
		public BarsList closeMinuteBars = new BarsList();
		public BarsList closeMinute30Bars = new BarsList();
		public BarsList closeHourBars = new BarsList();
		public BarsList closeDayBars = new BarsList();
		public BarsList closeSessionBars = new BarsList();
		public BarsList closeWeekBars = new BarsList();
		public BarsList closeMonthBars = new BarsList();
		public BarsList closeYearBars = new BarsList();
		
		public override void OnInitialize()
		{
			RequestUpdate(Intervals.Minute1);
			RequestUpdate(Intervals.Minute30);
			RequestUpdate(Intervals.Hour1);
			RequestUpdate(Intervals.Day1);
			RequestUpdate(Intervals.Week1);
			RequestUpdate(Intervals.Session1);
			RequestUpdate(Intervals.Month1);
			RequestUpdate(Intervals.Year1);
		}
	
		public override bool OnIntervalOpen(Interval interval) {
			switch( interval.BarUnit) {
				case BarUnit.Minute:
					if( interval.Period == 1) {
						openMinuteBars.Add(NewBar(Minutes));
					}
					if( interval.Period == 30) {
						openMinute30Bars.Add(NewBar(Data.Get(Intervals.Minute30)));
					}
					break;
				case BarUnit.Hour:
					openHourBars.Add(NewBar(Hours));
					break;
				case BarUnit.Day:
					openDayBars.Add(NewBar(Days));
					break;
				case BarUnit.Session:
					openSessionBars.Add(NewBar(Sessions));
					break;
				case BarUnit.Week:
					openWeekBars.Add(NewBar(Weeks));
					break;
				case BarUnit.Month:
					openMonthBars.Add(NewBar(Months));
					break;
				case BarUnit.Year:
					openYearBars.Add(NewBar(Years));
					break;
			}
			return true;
		}
		
		public Bar NewBar( Bars bars) {
			return new Bar( bars.Time[0],bars.EndTime[0],bars.Open[0],bars.High[0],bars.Low[0],bars.Close[0],Ticks[0].Time);
		}
		
		public override bool OnIntervalClose(Interval interval) {
			switch( interval.BarUnit) {
				case BarUnit.Minute:
					if( interval.Period == 1) {
						closeMinuteBars.Add(NewBar(Minutes));
					}
					if( interval.Period == 30) {
						closeMinute30Bars.Add(NewBar(Data.Get(Intervals.Minute30)));
					}
					break;
				case BarUnit.Hour:
					closeHourBars.Add(NewBar(Hours));
					break;
				case BarUnit.Day:
					closeDayBars.Add(NewBar(Days));
					break;
				case BarUnit.Session:
					closeSessionBars.Add(NewBar(Sessions));
					break;
				case BarUnit.Week:
					closeWeekBars.Add(NewBar(Weeks));
					break;
				case BarUnit.Month:
					closeMonthBars.Add(NewBar(Months));
					break;
				case BarUnit.Year:
					closeYearBars.Add(NewBar(Years));
					break;
			}
			return true;
		}
		
	}
}
#endif
