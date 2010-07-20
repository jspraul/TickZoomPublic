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

class PercentRStrategy(StrategyCommon):

	private slow = 14

	private fast = 14

	private tema as TEMA

	private sma as SMA

	private percentR as IndicatorCommon

	private indicatorColor as Color = Color.Blue

	private threshhold = 20

	
	def constructor():
		pass

	
	override def OnInitialize():
		sma = SMA(Bars.Close, slow)
		sma.IntervalDefault = IntervalDefault
		percentR = IndicatorCommon()
		tema = TEMA(Bars.Close, slow)
		AddIndicator(tema)
		AddIndicator(percentR)
		AddIndicator(sma)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(IntervalDefault):
			high as double = Formula.Highest(Bars.High, (slow + 1))
			low as double = Formula.Lowest(Bars.Low, (slow + 1))
			current = cast(int, tema[0])
			if sma.Count > slow:
				if Next.Position.Signal > 0:
					low = cast(int, Math.Min(sma[0], Bars.Low[0]))
				if Next.Position.Signal < 0:
					high = cast(int, Math.Max(sma[0], Bars.High[0]))
			if percentR.Count > 5:
				percentR[0] = (((current - low) * 100) / ((high - low) + 1))
			else:
				percentR[0] = 50
			if Next.Position.Signal == 0:
				if percentR[0] > (100 - threshhold):
					Enter.SellMarket()
				if percentR[0] < threshhold:
					Enter.BuyMarket()
			else:
				Exit.GoFlat()
		return true

	
	Slow as int:
		get:
			return slow
		set:
			slow = value

	
	override def ToString() as string:
		return ((slow + ',') + threshhold)

	
	IndicatorColor as Color:
		get:
			return indicatorColor
		set:
			indicatorColor = value

	
	Fast as int:
		get:
			return fast
		set:
			fast = value

	
	Threshhold as int:
		get:
			return threshhold
		set:
			threshhold = value

