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

class SimpleStrategy(StrategyCommon):

	private isActivated = false

	private average as TEMA

	private pace as IndicatorCommon

	private equity as IndicatorCommon

	private digression = 50

	private mean = 0

	
	def constructor():
		ExitStrategy.ControlStrategy = false
		Performance.Slippage = 0
		Performance.Commission = 1
		IntervalDefault = Intervals.Range30
		RequestUpdate(Intervals.Range5)
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
		
		//			avgPace = new SMA(pace,5);
		//			avgPace.PaneType = PaneType.Secondary;
		//			avgPace.GroupName = "Pace";
		//			AddIndicator(avgPace);
		
		pace = IndicatorCommon()
		pace.Drawing.GraphType = GraphType.Histogram
		pace.Drawing.GroupName = 'Pace'
		AddIndicator(pace)
		
		equity = IndicatorCommon()
		equity.Drawing.PaneType = PaneType.Secondary
		equity.Drawing.GroupName = 'Equity'
		AddIndicator(equity)
		
		//			Exits.DailyMaxProfit = 320;
		ExitStrategy.StopLoss = 300
		ExitStrategy.TargetProfit = 200

	
	override def OnProcessTick(tick as Tick) as bool:
		ts as Elapsed = (tick.Time - Range5.Time[0])
		pace[0] = Math.Log(ts.TotalSeconds)
		equity[0] = Performance.Equity.CurrentEquity
		if isActivated and (average.Count > 1):
			
			mean = cast(int, average[0])
			
			if (not Position.IsShort) and (tick.Bid >= (mean + digression)):
				Enter.SellMarket()
			if (not Position.IsLong) and (tick.Ask <= (mean - digression)):
				Enter.BuyMarket()
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
	


