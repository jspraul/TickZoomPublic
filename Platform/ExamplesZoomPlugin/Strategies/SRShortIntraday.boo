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
import System.Collections.Generic
import TickZoom.Api
import TickZoom.Common

class SRShortIntraday(StrategyCommon):

	private range = 80

	private lines as List[of IndicatorCommon]

	private lineCount = 0

	private levels as (int)

	private length = 30

	private rewardFactor as double = 2

	private levelCount = 0

	
	def constructor():
		Drawing.Color = Color.Green
		IntervalDefault = Intervals.Hour1

	
	override def OnInitialize():
		lines = List[of IndicatorCommon]()
		maxLines = 2
		levels = array(int, maxLines)
		for i in range(0, maxLines):
			srLine = IndicatorCommon()
			srLine.Drawing.Color = Color.Orange
			lines.Add(srLine)
			AddIndicator(srLine)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Minute1):
			// Only play if trend is flat.
			if Next.Position.Signal == 0:
				if Position.IsFlat and Formula.CrossesUnder(Bars.Low, Weeks.Low[1]):
					Enter.BuyMarket()
				if Position.IsFlat and (Bars.High[0] > Days.High[1]):
					Exit.GoFlat()
			else:
				Exit.GoFlat()
		return true

	
	def MatchesAny(level as int) as int:
		for i in range(0, levelCount):
			if Matches(level, levels[i]):
				return i
		return (-1)

	
	def Matches(level1 as int, level2 as int) as bool:
		return (Math.Pow((level1 - level2), 2) <= Math.Pow((range * 2), 2))

	
	override def OnIntervalClose() as bool:
		lineCount = 0
		lines[(lineCount++)][0] = Days.High[1]
		lines[(lineCount++)][0] = Days.Low[1]
		for i in range(lineCount, lines.Count):
			lines[i][0] = Bars.Close[0]
		// Remove any duplicate S/R levels which 
		// are closer together than range.
		if lineCount < 1:
			return true
		levelCount = 0
		sr as int
		match as int
		for i in range(0, lineCount):
			sr = cast(int, lines[i][0])
			match = MatchesAny(sr)
			if match >= 0:
				levels[match] = ((sr + levels[match]) / 2)
			else:
				levels[levelCount] = sr
				levelCount += 1
		return true

	
	override def ToString() as string:
		return ((((rewardFactor + ',') + length) + ',') + range)

	
	Range as int:
		get:
			return range
		set:
			range = value

	
	Length as int:
		get:
			return length
		set:
			length = value

	
	RewardFactor as double:
		get:
			return rewardFactor
		set:
			rewardFactor = value
	

