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

class PivotLowVs(IndicatorCommon):

	private pivotLows as Doubles

	private pivotBars as Integers

	private LSTREN as int

	private RSTREN as int

	private LENGTH as int

	private SHBAR as double = 0

	private pivot as double = 0

	private pivotBar = 0

	private disableBoxes = false

	private rightPassed = false

	private leftPassed = false

	private highest as double = 0

	
	def constructor():
		self(2, 2, 5)

	
	def constructor(left as int, right as int, length as int):
		pivotLows = Doubles()
		pivotBars = Integers()
		LSTREN = left
		RSTREN = right
		LENGTH = Math.Max(length, ((left + right) + 1))
		Drawing.Color = Color.Aqua

	
	def constructor(left as int, right as int):
		self(left, right, ((left + right) + 1))

	
	override def OnInitialize():
		pass

	
	override def OnIntervalClose() as bool:
		currHigh as double = Bars.High[0]
		if highest == 0:
			highest = currHigh
		elif currHigh > highest:
			highest = currHigh
		// DO NOT DELETE. Turn to hidden instead if necessary.
		self[0] = highest
		
		pivot = 0
		for MAINLOOP in range(RSTREN, LENGTH):
			rightPassed = true
			leftPassed = true
			SHBAR = (Bars.Low[MAINLOOP] - 1)
			for VALUE1 in range((MAINLOOP - RSTREN), MAINLOOP):
			
				if SHBAR > Bars.Low[VALUE1]:
					rightPassed = false
					break 
			
			if rightPassed:
				for VALUE1 in range((MAINLOOP + 1), (MAINLOOP + (LSTREN + 1))):
					if SHBAR > Bars.Low[VALUE1]:
						leftPassed = false
						break 
			
			if rightPassed and leftPassed:
				pivot = SHBAR
				pivotBar = MAINLOOP
				break 
		
		if (leftPassed and rightPassed) and ((pivotLows.Count == 0) or (pivot != pivotLows[0])):
			if pivot < self[pivotBar]:
				highest = pivot
				pivotLows.Add(pivot)
				bar as int = (Bars.CurrentBar - pivotBar)
				pivotBars.Add(bar)
				elapsed as Elapsed
				if pivotBars.Count > 1:
					elapsed = (Bars.Time[pivotBar] - Bars.Time[(Bars.CurrentBar - pivotBars[1])])
				if not disableBoxes:
					Chart.DrawBox(Drawing.Color, bar, pivot)
					if pivotBars.Count > 1:
						Log.Debug(((((((('|Pivot Low,' + Bars.Time[pivotBar]) + ',') + elapsed.TotalSeconds) + ',') + Bars.Time[pivotBar]) + ',') + Bars.Time[(Bars.CurrentBar - pivotBars[1])]))
		
		return true

	Left as int:
		get:
			return LSTREN
		set:
			LSTREN = value

	
	Right as int:
		get:
			return RSTREN
		set:
			RSTREN = value

	
	Length as int:
		get:
			return LENGTH
		set:
			LENGTH = value

	
	PivotLows as Doubles:
		get:
			return pivotLows

	
	PivotBars as Integers:
		get:
			return pivotBars

	
	DisableBoxes as bool:
		get:
			return disableBoxes
		set:
			disableBoxes = value

