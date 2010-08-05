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
using System.Drawing;
using System.IO;

using TickZoom.Api;

using TickZoom.Common;

namespace TickZoom
{
	public class SimpleStrategy3 : Strategy
	{
		bool isActivated = false;
		TEMA average;
		SMA mediumAvg;
		SMA slowAvg;
		
		public SimpleStrategy3()
		{
			ExitStrategy.ControlStrategy = false;
			IntervalDefault = Intervals.Range30;
			RequestUpdate(Intervals.Second10);
			RequestUpdate(Intervals.Day1);
		}
		
		public override void OnInitialize()
		{
			#region DOM
//			dom = new DOMRatio();
//			dom.PaneType = PaneType.Secondary;
//			dom.PaneType = PaneType.Hidden;
//			dom.BarPeriod = BarPeriod.Second10;
//			AddIndicator(dom);
			#endregion
			
			average = new TEMA(Bars.Close,5);
			AddIndicator(average);	
			
			mediumAvg = new SMA(Bars.Close,10);
			AddIndicator(mediumAvg);	
			
			slowAvg = new SMA(Bars.Close,50);
			AddIndicator(slowAvg);	
			
			ExitStrategy.StopLoss = 300;
			ExitStrategy.DailyMaxLoss = 600;
			ExitStrategy.WeeklyMaxLoss = 1200;
			ExitStrategy.MonthlyMaxLoss = 1800;
			
			ExitStrategy.TargetProfit = 650;
			ExitStrategy.DailyMaxProfit = 600;
		}

//		int digression = 200;
		int mean = 0;
		public override bool OnProcessTick(Tick tick)
		{ 	
			if( isActivated && average.Count>1) {
				
				Trend trend = Trend.None;
				if( average[0] > mediumAvg[0] && mediumAvg[0] > slowAvg[0]) {
					trend = Trend.Up;	
				} else if ( average[0] < mediumAvg[0] && mediumAvg[0] < slowAvg[0]) {
					trend = Trend.Down;
				}
				
				mean = (int) average[0];
				switch( trend) {
					case Trend.Up:
						if( !Position.IsLong && tick.Bid < mean) {
							Orders.Enter.ActiveNow.BuyMarket();
						}
						break;
					case Trend.Down:
						if( !Position.IsShort && tick.Ask > mean) {
							Orders.Enter.ActiveNow.SellMarket();
						}
						break;
				}
				if( Position.IsShort && mean > mediumAvg[0]) {
				    Orders.Exit.ActiveNow.GoFlat();
				}
				if( Position.IsLong && mean < mediumAvg[0]) {
				    Orders.Exit.ActiveNow.GoFlat();
				}
			}
			return true;
		}
		
		public override bool OnIntervalClose(Interval timeFrame)
		{
			string logString = "";

			if( timeFrame.Equals(Intervals.Second10)) {
				
				if( Formula.IsForexWeek ) {
					isActivated = true;
				} else {
					isActivated = false;
				}
				if( !logString.Equals(lastLogString) && logString.Length > 0) {
					Log.Notice( Ticks[0].Time + ":" + logString);
					lastLogString = logString;
				}
				
			}
			return true;
		}
		string lastLogString = "";
		
	}
	
}