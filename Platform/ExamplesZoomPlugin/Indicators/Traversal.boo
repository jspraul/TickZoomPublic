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

class Traversal(IndicatorCommon):

	private low as PivotLowVs

	private high as PivotHighVs

	private zigBars as Integers

	private zigHighs as Doubles

	private zagBars as Integers

	private zagLows as Doubles

	private threshold = 100

	private costs = 25

	private simTradeStart as TimeStamp

	private simTradeLast as TimeStamp

	private debug = true

	private startMorning = Elapsed(0, 15, 0)

	private endMorning = Elapsed(14, 0, 0)

	
	// Fields that must be reset outside trading times.
	private simTradeTotal as double = 0

	private simTradeCount = 0

	private lastHighBar = 0

	private lastLowBar = 0

	private lastHighPrice as double = 0

	private lastLowPrice as double = double.MaxValue

	
	def constructor():
		super()
		zigBars = Integers(5)
		zigHighs = Doubles(5)
		zagBars = Integers(5)
		zagLows = Doubles(5)
		Drawing.Color = Color.LightGreen

	
	def Reset():
		simTradeTotal = 0
		simTradeCount = 0
		lastHighBar = 0
		lastLowBar = 0
		lastHighPrice = 0
		lastLowPrice = int.MaxValue
		zigBars.Clear()
		zagBars.Clear()
		zigHighs.Clear()
		zagLows.Clear()

	
	override def OnInitialize():
		low = PivotLowVs(1, 1)
		low.IntervalDefault = IntervalDefault
		low.DisableBoxes = true
		AddIndicator(low)
		
		high = PivotHighVs(1, 1)
		high.IntervalDefault = IntervalDefault
		high.DisableBoxes = true
		AddIndicator(high)

	
	private drawHigh = true

	override def OnIntervalClose() as bool:
		//			if( Bars.Time[0].TimeOfDay >= StartMorning && Bars.Time[1].TimeOfDay < StartMorning ) {
		//				Reset();
		//			}
		CheckForConfirmedPivot()
		FindNewPivots()
		return true
		//			if( tick.Time.TimeOfDay < StartMorning) {
		//				FindNewPivots();
		//			} else if( tick.Time.TimeOfDay > EndMorning) {
		//				Reset();
		//			} else {
		//				CheckForConfirmedPivot();
		//				FindNewPivots();
		//			}

	
	def FindNewPivots():
		if ((drawHigh and (high.PivotBars.Count > 0)) and (high.PivotBars[0] > lastLowBar)) and (high.PivotHighs[0] > lastHighPrice):
			lastHighBar = high.PivotBars[0]
			lastHighPrice = high.PivotHighs[0]
		if (((not drawHigh) and (low.PivotBars.Count > 0)) and (low.PivotBars[0] > lastHighBar)) and (low.PivotLows[0] < lastLowPrice):
			lastLowBar = low.PivotBars[0]
			lastLowPrice = low.PivotLows[0]

	
	def CheckForConfirmedPivot():
		if drawHigh and (Bars.Low[0] <= (lastHighPrice - threshold)):
			if (zagBars.Count == 0) or (lastHighBar > zagBars[0]):
				zigBars.Add(lastHighBar)
				zigHighs.Add(lastHighPrice)
				drawHigh = false
				if zagBars.Count > 0:
					Chart.DrawLine(Drawing.Color, zagBars[0], lastLowPrice, zigBars[0], lastHighPrice, LineStyle.Solid)
					//						DrawDownTrend();
					//						DrawHorizontal(lastHighPrice-100);
					LogNewHigh()
					lastLowPrice = int.MaxValue
		elif (not drawHigh) and (Bars.High[0] >= (lastLowPrice + threshold)):
			if (zigBars.Count == 0) or (lastLowBar > zigBars[0]):
				zagBars.Add(lastLowBar)
				zagLows.Add(lastLowPrice)
				drawHigh = true
				if zigBars.Count > 0:
					Chart.DrawLine(Drawing.Color, zagBars[0], lastLowPrice, zigBars[0], lastHighPrice, LineStyle.Solid)
					//						DrawUpTrend();
					//						DrawHorizontal(lastLowPrice+100);
					LogNewLow()
					lastHighPrice = 0

	
	def DrawHorizontal(price as int):
		Chart.DrawLine(Color.Purple, Bars.CurrentBar, price, (Bars.CurrentBar + 5), price, LineStyle.Dashed)

	
	def DrawUpTrend():
		if zagBars.Count > 1:
			currBar as int = (Bars.CurrentBar - zagBars[0])
			prevBar as int = (Bars.CurrentBar - zagBars[1])
			length as int = (zagBars[0] - zagBars[1])
			currPrice as double = Bars.Low[currBar]
			prevPrice as double = Bars.Low[prevBar]
			height as double = (prevPrice - currPrice)
			if currPrice >= prevPrice:
				nextBar as int = (zagBars[0] + (length * 2))
				nextPrice as double = (currPrice - (height * 2))
				Chart.DrawLine(Color.Blue, zagBars[0], currPrice, nextBar, nextPrice, LineStyle.Dashed)

	
	def DrawDownTrend():
		if zigBars.Count > 1:
			currBar as int = (Bars.CurrentBar - zigBars[0])
			prevBar as int = (Bars.CurrentBar - zigBars[1])
			length as int = (zigBars[0] - zigBars[1])
			currHigh as double = Bars.High[currBar]
			prevHigh as double = Bars.High[prevBar]
			height as double = (currHigh - prevHigh)
			if currHigh <= prevHigh:
				nextBar as int = (zigBars[0] + (length * 2))
				nextHigh as double = (currHigh + (height * 2))
				Chart.DrawLine(Color.Magenta, zigBars[0], currHigh, nextBar, nextHigh, LineStyle.Dashed)

	
	def LogNewHigh():
		if (debug and (Ticks[0].Time.TimeOfDay >= cast(double,StartMorning))) and (Ticks[0].Time.TimeOfDay <= cast(double,EndMorning)):
			prevBar as int = (Bars.CurrentBar - zagBars[0])
			currBar as int = (Bars.CurrentBar - lastHighBar)
			traversal = 'None'
			if zagBars.Count > 1:
				trendBar as int = (Bars.CurrentBar - zagBars[1])
				traversal = ('Dom' if (Bars.Low[prevBar] > Bars.Low[trendBar]) else 'Sub')
			Log.Notice(((((((((((((('High,' + traversal) + ',') + Bars.Time[currBar]) + ',') + Bars.Low[currBar]) + ',') + SimTradeProfit()) + ',') + (simTradeTotal - (simTradeCount * costs))) + ', ') + SimTradePreviousTimeStr(prevBar, currBar)) + ',') + SimTradeAverageTimeStr()))
			// If we got an above average traversal, start the timer over.
			//				if( IsPastAverageTime ) {
			simTradeLast = Bars.Time[currBar]
			//				}

	
	def LogNewLow():
		if (debug and (Ticks[0].Time.TimeOfDay >= cast(double,StartMorning))) and (Ticks[0].Time.TimeOfDay <= cast(double,EndMorning)):
			prevBar as int = (Bars.CurrentBar - zigBars[0])
			currBar as int = (Bars.CurrentBar - lastLowBar)
			traversal = 'None'
			if zigBars.Count > 1:
				trendBar as int = (Bars.CurrentBar - zigBars[1])
				traversal = ('Dom' if (Bars.High[prevBar] < Bars.High[trendBar]) else 'Sub')
			Log.Notice(((((((((((((('Low,' + traversal) + ',') + Bars.Time[currBar]) + ',') + Bars.Low[currBar]) + ',') + SimTradeProfit()) + ',') + (simTradeTotal - (simTradeCount * costs))) + ',') + SimTradePreviousTimeStr(prevBar, currBar)) + ',') + SimTradeAverageTimeStr()))
			// If we got an above average traversal, start the timer over.
			//				if( IsPastAverageTime ) {
			simTradeLast = Bars.Time[currBar]
			//				}

	
	
	
	def SimTradeProfit() as double:
		simProfit as double = 0
		if simTradeCount > 0:
			highBar as int = (Bars.CurrentBar - zigBars[0])
			lowBar as int = (Bars.CurrentBar - zagBars[0])
			simProfit = (Bars.High[highBar] - Bars.Low[lowBar])
			simTradeTotal += simProfit
		else:
			simTradeStart = Ticks[0].Time
		simTradeCount += 1
		return simProfit

	
	Pace as int:
		get:
			return cast(int, (((Ticks[0].Bid - Bars.Low[(Bars.CurrentBar - zagBars[0])]) if IsTraverseLong else (Bars.High[(Bars.CurrentBar - zigBars[0])] - Ticks[0].Ask)) / SimTradeCurrentTime.TotalMinutes))

	
	SimTradeAverageTime as Elapsed:
		get:
			if simTradeCount > 0:
				elapsed as Elapsed = (Ticks[0].Time - simTradeStart)
				avg as double = (((cast(double,elapsed) / simTradeCount) * 100) / 90)
				return Elapsed(avg)
			else:
				return Elapsed()

	
	IsDownTrend as bool:
		get:
			if (zigBars.Count < 2) or (zagBars.Count < 1):
				return false
			if (zigHighs[0] <= zigHighs[1]) and (Bars.Close[0] < zigHighs[0]):
				return true
			return false

	
	IsUpTrend as bool:
		get:
			if (zagBars.Count < 2) or (zigBars.Count < 1):
				return false
			if (zagLows[0] >= zagLows[1]) and (Bars.Close[0] > zagLows[0]):
				return true
			return false

	
	IsTraverseLong as bool:
		get:
			return (((zigBars.Count > 0) and (zagBars.Count > 0)) and (zagBars[0] > zigBars[0]))

	
	IsTraverseShort as bool:
		get:
			return (((zigBars.Count > 0) and (zagBars.Count > 0)) and (zigBars[0] > zagBars[0]))

	
	IsPastAverageTime as bool:
		get:
			return (cast(double,SimTradeCurrentTime) > cast(double,SimTradeAverageTime))

	
	def SimTradePreviousTime(startBar as int, endBar as int) as Elapsed:
		return (Bars.Time[endBar] - Bars.Time[startBar])

	
	SimTradeCurrentTime as Elapsed:
		get:
			return (Ticks[0].Time - simTradeLast)

	
	def SimTradePreviousTimeStr(startBar as int, endBar as int) as string:
		elapsed as Elapsed = SimTradePreviousTime(startBar, endBar)
		return String.Format('00:{0:00}:{0:00}', elapsed.TotalMinutes, elapsed.TotalSeconds)

	
	
	def SimTradeAverageTimeStr() as string:
		elapsed as Elapsed = SimTradeAverageTime
		return String.Format('00:{0:00}:{0:00}', elapsed.TotalMinutes, elapsed.TotalSeconds)

	
	def SimTradeCurrentTimeStr() as string:
		elapsed as Elapsed = SimTradeCurrentTime
		return String.Format('00:{0:00}:{0:00}', elapsed.TotalMinutes, elapsed.TotalSeconds)

	
	StartMorning as Elapsed:
		get:
			return startMorning
		set:
			startMorning = value

	
	EndMorning as Elapsed:
		get:
			return endMorning
		set:
			endMorning = value

