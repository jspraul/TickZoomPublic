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

class SRRetraceHourly(StrategyCommon):

	private retrace as Retrace

	private contractSize = 1

	private stretch as IndicatorCommon

	private domRatio as DOMRatio

	
	def constructor():
		ExitStrategy.ControlStrategy = false
		IntervalDefault = Intervals.Define(BarUnit.Change, 50)

	
	override def OnInitialize():
		super.OnInitialize()
		retrace = Retrace()
		retrace.Drawing.Color = Color.Red
		retrace.IntervalDefault = Intervals.Hour1
		AddIndicator(retrace)
		
		stretch = IndicatorCommon()
		stretch.Drawing.Color = Color.Red
		stretch.Drawing.PaneType = PaneType.Secondary
		stretch.IntervalDefault = Intervals.Hour1
		AddIndicator(stretch)
		
		domRatio = DOMRatio()
		domRatio.Drawing.Color = Color.Green
		domRatio.Drawing.PaneType = PaneType.Secondary
		domRatio.IntervalDefault = Intervals.Minute1
		AddIndicator(domRatio)
		
		Reset()
		// Initialize

	
	private largestSize as double = 0

	private positionsize as double

	private newPositions as double

	override def OnProcessTick(tick as Tick) as bool:
		if Hours.Count == 1:
			Exit.GoFlat()
			Reset()
			return true
		if Position.HasPosition:
			// TODO: Handle commission costs correctly inside Trade object.
			profitTarget = cast(int, (100 + ((Position.Size * contractSize) * 10)))
			// *2 to double cost of commission.
			if Performance.ComboTrades.CurrentProfitLoss >= profitTarget:
				Exit.GoFlat()
				Reset()
				return true
		
		positionsize = Math.Max(0, ((retrace.Stretch / 100) - 1))
		newPositions = Position.Size
		
		if positionsize > largestSize:
			newPositions = (Position.Size + contractSize)
			largestSize = positionsize
			if retrace.Stretch > 1000:
				newPositions += contractSize
			if retrace.Stretch > 1500:
				newPositions += contractSize
			if retrace.Stretch > 2000:
				newPositions += contractSize
			if retrace.Stretch > 2500:
				newPositions += contractSize
			if retrace.Stretch > 3000:
				newPositions += contractSize
			if retrace.Stretch > 3500:
				newPositions += contractSize
		
		if (Position.IsFlat or Position.IsLong) and (Ticks[0].Bid < retrace[0]):
			Enter.BuyMarket(newPositions)
		
		if (Position.IsFlat or Position.IsShort) and (Ticks[0].Ask > retrace[0]):
			Enter.SellMarket(newPositions)
		return true

	
	override def OnIntervalOpen(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Day1):
			gap as double = Math.Abs((Ticks[0].Bid - Ticks[1].Bid))
			if gap > 500:
				Reset()
				//				 TradeSignal.GoFlat();
		
		if timeFrame.Equals(Intervals.Minute1):
			stretch[0] = retrace.Stretch
		return true

	
	private lastResetTime as TimeStamp

	def Reset():
		largestSize = 0
		lastResetTime = Ticks[0].Time
		if retrace.Count > 0:
			retrace.Reset()

