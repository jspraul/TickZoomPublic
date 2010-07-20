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

class SRShortDayLow(StrategyCommon):

	private entryLevel as double = 0

	private profitTarget as double = 1000

	private dayHigh = IndicatorCommon()

	private dayLow = IndicatorCommon()

	
	def constructor():
		IntervalDefault = Intervals.Minute1

	
	override def OnInitialize():
		AddIndicator(dayHigh)
		AddIndicator(dayLow)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Day1):
			// Exit at the end of the day.
			Exit.GoFlat()
		if timeFrame.Equals(IntervalDefault):
			dayHigh[0] = Days.High[1]
			dayLow[0] = Days.Low[1]
			// Do we have a setup?
			if Next.Position.HasPosition:
				if Formula.CrossesUnder(Bars.Typical, Days.Low[1]):
					Enter.SellMarket()
					entryLevel = Bars.Close[0]
			else:
				Exit.GoFlat()
			// Look for profit target!
			if Position.HasPosition and Formula.CrossesUnder(Minutes.Low, (entryLevel - profitTarget)):
				Exit.GoFlat()
		return true

	
	ProfitTarget as double:
		get:
			return profitTarget
		set:
			profitTarget = value

