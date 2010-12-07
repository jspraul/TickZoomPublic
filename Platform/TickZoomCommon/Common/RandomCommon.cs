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
using System.Diagnostics;
using TickZoom.Api;


namespace TickZoom.Common
{
	/// <summary>
	/// Description of RandomStrategy.
	/// </summary>
	public class RandomCommon : Strategy
	{
		TimeStamp[] randomEntries = null;
		int randomIndex = 0;
		Random random = new Random(393957857);
		int sessionHours = 4;
		bool firstSession = false;
		Elapsed sessionStart = new Elapsed(8,0,0);
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(RandomCommon));
		private readonly bool debug = log.IsDebugEnabled;
		private readonly bool trace = log.IsTraceEnabled;		
		
		public RandomCommon()
		{
			if( trace) log.Trace("new");
			randomEntries = new TimeStamp[20];
			if( trace) log.Trace(Chain.ToString());
			RequestEvent(EventType.Tick);
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			TimeStamp tickTime = Ticks[0].Time;
			if( tickTime.TimeOfDay >= sessionStart) {
				if( randomIndex < randomEntries.Length &&
				    tickTime > randomEntries[randomIndex] &&
				    firstSession == true)
				{
					int sig = randomEntries[randomIndex].Second % 3 - 1;
					if( sig != Position.Current) {
						if( sig == 0) {
							Orders.Exit.ActiveNow.GoFlat();
						} else {
							if( Position.IsFlat) {
								if( sig > 0) {
									Orders.Enter.ActiveNow.BuyMarket(sig);
								} else if( sig < 0) {
									Orders.Enter.ActiveNow.SellMarket(Math.Abs(sig));
								}
							} else {
								if( sig > 0) {
									if( !Position.IsLong) {
										Orders.Reverse.ActiveNow.BuyMarket(sig);
									}
								} else if( sig < 0) {
									if( !Position.IsShort) {
										Orders.Reverse.ActiveNow.SellMarket(Math.Abs(sig));
									}
								}
							}
						}
					}
					randomIndex ++;
				}
			}
			return true;
		}

		public void CalculateTimes() {
			TimeStamp time = Ticks[0].Time;
			TimeStamp startTime = new TimeStamp(time.Year,time.Month,time.Day);
			startTime.Add(sessionStart);
			for( int i =0; i < randomEntries.Length; i++) {
				randomEntries[i] = startTime;
				randomEntries[i].AddSeconds(random.Next(0,sessionHours*60*60));
			}
			Array.Sort(randomEntries,0,randomEntries.Length);
			randomIndex = 0;
			firstSession = true;
		}
		
		public override bool OnIntervalOpen(Interval timeFrame)
		{
			switch( timeFrame.BarUnit) {
			case BarUnit.Day:
				CalculateTimes();
				break;
			}
			return true;
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			switch( timeFrame.BarUnit) {
			case BarUnit.Session:
				Orders.Exit.ActiveNow.GoFlat();
				break;
			}
			return true;
		}
	}
}
