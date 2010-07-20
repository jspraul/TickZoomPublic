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
	public class FractalStrategy : Strategy
	{
		bool isActivated = false;
		TEMA average;
		IndicatorCommon pace;
		IndicatorCommon equity;
		int digression = 100;
		int mean = 0;
		PivotHighVs pivotHighs;
		PivotLowVs pivotLows;
		
		
		public FractalStrategy()
		{
			ExitStrategy.ControlStrategy = false;
			IntervalDefault = Intervals.Range10;
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
			
			pivotHighs = new PivotHighVs(2,2);
			pivotHighs.Drawing.IsVisible = true;
			AddIndicator(pivotHighs);
			
			pivotLows = new PivotLowVs(2,2);
			pivotLows.Drawing.IsVisible = true;
			AddIndicator(pivotLows);
			
			average = new TEMA(Bars.Close,5);
//			average.Drawing.IsVisible = true;
			average.Drawing.GroupName = "Average";
			AddIndicator(average);	
			
			pace = new IndicatorCommon();
			pace.Drawing.GraphType = GraphType.Histogram;
//			pace.Drawing.IsVisible = true;
			pace.Drawing.GroupName = "Pace";
			AddIndicator(pace);
			
			equity = new IndicatorCommon();
//			equity.Drawing.IsVisible = true;
			equity.Drawing.GroupName = "Equity";
			equity.Drawing.PaneType = PaneType.Secondary;
			equity.Drawing.GraphType = GraphType.FilledLine;
			equity.Drawing.Color = Color.Green;
			AddIndicator(equity);
			
			ExitStrategy.DailyMaxProfit = 320;
//			Exits.DailyMaxLoss = 320;
 		    ExitStrategy.StopLoss = 300;
		}

		public override bool OnProcessTick(Tick tick)
		{ 	
			Elapsed ts = tick.Time - Bars.Time[0];
			pace[0] = ts.TotalSeconds;
			equity[0] = Performance.Equity.CurrentEquity;
			
			if( isActivated && average.Count>1) {
				
				mean = (int) average[0];

				if( !Position.IsShort && tick.Bid >= mean + digression) {
//				 Signal.GoShort();
				}
				if( !Position.IsLong && tick.Ask <= mean - digression) {
//				 Signal.GoLong();
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