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

class BreakoutTrend(StrategyCommon):

	private indicatorTimeFrame as Interval

	private length = 0

	private indicatorColor as Color = Color.Blue

	private trendStrength = 5

	private lastBreakOut = 0

	
	def constructor():
		// Set defaults here.
		indicatorTimeFrame = IntervalDefault

	
	override def OnIntervalClose() as bool:
		if Bars.High[0] > Formula.Highest(Bars.High, length):
			Enter.SellMarket()
			lastBreakOut = Bars.CurrentBar
		elif Bars.Low[0] < Formula.Lowest(Bars.Low, length):
			Enter.BuyMarket()
			lastBreakOut = Bars.CurrentBar
		median as double = ((Bars.High[0] + Bars.Low[0]) / 2)
		if Position.IsLong:
			if median < Position.SignalPrice:
				Exit.GoFlat()
		if Position.IsShort:
			if median > Position.SignalPrice:
				Exit.GoFlat()
		
		// If too long since last break out, go flat.
		if (Bars.CurrentBar - lastBreakOut) >= trendStrength:
			Exit.GoFlat()
		
		
		//			// If child strategy, make sure signal direction matches.
		if (Next is not null) and (Next.Position.Signal != Position.Signal):
			Exit.GoFlat()
		return true

	
	Length as int:
		get:
			return length
		set:
			length = value

	
	override def ToString() as string:
		return ((length + ',') + trendStrength)

	
	PivotColor as Color:
		get:
			return indicatorColor
		set:
			indicatorColor = value

	
	TrendStrength as int:
		get:
			return trendStrength
		set:
			trendStrength = value
	

