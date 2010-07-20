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
import System.Drawing
import TickZoom.Common

class TwelveMonthLow(IndicatorCommon):

	def constructor():
		IntervalDefault = Intervals.Day1
		Drawing.Color = Color.Magenta
		Drawing.PaneType = PaneType.Primary

	
	override def OnIntervalClose() as bool:
		if Months.BarCount > 12:
			self[0] = Formula.Lowest(Months.Low, 12, 1)
		return true
	

