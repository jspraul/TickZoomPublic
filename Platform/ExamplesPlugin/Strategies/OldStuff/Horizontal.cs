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
	public class Horizontal : Strategy
	{
		IndicatorCommon equity;
		
		
		public Horizontal()
		{
			ExitStrategy.ControlStrategy = false;
			IntervalDefault = Intervals.Range10;
			RequestUpdate(Intervals.Range5);
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
			equity = new IndicatorCommon();
			equity.Drawing.PaneType = PaneType.Secondary;
			equity.Drawing.GroupName = "Equity";
			AddIndicator(equity);
			
//			Exits.DailyMaxProfit = 320;
// 		    Exits.StopLoss = 300;
// 		    Exits.TargetProfit = 200;
		}

		public override bool OnProcessTick(Tick tick)
		{ 	
			Elapsed ts = tick.Time - Range5.Time[0];
			equity[0] = Performance.Equity.CurrentEquity;
			return true;
		}
		
		public override bool OnIntervalClose()
		{
			if( !Position.IsLong && Bars.Close[0] > Days.Open[0]+200 &&
				Bars.Close[1] < Days.Open[0]+200 ) {
				Orders.Enter.ActiveNow.SellMarket();
			}
			if( !Position.IsShort && Bars.Close[0] < Days.Open[0]+200 &&
			   Bars.Close[1] > Days.Open[0]+200 ) {
				Orders.Enter.ActiveNow.BuyMarket();
			}
			return true;
		}
		
		public override bool OnIntervalClose(Interval timeFrame)
		{
			string logString = "";

			if( timeFrame.Equals(Intervals.Second10)) {
				
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