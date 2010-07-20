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

class SRLongChannel(StrategyCommon):

	private profitTarget = 1000

	
	def constructor():
		pass

	
	override def OnInitialize():
		pass

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Minute1):
			// Do we have a setup?
			if Next.Position.IsFlat or Next.Position.IsShort:
				if Formula.CrossesOver(Minutes.Low, (Weeks.Low[1] + 100), 1):
					Enter.BuyMarket()
			else:
				Exit.GoFlat()
		if timeFrame.Equals(Intervals.Minute1):
			// Look for profit target!
			high as double = (Weeks.High[1] - 50)
			if Position.HasPosition and Formula.CrossesOver(Minutes.High, high):
				Exit.GoFlat()
		return true

	
	ProfitTarget as int:
		get:
			return profitTarget
		set:
			profitTarget = value

