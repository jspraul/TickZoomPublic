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

class SRShortChannel(StrategyCommon):

	private profitTarget = 1000

	private weekHigh as IndicatorCommon

	private halfLine as IndicatorCommon

	private weekLow as IndicatorCommon

	private length = 4

	
	def constructor():
		pass

	
	override def OnInitialize():
		weekHigh = WeekHigh()
		AddIndicator(weekHigh)
		
		weekLow = WeekLow()
		AddIndicator(weekLow)
		
		halfLine = WeekMiddle()
		AddIndicator(halfLine)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		weekHigh[0] = Weeks.High[1]
		weekLow[0] = Weeks.Low[1]
		halfLine[0] = ((Math.Max(Weeks.High[0], Weeks.High[1]) + Math.Min(Weeks.Low[0], Weeks.Low[1])) / 2)
		if timeFrame.Equals(Intervals.Minute1):
			if (Position.IsFlat and (Hours.High[length] < weekHigh[length])) and Formula.CrossesUnder(Hours.High, Weeks.High[1], 1):
				Enter.SellMarket()
			if Position.HasPosition and Formula.CrossesUnder(Minutes.Low, Weeks.Low[1], 1):
				Exit.GoFlat()
		return true

	
	ProfitTarget as int:
		get:
			return profitTarget
		set:
			profitTarget = value

	
	Length as int:
		get:
			return length
		set:
			length = value

