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

class PointFigure(StrategyCommon):

	private barFlag = false

	private boxSize = 50

	
	def constructor():
		IntervalDefault = Intervals.Define(BarUnit.PointFigure, boxSize)

	
	override def OnInitialize():
		pass

	
	override def OnProcessTick(tick as Tick) as bool:
		high as double = (Bars.High[1] + boxSize)
		low as double = (Bars.Low[1] - boxSize)
		if Position.IsLong and (not barFlag):
			if Performance.ComboTrades.CurrentProfitLoss > 0:
				if Position.Size > 1:
					Enter.BuyMarket()
					// Reduce TradeSignal.Positions.
				if Ticks[0].Bid < low:
					Enter.SellMarket()
			elif (Ticks[0].Ask > high) and (Ticks[0].Ask < (Position.SignalPrice - 100)):
				Enter.BuyMarket((Position.Size * 1.5))
				barFlag = true
		if Position.IsShort and (not barFlag):
			if Performance.ComboTrades.CurrentProfitLoss > 0:
				if Position.Size > 1:
					Enter.SellMarket()
					// Reduce TradeSignal.Positions.
				if Bars.High[0] > high:
					Enter.BuyMarket()
			elif (Ticks[0].Bid < low) and (Ticks[0].Bid > (Position.SignalPrice + 100)):
				Enter.SellMarket((Position.Size * 1.5))
				barFlag = true
		if Position.IsFlat:
			if Ticks[0].Ask > high:
				Enter.BuyMarket()
			if Ticks[0].Bid < low:
				Enter.SellMarket()
		return true

	
	private holdPositions as double = 0

	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Week1):
			if Position.IsShort:
				holdPositions = (-Position.Size)
				Enter.SellMarket()
				// Reduce weekend risk.
			if Position.IsLong:
				holdPositions = Position.Size
				Enter.BuyMarket()
				// Reduce weekend risk.
		return true

	
	override def OnIntervalOpen(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Week1):
			if Position.IsShort:
				Enter.SellMarket(holdPositions)
				// Return to pre-weekend TradeSignal.Positions.
			if Position.IsLong:
				Enter.BuyMarket(holdPositions)
				// return to pre-weekend TradeSignal.Positions.
		holdPositions = 0
		return true

	override def OnIntervalClose() as bool:
		barFlag = false
		return true
	

