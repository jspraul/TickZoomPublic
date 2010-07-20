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

class Retrace2(IndicatorCommon):

	private lowest as double
	private highest as double
	private stretch as double
	private rebound as int
	
	def constructor():
		pass

	override def OnInitialize():
		pass
	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if Bars.BarCount <= 2:
			Reset()
			return true
		if timeFrame.Equals(Intervals.Minute1):
			if double.IsNaN(self[0]):
				self[0] = ((Bars.Low[0] + Bars.High[0]) / 2)
				lowest = (highest = cast(int, self[0]))
			else:
				if Bars.High[0] > highest:
					self[0] += (Math.Max((Bars.High[0] - highest), 0) / 2)
					highest = Bars.High[0]
				if Bars.Low[0] < lowest:
					self[0] -= (Math.Max((lowest - Bars.Low[0]), 0) / 2)
					lowest = Bars.Low[0]
				if Formula.CrossesUnder(Bars.Low, self[0]) or Formula.CrossesOver(Bars.High, self[0]):
					highest = Bars.High[0]
					lowest = Bars.Low[0]
			stretch = (highest - lowest)
			rebound = cast(int, Math.Max(0, (stretch - Math.Abs((Bars.Close[0] - self[0])))))
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
	
