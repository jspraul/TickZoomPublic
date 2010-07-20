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

class LRGraph(IndicatorCommon):

	private fastLR as Channel

	private fastLength = 5

	private middleLR as Channel

	private middle as IndicatorCommon

	private middleLength = 15

	private slowLR as Channel

	private slow as IndicatorCommon

	private slowLength = 45

	
	def constructor():
		super()
		Drawing.GraphType = GraphType.Histogram
		Drawing.PaneType = PaneType.Secondary
		Drawing.GroupName = 'Fast'

	
	override def OnInitialize():
		slowLR = Channel(Bars)
		middleLR = Channel(Bars)
		fastLR = Channel(Bars)
		middle = IndicatorCommon()
		middle.Drawing.GraphType = GraphType.Histogram
		middle.Drawing.PaneType = PaneType.Secondary
		middle.Drawing.GroupName = 'Middle'
		AddIndicator(middle)
		
		slow = IndicatorCommon()
		slow.Drawing.GraphType = GraphType.Histogram
		slow.Drawing.PaneType = PaneType.Secondary
		slow.Drawing.GroupName = 'Slow'
		AddIndicator(slow)

	
	override def OnIntervalClose() as bool:
		fastLR.addPoint(Bars.CurrentBar, Formula.Middle(Bars, 0))
		if fastLR.CountPoints >= fastLength:
			fastLR.Calculate()
			fastLR.UpdateEnds()
			self[0] = fastLR.Slope
		
		middleLR.addPoint(Bars.CurrentBar, Formula.Middle(Bars, 0))
		if middleLR.CountPoints >= middleLength:
			middleLR.Calculate()
			middleLR.UpdateEnds()
			middle[0] = middleLR.Slope
		
		slowLR.addPoint(Bars.CurrentBar, Formula.Middle(Bars, 0))
		if slowLR.CountPoints >= slowLength:
			slowLR.Calculate()
			slowLR.UpdateEnds()
			slow[0] = slowLR.Slope
		return true

	
	Middle as IndicatorCommon:
		get:
			return middle

	
	Slow as IndicatorCommon:
		get:
			return slow

	Code as int:
		get:
			fastBit as int = (1 if (self[0] > 1) else 0)
			middleBit as int = (1 if (middle[0] > 1) else 0)
			slowBit as int = (1 if (slow[0] > 0) else 0)
			return (((slowBit * 4) + (middleBit * 2)) + fastBit)

