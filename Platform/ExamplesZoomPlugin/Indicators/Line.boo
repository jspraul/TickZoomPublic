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

class Line:

	private interceptBar as int

	
	private bar1 as int

	private y1 as int

	private bar2 as int

	private y2 as int

	
	private lr = LinearRegression()

	
	def constructor(interceptBar as int):
		self.interceptBar = interceptBar

	
	LastBar as int:
		get:
			return interceptBar

	
	def GetYFromBar(bar as int) as int:
		return GetY((interceptBar - bar))

	
	def setPoints(p1 as Point, p2 as Point):
		setPoints(p1.X, p1.Y, p2.X, p2.Y)

	
	def setPoints(bar1 as int, y1 as int, bar2 as int, y2 as int):
		lr.clearPoints()
		self.bar1 = bar1
		self.y1 = y1
		self.bar2 = bar2
		self.y2 = y2
		lr.addPoint((interceptBar - bar1), y1)
		lr.addPoint((interceptBar - bar2), y2)

	
	def calculate():
		lr.calculate()

	
	def GetY(x as int) as int:
		return cast(int, (lr.Intercept + (x * lr.Slope)))

	
	private highest = 0

	private lowest = 0

	def calcMaxDev(bars as Bars, length as int):
		// Find max deviation from the line.
		highest = 0
		lowest = 0
		for bar in range(0, length):
			lineY as double = GetY(bar)
			highY as double = bars.High[bar]
			lowY as double = bars.Low[bar]
			if (highY - lineY) > highest:
				highest = cast(int, (highY - lineY))
			if (lineY - lowY) > lowest:
				lowest = cast(int, (lineY - lowY))

	
	IsDown as bool:
		get:
			return (lr.Slope >= 0)

	
	IsUp as bool:
		get:
			return (lr.Slope < 0)

	
	def extend(length as int):
		bar1 += length
		y1 = GetYFromBar(bar1)

	
	Bar1 as int:
		get:
			return bar1

	
	Y1 as int:
		get:
			return y1

	
	Bar2 as int:
		get:
			return bar2

	
	Y2 as int:
		get:
			return y2

	
	Length as int:
		get:
			return (interceptBar - bar2)

	
	Highest as int:
		get:
			return highest

	
	Lowest as int:
		get:
			return lowest

