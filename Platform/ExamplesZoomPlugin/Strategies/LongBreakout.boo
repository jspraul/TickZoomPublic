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

class LongBreakout(StrategyCommon):

	private indicatorTimeFrame as Interval

	private sma as SMA

	private indicatorTimePeriod as int

	private breakoutLength = 4

	private indicatorColor as Color = Color.White

	private averageLength = 32

	private displace = 2

	private rewardFactor = 2

	private hourPeriod = 1

	private historicalReward = 4500

	
	def constructor():
		// Set defaults here.
		indicatorTimeFrame = IntervalDefault
		Drawing.Color = Color.Green
		IndicatorColor = Color.Green
		IntervalDefault = Intervals.Day1

	
	override def OnInitialize():
		sma = SMA(Bars.Close, averageLength)
		sma.IntervalDefault = IntervalDefault
		AddIndicator(sma)

	private barCount = 0

	override def OnIntervalClose(timeFrame as Interval) as bool:
		if (timeFrame.BarUnit == BarUnit.Hour) and (timeFrame.Period == hourPeriod):
			barCount += 1
			if Bars.BarCount > 10:
				HandleEntry()
				HandleExit()
		return true

	
	private def HandleEntry():
		resistance as double = Formula.Highest(Bars.High, breakoutLength, (displace + 1))
		if (Bars.Typical[1] > resistance) and (Ticks[0].Bid > resistance):
			Enter.BuyMarket()

	
	private def HandleExit():
		if Position.IsLong:
			if Hours.Typical[0] < sma[0]:
				Exit.GoFlat()

	
	IndicatorTimePeriod as int:
		get:
			return indicatorTimePeriod
		set:
			indicatorTimePeriod = value

	
	BreakoutLength as int:
		get:
			return breakoutLength
		set:
			breakoutLength = value

	
	override def ToString() as string:
		return ((((((((rewardFactor + ',') + displace) + ',') + breakoutLength) + ',') + averageLength) + ',') + barCount)

	
	IndicatorColor as Color:
		get:
			return indicatorColor
		set:
			indicatorColor = value

	
	AverageLength as int:
		get:
			return averageLength
		set:
			averageLength = value

	
	Displace as int:
		get:
			return displace
		set:
			displace = value

	
	RewardFactor as int:
		get:
			return rewardFactor
		set:
			rewardFactor = value

	
	HistoricalReward as int:
		get:
			return historicalReward
		set:
			historicalReward = value

	
	HourPeriod as int:
		get:
			return hourPeriod
		set:
			hourPeriod = value

