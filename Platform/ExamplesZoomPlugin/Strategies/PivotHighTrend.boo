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
import System.Collections.Generic
import TickZoom.Api
import TickZoom.Common

class PivotHighTrend(StrategyCommon):

	private pivotHigh as PivotHighVs

	private pivotLow as PivotLowVs

	private strength = 6

	private pivotDiff as IndicatorCommon

	private maxPivotDiff = 1000

	
	def constructor():
		pass

	
	override def OnInitialize():
		pivotHigh = PivotHighVs(strength, strength)
		pivotHigh.IntervalDefault = Intervals.Hour1
		AddIndicator(pivotHigh)
		
		pivotLow = PivotLowVs(strength, strength)
		pivotLow.IntervalDefault = Intervals.Hour1
		AddIndicator(pivotLow)
		
		pivotDiff = IndicatorCommon()
		pivotDiff.IntervalDefault = Intervals.Hour1
		AddIndicator(pivotDiff)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Minute1):
			pivotDiff[0] = (pivotHigh.PivotHighs[0] - pivotLow.PivotLows[0])
			if pivotDiff[0] < maxPivotDiff:
				if (Position.IsShort | Position.IsFlat) and Formula.CrossesOver(Hours.High, cast(int, pivotHigh[1])):
					Enter.BuyMarket()
			if (Position.IsLong | Position.IsFlat) and Formula.CrossesUnder(Hours.Low, cast(int, pivotLow[1])):
				Exit.GoFlat()
		return true

	
	override def OnGetOptimizeResult(optimizeValues as Dictionary[of string, object]) as string:
		result as string = super.OnGetOptimizeResult(optimizeValues)
		return ((((((pivotHigh.LeftStrength + ',') + pivotHigh.RightStrength) + ',') + pivotHigh.Length) + ',') + result)

	
	Strength as int:
		get:
			return strength
		set:
			strength = value
	

