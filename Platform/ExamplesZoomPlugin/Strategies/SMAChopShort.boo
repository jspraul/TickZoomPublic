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

class SMAChopShort(StrategyCommon):

	private slow = 10

	private lowTrigger = 220

	private highTrigger = 300

	private slowAvg as SMA

	private diff as IndicatorCommon

	
	def constructor():
		Drawing.Color = Color.Magenta
		IntervalDefault = Intervals.Hour4

	
	override def OnInitialize():
		slowAvg = SMA(Bars.Close, slow)
		diff = IndicatorCommon()
		diff.Drawing.PaneType = PaneType.Secondary
		AddIndicator(slowAvg)
		AddIndicator(diff)

	
	override def ToString() as string:
		return ((((slow + ',') + lowTrigger) + ',') + highTrigger)

	
	override def OnIntervalClose() as bool:
		if Next.Position.IsShort or Next.Position.IsFlat:
			diff[0] = (Bars.Typical[0] - slowAvg[0])
			if diff[0] > highTrigger:
				Enter.SellMarket()
			if diff[0] < (-lowTrigger):
				Exit.GoFlat()
		else:
			Exit.GoFlat()
		return true

	
	Slow as int:
		get:
			return slow
		set:
			slow = value

	LowTrigger as int:
		get:
			return lowTrigger
		set:
			lowTrigger = value

	
	HighTrigger as int:
		get:
			return highTrigger
		set:
			highTrigger = value

