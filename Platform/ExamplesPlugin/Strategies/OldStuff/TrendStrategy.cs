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
	public class TrendStrategy : Strategy
	{
		bool isActivated = false;
		DynamicSR sr;
		IndicatorCommon paint;
		
		public TrendStrategy()
		{
			ExitStrategy.ControlStrategy = false;
			RequestUpdate( Intervals.Second10);
			RequestUpdate( Intervals.Day1);
		}
		
		public override void OnInitialize()
		{
			
			#region DOM indicator
//			dom = new DOMRatio();
//			dom.PaneType = PaneType.Secondary;
//			dom.PaneType = PaneType.Hidden;
//			dom.BarPeriod = BarPeriod.Second10;
//			AddIndicator(dom);
			#endregion
			
			#region Averages
//			longAvg = new SMA(10);
//			longAvg.PaneType = PaneType.Primary;
//			longAvg.PeriodDefault = Range60;
//			longAvg.Color = Color.Green;
//			AddIndicator(longAvg);	
//			
//			mediumAvg = new SMA(5);
//			mediumAvg.PaneType = PaneType.Primary;
//			mediumAvg.PeriodDefault = Range60;
//			mediumAvg.Color = Color.Yellow;
//			AddIndicator(mediumAvg);	
//			
//			shortAvg = new TEMA(10);
//			shortAvg.PaneType = PaneType.Primary;
//			shortAvg.PeriodDefault = Range60;
//			shortAvg.Color = Color.Red;
//			AddIndicator(shortAvg);	
			#endregion
			
			sr = new DynamicSR();
			sr.LookbackPeriod = 2;
			AddIndicator( sr);
			
			paint = new IndicatorCommon();
			paint.Drawing.GraphType = GraphType.PaintBar;
			AddIndicator( paint);
		}
		
		public bool LongExitSignal {
			get { return Ticks[0].Bid < Formula.Lowest(sr.DynamicS,3,0); }
		}
		public bool ShortExitSignal {
			get { return Ticks[0].Ask > Formula.Highest(sr.DynamicR,3,0); }
		}
		public override bool OnProcessTick(Tick tick)
		{ 	
			if( Position.IsLong && LongExitSignal) {
				Orders.Exit.ActiveNow.GoFlat();
			}
			if( Position.IsShort && ShortExitSignal) {
				Orders.Exit.ActiveNow.GoFlat();
			}
			if( isActivated) {
//				if( TradeSignal.IsFlat && tick.Ask > Highest( Bars.High, 2)) {
//					Setup("long");
//				}
//				if( TradeSignal.IsFlat && tick.Bid < Lowest( Bars.Low, 2)) {
//					Setup("short");
//				}
//				if( !TradeSignal.IsShort && TradeSignal.IsSetup("short")) {
//					int digression = tick.Bid - SetupTick("short").Bid;
//					if( digression > shortDigression) {
//					 TradeSignal.GoShort();
//						Exits.StopLoss = 500;
//					}
//				}
//				if( !TradeSignal.IsLong && TradeSignal.IsSetup("long")) {
//					int digression = SetupTick("long").Ask - tick.Ask;
//					if( digression > longDigression) {
//					 TradeSignal.GoLong();
//						Exits.StopLoss = 500;
//					}
//				}
			}
			return true;
		}
		
		public override bool OnIntervalOpen(Interval timeFrame)
		{
			if( timeFrame.Equals(Intervals.Day1)) {
				stopTradingToday = false;
				isActivated = false;
			}
			return true;
		}
		
		public bool IsValidWeekDay
		{
			get { return !(Bars.Time[0].WeekDay == WeekDay.Saturday || Bars.Time[0].WeekDay == WeekDay.Sunday ); }
		}
		
		public override bool OnIntervalClose(Interval timeFrame)
		{
			if( timeFrame.Equals(Intervals.Second10 )) {
				
				string logString = "";
				isActivated = true;
				
				if( stopTradingToday ) {
					Orders.Exit.ActiveNow.GoFlat();
					logString = "Stop Trading Today";
					isActivated = false;
				} else {
					isActivated = true;
				}
				if( !logString.Equals(lastLogString) && logString.Length > 0) {
					Log.Notice( Ticks[0].Time + ":" + logString);
					lastLogString = logString;
				}
				
			}
			if( timeFrame.Equals(IntervalDefault)) {
				if( sr.Trend == Trend.Up) {
					paint[0] = 0;
				} else if( sr.Trend == Trend.Down) {
					paint[0] = 1;
				} else {
					paint[0] = 2;
				}
				if( isActivated && IsValidWeekDay) {
					if( sr.Trend == Trend.Up) {
						if( Position.IsFlat && !LongExitSignal) {
							Orders.Enter.ActiveNow.BuyMarket();
							Log.Notice(Ticks[0].Time + ", bar=" + Chart.DisplayBars.CurrentBar + ", Long");
						}
					} else if( sr.Trend == Trend.Down) {
						if( Position.IsFlat && !ShortExitSignal) {
							Orders.Enter.ActiveNow.SellMarket();
							Log.Notice(Ticks[0].Time + ", bar=" + Chart.DisplayBars.CurrentBar + ", Short");
						}
					}
				}
			}
			return true;
		}
		string lastLogString = "";
		
		bool stopTradingToday = false;
		public bool StopTradingToday { 
			get { return stopTradingToday; }
			set { stopTradingToday = value; }
		}
		
	}
	
}