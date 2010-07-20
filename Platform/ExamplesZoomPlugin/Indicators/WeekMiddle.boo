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

class WeekMiddle(IndicatorCommon):

	def constructor():
		IntervalDefault = Intervals.Day1
		Drawing.Color = Color.Red
		Drawing.PaneType = PaneType.Primary

	
	override def OnIntervalClose() as bool:
		if Weeks.High.Count < 2:
			return true
		//			this[0] = (Math.Min(Formula.Weeks.Low[1],Formula.Weeks.Low[0]) +
		//			                    Math.Max(Formula.Weeks.High[1],Formula.Weeks.High[0]))/2;
		self[0] = ((Weeks.Low[1] + Weeks.High[1]) / 2)
		//			this[0] = (Weeks.Low[1] + Highest(Weeks.High,2))/2;
		return true
	

