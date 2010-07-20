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

class SRShortHourly(SRBreakoutSupport):

	def constructor():
		ExitStrategy.ControlStrategy = false
		IntervalDefault = Intervals.Minute10
		MinimumMove = 100
		ExitStrategy.StopLoss = 950
		//			MinimumMove = 32;
		//			ExitSupport.StopLoss = 530;
		//			ExitSupport.TrailStop = 680;

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if Days.Count < 3:
			return true
		
		if timeFrame.Equals(Intervals.Minute1):
			
			EntryLevel[0] = (Resistance[0] - Filter)
			
			if Minutes.Close[0] < EntryLevel[0]:
				Enter.Setup('under')
			
			if (Position.IsFlat and Enter.IsSetup('under')) and (Minutes.Close[0] > EntryLevel[0]):
				Enter.SellMarket()
			
			if Position.IsShort:
				if Minutes.High[0] < (Middle[1] + Filter):
					Exit.GoFlat()
		return true

	
	protected override ShortTrend as bool:
		get:
			return ((Resistance.Pivots[0] < Resistance.Pivots[1]) and (Support.Pivots[0] < Support.Pivots[1]))

