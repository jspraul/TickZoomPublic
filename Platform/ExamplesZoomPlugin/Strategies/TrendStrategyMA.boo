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

class TrendStrategyMA(StrategyCommon):

	//		DOMRatio dom;
	private isActivated = false

	private average as TEMA

	private slowAvg as SMA

	
	def constructor():
		ExitStrategy.ControlStrategy = false
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
		
		slowAvg = SMA(Bars.Close, 10)
		AddIndicator(slowAvg)

	
	override def OnProcessTick(tick as Tick) as bool:
		if isActivated and (average.Count > 1):
			//				Trend trend = Trend.None;
			if average[0] > slowAvg[0]:
				//					trend = Trend.Up;	
				if not Position.IsLong:
					Enter.BuyMarket()
					ExitStrategy.StopLoss = 300
					ExitStrategy.DailyMaxLoss = 600
					ExitStrategy.WeeklyMaxLoss = 1200
					ExitStrategy.MonthlyMaxLoss = 1800
			elif average[0] < slowAvg[0]:
				//					trend = Trend.Down;
				if not Position.IsShort:
					Enter.SellMarket()
					ExitStrategy.StopLoss = 300
					ExitStrategy.DailyMaxLoss = 600
					ExitStrategy.WeeklyMaxLoss = 1000
					ExitStrategy.MonthlyMaxLoss = 1800
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
	


