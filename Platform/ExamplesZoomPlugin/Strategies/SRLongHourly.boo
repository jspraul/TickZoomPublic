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
import System.Drawing
import TickZoom.Api
import TickZoom.Common

class SRLongHourly(StrategyCommon):

	private retrace as IndicatorCommon
	private reboundPercent as IndicatorCommon
	private maximumStretch = 1700
	private stretchFloor = 1000
	private stretchDivisor = 95

	def constructor():
		ExitStrategy.ControlStrategy = false
		IntervalDefault = Intervals.Minute10

	
	override def OnInitialize():
		super.OnInitialize()
		retrace = IndicatorCommon()
		retrace.Drawing.Color = Color.Red
		retrace.IntervalDefault = Intervals.Hour1
		AddIndicator(retrace)
		
		reboundPercent = IndicatorCommon()
		reboundPercent.Drawing.Color = Color.Red
		reboundPercent.IntervalDefault = Intervals.Hour1
		reboundPercent.Drawing.PaneType = PaneType.Secondary
		AddIndicator(reboundPercent)

	
	private lowest as double

	private highest as double

	private maxSize = 0

	override def OnIntervalOpen(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Month1):
			highest = Hours.High[0]
			lowest = Hours.Low[0]
			retrace[0] = ((highest + lowest) / 2)
		return true

	def reset():
		maxSize = 0
		highest = Hours.High[0]
		lowest = Hours.Low[0]
		retrace[0] = ((highest + lowest) / 2)

	override def OnIntervalClose(timeFrame as Interval) as bool:
		super.OnIntervalClose(timeFrame)
		if Days.Count < 3:
			return true
		
		if timeFrame.Equals(Intervals.Hour1):
			
			if double.IsNaN(retrace[0]):
				middle = (Hours.Low[0] + Hours.High[0]) / 2
				lowest = highest = middle;
				retrace[0] = middle;
			else:
				
				if Hours.High[0] > highest:
					retrace[0] += (Math.Max((Hours.High[0] - highest), 0) / 2)
					highest = Hours.High[0]
				if Hours.Low[0] < lowest:
					retrace[0] -= (Math.Max((lowest - Hours.Low[0]), 0) / 2)
					lowest = Hours.Low[0]
				if Formula.CrossesUnder(Hours.Low, retrace[0]) or Formula.CrossesOver(Hours.High, retrace[0]):
					highest = Hours.High[0]
					lowest = Hours.Low[0]
		
		if timeFrame.Equals(Intervals.Minute1):
			stretch as int = (cast(int, (highest - lowest)) / 2)
			rebound = cast(int, Math.Max(0, (stretch - Math.Abs((Bars.Close[0] - retrace[0])))))
			reboundPercent[0] = Math.Min(100, ((rebound * 100) / (highest - lowest)))
			positionsize as int = Math.Max(0, ((stretch / 100) - 2))
			reboundThreshold as int = Math.Max(0, (45 - ((Math.Max(0, (stretch - stretchFloor)) / stretchDivisor) * 5)))
			
			if stretch > maximumStretch:
				Exit.GoFlat()
				reset()
				return true
			
			if Minutes.Close[0] < retrace[0]:
				if positionsize > maxSize:
					Enter.BuyMarket((Position.Size + 1))
					maxSize = positionsize
			if Position.IsLong and (reboundPercent[0] > reboundThreshold):
				Exit.GoFlat()
				reset()
				return true
			
			if Minutes.Close[0] > retrace[0]:
				if positionsize > maxSize:
					Enter.SellMarket((Position.Size + 1))
					maxSize = positionsize
			if Position.IsShort and (reboundPercent[0] > reboundThreshold):
				Exit.GoFlat()
				reset()
				return true
			
			if Position.IsFlat:
				maxSize = 0
		return true

	
	MaximumStretch as int:
		get:
			return maximumStretch
		set:
			maximumStretch = value

	
	StretchFloor as int:
		get:
			return stretchFloor
		set:
			stretchFloor = value

	
	StretchDivisor as int:
		get:
			return stretchDivisor
		set:
			stretchDivisor = value

