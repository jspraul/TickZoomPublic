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

class Retrace(IndicatorCommon):

	private lowest as double
	private highest as double
	private stretch as double
	private adjustPercent = 0.5

	def constructor():
		pass

	
	override def OnInitialize():
		pass

	
	override def OnProcessTick(tick as Tick) as bool:
		prevTick as Tick = Ticks[1]
		middle as double = ((tick.Ask + tick.Bid) / 2)
		if Hours.BarCount <= 2:
			Reset()
			return true
		if double.IsNaN(self[0]):
			self[0] = middle
			lowest = (highest = cast(int, self[0]))
		else:
			if middle > highest:
				self[0] += (Math.Max((middle - highest), 0) * adjustPercent)
				highest = middle
			if middle < lowest:
				self[0] -= (Math.Max((lowest - middle), 0) * adjustPercent)
				lowest = middle
			if ((tick.Ask < self[0]) and (prevTick.Ask >= self[0])) or ((tick.Bid > self[0]) and (prevTick.Bid <= self[0])):
				highest = middle
				lowest = middle
		stretch = (highest - lowest)
		//			rebound = (int) Math.Max(0,stretch-Math.Abs(middle - this[0]));
		return true

	
	def Rebound(price as int) as int:
		rebound = cast(int, Math.Max(0, (stretch - Math.Abs((price - self[0])))))
		return rebound

	
	Stretch as double:
		get:
			return stretch

	
	def Reset():
		highest = Bars.High[0]
		lowest = Bars.Low[0]
		self[0] = ((highest + lowest) / 2)

	
	RetracePercent as double:
		get:
			return (1 - adjustPercent)
		set:
			adjustPercent = (1 - value)

