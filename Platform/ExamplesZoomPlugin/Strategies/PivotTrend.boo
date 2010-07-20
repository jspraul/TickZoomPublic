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

class PivotTrend(StrategyCommon):

	private pivotLow as PivotLowVs

	private pivotHigh as PivotHighVs

	private pivotTimeFrame as Interval

	private leftStrength = 2

	// Weeks
	private rightStrength = 10

	// Days
	private length = 0

	private pivotColor as Color = Color.Blue

	private trendStrength = 5

	private lastBreakOut = 0

	
	def constructor():
		// Set defaults here.
		pivotTimeFrame = IntervalDefault

	
	override def OnInitialize():
		pivotLow = PivotLowVs(leftStrength, rightStrength)
		pivotLow.IntervalDefault = pivotTimeFrame
		pivotLow.Drawing.Color = pivotColor
		pivotHigh = PivotHighVs(leftStrength, rightStrength)
		pivotHigh.IntervalDefault = pivotTimeFrame
		pivotHigh.Drawing.Color = pivotColor
		AddIndicator(pivotLow)
		AddIndicator(pivotHigh)

	
	override def OnIntervalClose() as bool:
		if (pivotHigh.Count > 1) and (pivotLow.Count > 1):
			if Bars.High[0] > pivotHigh[0]:
				Enter.BuyMarket()
				lastBreakOut = Bars.CurrentBar
			elif Bars.Low[0] < pivotLow[0]:
				Enter.SellMarket()
				lastBreakOut = Bars.CurrentBar
		// If too long since last break out, go flat.
		if (Bars.CurrentBar - lastBreakOut) >= trendStrength:
			Exit.GoFlat()
		// If child strategy, make sure signal direction matches.
		if (Next is not null) and (Next.Position.Signal != Position.Signal):
			Exit.GoFlat()
		return true

	
	PivotTimeFrame as Interval:
		get:
			return pivotTimeFrame
		set:
			pivotTimeFrame = value

	
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

	
	override def ToString() as string:
		return ((((leftStrength + ',') + rightStrength) + ',') + trendStrength)

	
	PivotColor as Color:
		get:
			return pivotColor
		set:
			pivotColor = value

	
	TrendStrength as int:
		get:
			return trendStrength
		set:
			trendStrength = value
	

