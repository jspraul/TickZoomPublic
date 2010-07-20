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

class RandomIntraday(StrategyCommon):

	private randomEntries as (TimeStamp) = null

	private randomIndex = 0

	private random = Random(393957857)

	private sessionHours = 4

	
	def constructor():
		randomEntries = array(TimeStamp, 20)

	
	override def OnProcessTick(tick as Tick) as bool:
		tickTime as TimeStamp = Ticks[0].Time
		if (Sessions.IsActive and (randomIndex < randomEntries.Length)) and (tickTime > randomEntries[randomIndex]):
			randSignal as int = (randomEntries[randomIndex].Second % 2)
			Position.Signal = (1 if (randSignal == 1) else (-1))
			
			randomIndex += 1
		return true

	
	override def OnIntervalOpen(timeFrame as Interval) as bool:
		for i in range(0, randomEntries.Length):
			randomEntries[i] = Sessions.Time[0]
			randomEntries[i].AddSeconds(random.Next(0, ((sessionHours * 60) * 60)))
		Array.Sort(randomEntries, 0, randomEntries.Length)
		randomIndex = 0
		return true

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		Exit.GoFlat()
		return true

