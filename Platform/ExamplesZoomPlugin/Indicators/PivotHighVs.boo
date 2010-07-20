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

class PivotHighVs(IndicatorCommon):

	private pivotBars as Integers

	private pivotHighs as Doubles

	private leftStrength as int

	private rightStrength as int

	private length as int

	private pivotHigh as double = 0

	private pivot as double = 0

	private pivotBar = 0

	private disableBoxes = false

	private pivotPassed = false

	private leftPassed = false

	protected lowest as double

	
	def constructor():
		self(2, 2, 5)

	
	def constructor(left as int, right as int, length as int):
		pivotHighs = Doubles()
		pivotBars = Integers()
		self.leftStrength = left
		self.rightStrength = right
		self.length = Math.Max(length, ((left + right) + 1))
		Drawing.Color = Color.Orange

	
	def constructor(left as int, right as int):
		self(left, right, ((left + right) + 1))

	
	override def OnInitialize():
		pass

	
	override def OnIntervalClose() as bool:
		currLow as double = Bars.Low[0]
		if lowest == 0:
			lowest = currLow
		elif currLow < lowest:
			lowest = currLow
		// DO NOT DELETE : Turn to hidden instead if necessary.
		self[0] = lowest
		
		pivot = 0
		pivotBar = 0
		for mainLoop in range(rightStrength, length):
			pivotPassed = true
			leftPassed = true
			pivotHigh = (Bars.High[mainLoop] + 1)
			for i in range((mainLoop - rightStrength), mainLoop):
			
				if pivotHigh < Bars.High[i]:
					pivotPassed = false
					break 
			if pivotPassed:
				for i in range((mainLoop + 1), (mainLoop + (leftStrength + 1))):
					if pivotHigh < Bars.High[i]:
						leftPassed = false
						break 
			
			if leftPassed and pivotPassed:
				pivot = pivotHigh
				pivotBar = mainLoop
				break 
		if (leftPassed and pivotPassed) and ((pivotHighs.Count == 0) or (pivot != pivotHighs[0])):
			if pivot > self[pivotBar]:
				lowest = pivot
				pivotHighs.Add(pivot)
				bar as int = (Bars.CurrentBar - pivotBar)
				pivotBars.Add(bar)
				elapsed as Elapsed
				if pivotBars.Count > 1:
					elapsed = (Bars.Time[pivotBar] - Bars.Time[(Bars.CurrentBar - pivotBars[1])])
				if not disableBoxes:
					Chart.DrawBox(Drawing.Color, bar, pivot)
					if pivotBars.Count > 1:
						Log.Debug(((((((('|Pivot High,' + Bars.Time[pivotBar]) + ',') + elapsed.TotalSeconds) + ',') + Bars.Time[pivotBar]) + ',') + Bars.Time[(Bars.CurrentBar - pivotBars[1])]))
		
		return true

	LeftStrength as int:
		get:
			return leftStrength
		set:
			leftStrength = value

	
	RightStrength as int:
		get:
			return rightStrength
		set:
			rightStrength = value

	
	Length as int:
		get:
			return length
		set:
			length = value

	
	PivotHighs as Doubles:
		get:
			return pivotHighs

	
	PivotBars as Integers:
		get:
			return pivotBars

	
	DisableBoxes as bool:
		get:
			return disableBoxes
		set:
			disableBoxes = value
	

