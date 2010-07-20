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

class SimpleStrategy3(StrategyCommon):

	private isActivated = false

	private average as TEMA

	private mediumAvg as SMA

	private slowAvg as SMA

	
	def constructor():
		ExitStrategy.ControlStrategy = false
		Performance.Slippage = 0
		Performance.Commission = 1
		IntervalDefault = Intervals.Range30
		RequestUpdate(Intervals.Second10)
		RequestUpdate(Intervals.Day1)

	
	override def OnInitialize():
		#region DOM
		//			dom = new DOMRatio();
		//			dom.PaneType = PaneType.Secondary;
		//			dom.PaneType = PaneType.Hidden;
		//			dom.BarPeriod = BarPeriod.Second10;
		//			AddIndicator(dom);
		#endregion
		
		average = TEMA(Bars.Close, 5)
		AddIndicator(average)
		
		mediumAvg = SMA(Bars.Close, 10)
		AddIndicator(mediumAvg)
		
		slowAvg = SMA(Bars.Close, 50)
		AddIndicator(slowAvg)
		
		ExitStrategy.StopLoss = 300
		ExitStrategy.DailyMaxLoss = 600
		ExitStrategy.WeeklyMaxLoss = 1200
		ExitStrategy.MonthlyMaxLoss = 1800
		
		ExitStrategy.TargetProfit = 650
		ExitStrategy.DailyMaxProfit = 600

	
	//		int digression = 200;
	private mean = 0

	override def OnProcessTick(tick as Tick) as bool:
		if isActivated and (average.Count > 1):
			
			trend as Trend = Trend.None
			if (average[0] > mediumAvg[0]) and (mediumAvg[0] > slowAvg[0]):
				trend = Trend.Up
			elif (average[0] < mediumAvg[0]) and (mediumAvg[0] < slowAvg[0]):
				trend = Trend.Down
			
			mean = cast(int, average[0])
			converterGeneratedName1 = trend
			if converterGeneratedName1 == Trend.Up:
				if (not Position.IsLong) and (tick.Bid < mean):
					Enter.BuyMarket()
			elif converterGeneratedName1 == Trend.Down:
				if (not Position.IsShort) and (tick.Ask > mean):
					Enter.SellMarket()
			if Position.IsShort and (mean > mediumAvg[0]):
				Exit.GoFlat()
			if Position.IsLong and (mean < mediumAvg[0]):
				Exit.GoFlat()
		return true

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		logString = ''
		
		if timeFrame.Equals(Intervals.Second10):
			
			if Formula.IsForexWeek:
				isActivated = true
			else:
				isActivated = false
			if (not logString.Equals(lastLogString)) and (logString.Length > 0):
				Log.Notice(((Ticks[0].Time + ':') + logString))
				lastLogString = logString
			
		return true

	private lastLogString = ''
	


