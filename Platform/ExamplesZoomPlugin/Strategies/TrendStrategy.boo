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
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General License for more details.
 * 
 * You should have received a copy of the GNU General License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */

#endregion



namespace TickZoom

import System
import TickZoom.Api
import TickZoom.Common

class TrendStrategy(StrategyCommon):

	private isActivated = false

	private sr as DynamicSR

	private paint as IndicatorCommon

	
	def constructor():
		ExitStrategy.ControlStrategy = false
		Performance.Slippage = 0
		Performance.Commission = 1
		RequestUpdate(Intervals.Second10)
		RequestUpdate(Intervals.Day1)

	
	override def OnInitialize():
		
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
		
		sr = DynamicSR()
		sr.LookbackPeriod = 2
		AddIndicator(sr)
		
		paint = IndicatorCommon()
		paint.Drawing.GraphType = GraphType.PaintBar
		AddIndicator(paint)

	
	LongExitSignal as bool:
		get:
			return (Ticks[0].Bid < Formula.Lowest(sr.DynamicS, 3, 0))

	ShortExitSignal as bool:
		get:
			return (Ticks[0].Ask > Formula.Highest(sr.DynamicR, 3, 0))

	override def OnProcessTick(tick as Tick) as bool:
		if Position.IsLong and LongExitSignal:
			Exit.GoFlat()
		if Position.IsShort and ShortExitSignal:
			Exit.GoFlat()
		if isActivated:
			pass
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
		return true

	
	override def OnIntervalOpen(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Day1):
			stopTradingToday = false
			isActivated = false
		return true

	
	IsValidWeekDay as bool:
		get:
			return (not ((Bars.Time[0].WeekDay == WeekDay.Saturday) or (Bars.Time[0].WeekDay == WeekDay.Sunday)))

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Second10):
			
			logString = ''
			isActivated = true
			
			if stopTradingToday:
				Exit.GoFlat()
				logString = 'Stop Trading Today'
				isActivated = false
			else:
				isActivated = true
			if (not logString.Equals(lastLogString)) and (logString.Length > 0):
				Log.Notice(((Ticks[0].Time + ':') + logString))
				lastLogString = logString
			
		if timeFrame.Equals(IntervalDefault):
			if sr.Trend == Trend.Up:
				paint[0] = 0
			elif sr.Trend == Trend.Down:
				paint[0] = 1
			else:
				paint[0] = 2
			if isActivated and IsValidWeekDay:
				if sr.Trend == Trend.Up:
					if Position.IsFlat and (not LongExitSignal):
						Enter.BuyMarket()
						Log.Notice((((Ticks[0].Time + ', bar=') + Chart.DisplayBars.CurrentBar) + ', Long'))
				elif sr.Trend == Trend.Down:
					if Position.IsFlat and (not ShortExitSignal):
						Enter.SellMarket()
						Log.Notice((((Ticks[0].Time + ', bar=') + Chart.DisplayBars.CurrentBar) + ', Short'))
		return true

	private lastLogString = ''

	
	private stopTradingToday = false

	StopTradingToday as bool:
		get:
			return stopTradingToday
		set:
			stopTradingToday = value
	


