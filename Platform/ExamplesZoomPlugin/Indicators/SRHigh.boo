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

class SRHigh(IndicatorCommon):

	private minimumMove as double

	
	private pivots as Integers

	
	def constructor():
		self(300)

	
	def constructor(minMove as double):
		pivots = Integers()
		self.minimumMove = minMove

	
	override def OnInitialize():
		pass
		

	
	private lowestLow as double = int.MaxValue

	private highestHigh as double = int.MinValue

	override def OnIntervalClose() as bool:
		if Count == 1:
			highestHigh = Bars.High[0]
			lowestLow = Bars.Low[0]
		else:
			// this goes higher while..
			highestHigh = Math.Max(highestHigh, Bars.High[0])
			
			// lowest goes lower
			lowestLow = Math.Min(Bars.Low[0], lowestLow)
			
			// when the high rises high enough it becomes a new potential pivot.
			if (Bars.High[0] > Formula.Highest(Bars.High, 3, 1)) and ((Bars.High[0] - lowestLow) > minimumMove):
				highestHigh = Bars.High[0]
				lowestLow = Bars.Low[0]
			
			// when low drops below highest far enough, it's a new pivot
			if (self[0] != highestHigh) and ((highestHigh - Bars.Low[0]) > minimumMove):
				self[0] = highestHigh
				pivots.Add(cast(int, self[0]))
			
		return true

	
	Pivots as Integers:
		get:
			return pivots

	
	MinimumMove as double:
		get:
			return minimumMove
		set:
			minimumMove = value

