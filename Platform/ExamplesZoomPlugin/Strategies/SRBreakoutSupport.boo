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

class SRBreakoutSupport(StrategyCommon):

	private minimumMove = 370

	private avgRange as AvgRange

	private resistance as SRHigh

	private support as SRLow

	private entryLevel as IndicatorCommon

	private filterPercent = 10

	private middle as IndicatorCommon

	
	override def OnInitialize():
		super.OnInitialize()
		resistance = SRHigh(minimumMove)
		resistance.IntervalDefault = IntervalDefault
		AddIndicator(resistance)
		
		support = SRLow(minimumMove)
		support.IntervalDefault = IntervalDefault
		AddIndicator(support)
		
		avgRange = AvgRange(14)
		avgRange.IntervalDefault = Intervals.Day1
		AddIndicator(avgRange)
		
		entryLevel = IndicatorCommon()
		entryLevel.IntervalDefault = IntervalDefault
		AddIndicator(entryLevel)
		
		middle = IndicatorCommon()
		middle.IntervalDefault = Intervals.Day1
		middle.Drawing.Color = Color.Orange
		AddIndicator(middle)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Day1):
			updateMinimumMove()
		if timeFrame.Equals(Intervals.Hour1):
			middle[0] = ((Resistance[0] + Support[0]) / 2)
		return true

	
	IsNewResistance as bool:
		get:
			return (resistance[0] != resistance[1])

	
	IsNewSupport as bool:
		get:
			return (resistance[0] != resistance[1])

	
	private def updateMinimumMove():
		support.MinimumMove = cast(int, ((avgRange[0] * minimumMove) / 100))
		resistance.MinimumMove = cast(int, ((avgRange[0] * minimumMove) / 100))

	
	protected virtual LongTrend as bool:
		get:
			return ((resistance.Pivots[0] > (resistance.Pivots[1] + avgRange.HistoricalStdDev)) and (support.Pivots[0] > (support.Pivots[1] + avgRange.HistoricalStdDev)))

	
	protected virtual ShortTrend as bool:
		get:
			return ((resistance.Pivots[0] < (resistance.Pivots[1] - avgRange.HistoricalStdDev)) and (support.Pivots[0] < (support.Pivots[1] - avgRange.HistoricalStdDev)))

	
	MinimumMove as int:
		get:
			return minimumMove
		set:
			minimumMove = value

	
	protected LowVolatility as bool:
		get:
			return (avgRange[0] < avgRange.HistoricalAverage)

	
	protected HighVolatility as bool:
		get:
			return (avgRange[0] > (avgRange.HistoricalAverage + avgRange.HistoricalStdDev))

	AvgRange as AvgRange:
		get:
			return avgRange

	Resistance as SRHigh:
		get:
			return resistance

	
	Support as SRLow:
		get:
			return support

	
	EntryLevel as IndicatorCommon:
		get:
			return entryLevel

	
	FilterPercent as int:
		get:
			return filterPercent
		set:
			filterPercent = value

	
	Filter as int:
		get:
			return (cast(int, (avgRange[0] * filterPercent)) / 100)

	
	Middle as IndicatorCommon:
		get:
			return middle

