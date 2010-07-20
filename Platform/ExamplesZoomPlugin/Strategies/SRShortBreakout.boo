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

class SRShortBreakout(StrategyCommon):

	private weekMiddle as IndicatorCommon

	def constructor():
		pass

	
	override def OnInitialize():
		IntervalDefault = Intervals.Day1
		
		weekMiddle = WeekMiddle()
		AddIndicator(weekMiddle)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if Weeks.Low.Count < 2:
			return true
		if timeFrame.Equals(Intervals.Month1):
			if Position.HasPosition:
				Exit.GoFlat()
		
		if timeFrame.Equals(Intervals.Minute1):
			if Position.IsFlat and Formula.CrossesUnder(Bars.Typical, Weeks.Low[1], 1):
				Enter.SellMarket()
			if Position.HasPosition and Formula.CrossesOver(Bars.Typical, Weeks.High[1], 1):
				Exit.GoFlat()
		return true

