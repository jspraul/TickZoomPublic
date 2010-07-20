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

class DOMRatio(IndicatorCommon):

	private bidSize as IndicatorCommon

	private askSize as IndicatorCommon

	private ratio as IndicatorCommon

	private extreme as IndicatorCommon

	private average as IndicatorCommon

	private dryUp as IndicatorCommon

	
	Ratio as IndicatorCommon:
		get:
			return ratio

	
	def constructor():
		Drawing.PaneType = PaneType.Secondary

	
	override def OnInitialize():
		Drawing.GroupName = 'DOM'
		
		ratio = IndicatorCommon()
		ratio.Drawing.Color = Color.Magenta
		ratio.Drawing.GroupName = 'Ratio'
		AddIndicator(ratio)
		
		bidSize = IndicatorCommon()
		bidSize.Drawing.Color = Color.Blue
		bidSize.Drawing.GroupName = 'InnerDOM'
		AddIndicator(bidSize)
		
		askSize = IndicatorCommon()
		askSize.Drawing.Color = Color.Magenta
		askSize.Drawing.GroupName = 'InnerDOM'
		AddIndicator(askSize)
		
		extreme = Formula.Line(Extreme, Color.Red)
		average = Formula.Line(Average, Color.Orange)
		dryUp = Formula.Line(DryUp, Color.Green)

	
	override def OnIntervalOpen() as bool:
		bidSize[0] = 0
		askSize[0] = 0
		return true

	
	private totalBidSize as long = 0

	private totalAskSize as long = 0

	private tickCount as long = 0

	override def OnProcessTick(tick as Tick) as bool:
		askDepth as int = tick.AskDepth
		bidDepth as int = tick.BidDepth
		totalAskSize += askDepth
		totalBidSize += bidDepth
		tickCount += 1
		
		if bidDepth > askDepth:
			ratio[0] = (0 if (askDepth == 0) else (bidDepth / askDepth))
		elif bidDepth < askDepth:
			ratio[0] = (0 if (bidDepth == 0) else ((-askDepth) / bidDepth))
		else:
			ratio[0] = 0
		
		bidSize[0] = (tick.BidLevel(0) + tick.BidLevel(1))
		askSize[0] = (tick.AskLevel(0) + tick.AskLevel(1))
		
		return true

	
	private endDate = TimeStamp(2007, 8, 29)

	override def OnIntervalClose() as bool:
		if (totalAskSize > 0) and (tickCount > 0):
			self[0] = cast(int, ((totalBidSize + totalAskSize) / tickCount))
		
		tickCount = 0
		totalBidSize = 0
		totalAskSize = 0
		
		CalcStrength()
		return true

	
	Extreme as int:
		get:
			return 6000

	
	Average as int:
		get:
			return 4000

	
	DryUp as int:
		get:
			return 2000

	
	BidSize as IndicatorCommon:
		get:
			return bidSize

	
	AskSize as IndicatorCommon:
		get:
			return askSize

	
	private strength as Strength = Strength.Lo

	
	Strength as Strength:
		get:
			return strength

	private dryUpTimer as TimeStamp

	private def CalcStrength():
		if self[0] > Extreme:
			strength = Strength.Ex
		elif self[0] > Average:
			strength = Strength.Hi
		elif self[0] > DryUp:
			strength = Strength.Lo
		else:
			strength = Strength.DU
			dryUpTimer = Ticks[0].Time
			dryUpTimer.AddMinutes(5)

	
	IsDryUp as bool:
		get:
			return ((strength == Strength.DU) or (Ticks[0].Time < dryUpTimer))

	
	IsLow as bool:
		get:
			return (strength == Strength.Lo)

	
	IsHigh as bool:
		get:
			return (strength == Strength.Hi)

	
	IsExtreme as bool:
		get:
			return (strength == Strength.Ex)

