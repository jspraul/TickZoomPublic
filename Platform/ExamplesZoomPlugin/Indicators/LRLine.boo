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

internal enum BarsToUse:

	Highs

	Lows

	Middle

class LRLine:

	private log as Log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

	private bars as Bars

	private interceptBar = 0

	private breakoutBar = 0

	private firstBar as int = int.MinValue

	private lastBar as int

	private lr = LinearRegression()

	private upperLineId as int = (-1)

	// for charts
	private lowerLineId as int = (-1)

	// for charts
	private lineId as int = (-1)

	// for charts
	private highestDev as double = 0

	private lowestDev as double = 0

	private highestX = 0

	private lowestX = 0

	private highestHigh as double = double.MinValue

	private lowestLow as double = double.MaxValue

	private fttHighestHigh as double = double.MinValue

	private fttLowestLow as double = double.MaxValue

	private longColor as Color = Color.Green

	private shortColor as Color = Color.Magenta

	private isCalculated = false

	private direction as Trend = Trend.None

	private drawLines = true

	private reDrawLines = true

	private chart as Chart = null

	private barsToUse as BarsToUse = BarsToUse.Middle

	private maxSlope as double = double.MaxValue

	private minSlope as double = double.MinValue

	
	private traverseDirection as Trend = Trend.Flat

	private highMaxVolume as Doubles = Factory.Engine.Doubles()

	private lowMaxVolume as Doubles = Factory.Engine.Doubles()

	private model as ModelCommon

	
	private length = 0

	
	def constructor(model as ModelCommon, bars as Bars, chart as Chart, length as int):
		self.model = model
		self.bars = bars
		self.length = length
		self.DrawSolidLines = true
		self.DrawDashedLines = true
		self.chart = chart

	
	def UseHighs():
		barsToUse = BarsToUse.Highs

	
	def UseLows():
		barsToUse = BarsToUse.Lows

	
	def GetYFromBar(bar as int) as int:
		return GetY((bar - interceptBar))

	
	def AddBar(bar as int):
		converterGeneratedName1 = barsToUse
		if converterGeneratedName1 == BarsToUse.Highs:
			lr.addPoint((bar - interceptBar), bars.High[(bars.CurrentBar - bar)])
		elif converterGeneratedName1 == BarsToUse.Lows:
			lr.addPoint((bar - interceptBar), bars.Low[(bars.CurrentBar - bar)])
		elif converterGeneratedName1 == BarsToUse.Middle:
			lr.addPoint((bar - interceptBar), model.Formula.Middle(bars, (bars.CurrentBar - bar)))
		else:
			lr.addPoint((bar - interceptBar), model.Formula.Middle(bars, (bars.CurrentBar - bar)))

	
	def ClearBars():
		lr.clearPoints()

	
	def removePointsBefore(bar as int):
		lr.removePointsBefore(bar)

	
	Correlation as double:
		get:
			return lr.Correlation

	
	def Calculate():
		isCalculated = true
		lr.calculate()
		lr.Slope = Math.Min(maxSlope, lr.Slope)
		lr.Slope = Math.Max(minSlope, lr.Slope)
		direction = (Trend.Up if (lr.Slope > 0) else (Trend.Down if (lr.Slope < 0) else Trend.Flat))

	
	def UpdateEnds():
		if not isCalculated:
			raise ApplicationException('Linear Regression was never calculated.')
		firstBar = int.MinValue
		for i in range(0, lr.Coord.Count):
			if lr.Coord[i].X > firstBar:
				firstBar = cast(int, lr.Coord[i].X)
		lastBar = firstBar
		for i in range(0, lr.Coord.Count):
			if lr.Coord[i].X < lastBar:
				lastBar = cast(int, lr.Coord[i].X)

	
	private extend = 2

	
	private drawSolidLines = true

	
	private drawDashedLines = false

	
	def Update():
		bar as int = bars.CurrentBar
		lookBack = 1
		firstLineBar as int = (bars.CurrentBar - (length - 1))
		InterceptBar = firstLineBar
		
		ClearBars()
		goto converterGeneratedName2
		
		while true:
			bar -= 1
			:converterGeneratedName2
			break  unless ((bar > (bars.CurrentBar - bars.Count)) and (bar >= firstLineBar))
			AddBar(bar)
		if CountPoints > lookBack:
			Calculate()
			UpdateEnds()
			calcMaxDev()
			if drawLines:
				if lineId < 0:
					DrawChannel()
				else:
					RedrawChannel()
		else:
			return 

	
	def GetY(x as int) as int:
		return cast(int, (lr.Intercept + (x * lr.Slope)))

	
	def calcHighest():
		// Find max deviation from the line.
		highestHigh = int.MinValue
		lowestLow = int.MaxValue
		x as int = lastBar
		goto converterGeneratedName3
		while true:
			x += 1
			:converterGeneratedName3
			break  unless ((x <= firstBar) and ((firstBar - x) < bars.Count))
			high as double = bars.High[(firstBar - x)]
			low as double = bars.Low[(firstBar - x)]
			if (firstBar - x) < 4:
				if high > highestHigh:
					highestHigh = high
					highestX = x
				if low < lowestLow:
					lowestLow = low
					lowestX = x

	
	def calcMaxDev():
		calcHighest()
		calcDeviation()

	
	def calcDeviation():
		// Find max deviation from the line.
		highestDev = 0
		lowestDev = 0
		x as int = lastBar
		goto converterGeneratedName4
		while true:
			x += 1
			:converterGeneratedName4
			break  unless ((x <= firstBar) and ((firstBar - x) < bars.Count))
			highY as double = GetY(x)
			high as double = bars.High[(firstBar - x)]
			low as double = bars.Low[(firstBar - x)]
			highDev as double = (high - highY)
			lowDev as double = (highY - low)
			if highDev > highestDev:
				highestDev = highDev
			if lowDev > lowestDev:
				lowestDev = lowDev
		CalcFTT(bars)

	
	private def CalcFTT(bars as Bars):
		if highMaxVolume.Count == 0:
			highMaxVolume.Add(0)
		else:
			highMaxVolume.Add(highMaxVolume[0])
		if lowMaxVolume.Count == 0:
			lowMaxVolume.Add(0)
		else:
			lowMaxVolume.Add(lowMaxVolume[0])
		if (bars.High[0] > fttHighestHigh) and (bars.High[0] > bars.High[1]):
			if bars.Volume[0] > highMaxVolume[0]:
				highMaxVolume[0] = bars.Volume[0]
			fttHighestHigh = bars.High[0]
		if (bars.Low[0] < fttLowestLow) and (bars.Low[0] < bars.Low[1]):
			if bars.Volume[0] > lowMaxVolume[0]:
				lowMaxVolume[0] = bars.Volume[0]
				log.Debug(('Low Max Volume Set to ' + lowMaxVolume[0]))
			fttLowestLow = bars.Low[0]

	
	def DrawChannel():
		extendBar as int = ((firstBar + interceptBar) + extend)
		extendY as int = GetYFromBar(extendBar)
		y2 as int = GetY(lastBar)
		topColor as Color
		botColor as Color
		topColor = (botColor = (longColor if (direction == Trend.Up) else shortColor))
		if drawDashedLines:
			lineId = chart.DrawLine(topColor, extendBar, extendY, (lastBar + interceptBar), y2, LineStyle.Dashed)
		if drawSolidLines:
			upperLineId = chart.DrawLine(topColor, extendBar, (extendY + highestDev), (lastBar + interceptBar), (y2 + highestDev), LineStyle.Solid)
			lowerLineId = chart.DrawLine(topColor, extendBar, (extendY - lowestDev), (lastBar + interceptBar), (y2 - lowestDev), LineStyle.Solid)

	
	def RedrawChannel():
		extendBar as int = ((firstBar + interceptBar) + extend)
		extendY as int = GetYFromBar(extendBar)
		y2 as int = GetY(lastBar)
		topColor as Color
		botColor as Color
		topColor = (botColor = (longColor if (direction == Trend.Up) else shortColor))
		if drawDashedLines:
			chart.ChangeLine(lineId, botColor, extendBar, extendY, (lastBar + interceptBar), y2, LineStyle.Dashed)
		if drawSolidLines:
			chart.ChangeLine(upperLineId, topColor, extendBar, (extendY + highestDev), (lastBar + interceptBar), (y2 + highestDev), LineStyle.Solid)
			chart.ChangeLine(lowerLineId, topColor, extendBar, (extendY - lowestDev), (lastBar + interceptBar), (y2 - lowestDev), LineStyle.Solid)

	
	Slope as double:
		get:
			return lr.Slope
		set:
			lr.Slope = value

	
	// Try down and up true for slope = 0 (horizontal line)
	// so that we get correct top and bottom lines.
	IsDown as bool:
		get:
			return (direction == Trend.Down)

	
	IsUp as bool:
		get:
			return (direction == Trend.Up)

	
	Bar1 as int:
		get:
			return (firstBar + interceptBar)

	
	Bar2 as int:
		get:
			return (lastBar + interceptBar)

	
	BarsInChannel as int:
		get:
			return CountPoints

	
	Middle as double:
		get:
			return ((Top + Bottom) / 2)

	
	Top as double:
		get:
			line as int = High
			return (line + highestDev)

	
	Width as double:
		get:
			return (Top - Bottom)

	
	High as int:
		get:
			return cast(int, GetYFromBar(bars.CurrentBar))

	
	Bottom as double:
		get:
			line as double = High
			return (line - lowestDev)

	
	Length as int:
		get:
			return (firstBar - lastBar)

	
	HighestDev as double:
		get:
			return highestDev

	
	LowestDev as double:
		get:
			return lowestDev

	
	CountPoints as int:
		get:
			return lr.Coord.Count

	
	UpperLineId as int:
		get:
			return upperLineId
		set:
			upperLineId = value

	
	LineId as int:
		get:
			return lineId
		set:
			lineId = value

	
	InterceptBar as int:
		get:
			return interceptBar
		set:
			interceptBar = value

	
	LastLowBar as int:
		get:
			return (lowestX + interceptBar)

	
	LastLowPrice as double:
		get:
			return lowestLow

	
	LastBar as int:
		get:
			return (highestX + interceptBar)

	
	LongColor as Color:
		get:
			return longColor
		set:
			longColor = value

	
	ShortColor as Color:
		get:
			return shortColor
		set:
			shortColor = value

	
	private tick as Tick

	private status as ChannelStatus = ChannelStatus.Inside

	
	Status as ChannelStatus:
		get:
			return status

	
	private def UpdateTick():
		if tick.Ask > (Top - 20):
			status = ChannelStatus.Above
		elif tick.Ask > High:
			status = ChannelStatus.Upper
		elif tick.Bid < (Bottom + 20):
			status = ChannelStatus.Below
		elif tick.Bid < High:
			status = ChannelStatus.Lower
		else:
			status = ChannelStatus.Inside
		converterGeneratedName5 = status
		if (converterGeneratedName5 == ChannelStatus.Above) or (converterGeneratedName5 == ChannelStatus.Upper):
			traverseDirection = Trend.Down
		elif (converterGeneratedName5 == ChannelStatus.Below) or (converterGeneratedName5 == ChannelStatus.Lower):
			traverseDirection = Trend.Up
		isLongFTT = ((((highMaxVolume.Count > 0) and IsUp) and (tick.Ask > fttHighestHigh)) and (bars.Volume[0] < (highMaxVolume[0] - 5000)))
		isShortFTT = ((((lowMaxVolume.Count > 0) and IsDown) and (tick.Bid < fttLowestLow)) and (bars.Volume[0] < (lowMaxVolume[0] - 5000)))
		//			if( TradeSignal.IsShortFTT) {
		//				TickConsole.WriteFile( "TradeSignal.IsShortFTT: isLow="+(lowMaxVolume.Count>0 && IsDown && tick.Bid < fttLowestLow)+", volume="+bars.Volume[0] +", lowMaxVolume="+(lowMaxVolume[0]-5000));
		//			}
		isLongTraverse = ((((highMaxVolume.Count > 0) and IsUp) and (tick.Ask > fttHighestHigh)) and (bars.Volume[0] > highMaxVolume[0]))
		isShortTraverse = ((((lowMaxVolume.Count > 0) and IsDown) and (tick.Bid < fttLowestLow)) and (bars.Volume[0] > lowMaxVolume[0]))

	
	Tick as Tick:
		get:
			return tick
		set:
			tick = value
			UpdateTick()

	
	def LogShortTraverse():
		log.Debug(((((('LogShortTraverse=' + (((lowMaxVolume.Count > 0) and IsDown) and (tick.Bid < fttLowestLow))) + ', volume=') + bars.Volume[0]) + ', low max volume=') + lowMaxVolume[0]))

	
	private isLongFTT = false

	private isShortFTT = false

	private isLongTraverse = false

	private isShortTraverse = false

	
	IsLongTraverse as bool:
		get:
			return isLongTraverse

	
	IsShortTraverse as bool:
		get:
			return isShortTraverse

	
	IsLongFTT as bool:
		get:
			return isLongFTT

	
	IsShortFTT as bool:
		get:
			//				if( TradeSignal.IsShortFTT && lowMaxVolume.Count>0) {
			//					TickConsole.WriteFile( "TradeSignal.IsShortFTT: isLow="+(IsDown && tick.Bid < fttLowestLow)+", volume="+bars.Volume[0] +", lowMaxVolume="+(lowMaxVolume[0]-5000));
			//				}
			return isShortFTT

	
	IsAbove as bool:
		get:
			return (status == ChannelStatus.Above)

	
	IsGoingUp as bool:
		get:
			return (traverseDirection == Trend.Up)

	
	IsGoingDown as bool:
		get:
			return (traverseDirection == Trend.Down)

	
	IsBelow as bool:
		get:
			return (status == ChannelStatus.Below)

	
	IsUpper as bool:
		get:
			return (status == ChannelStatus.Upper)

	
	IsLower as bool:
		get:
			return (status == ChannelStatus.Lower)

	
	IsInside as bool:
		get:
			return (status == ChannelStatus.Inside)

	
	enum ChannelStatus:

		Above

		Upper

		Inside

		Lower

		Below

	
	TraverseDirection as Trend:
		get:
			return traverseDirection

	
	HighMaxVolume as double:
		get:
			return highMaxVolume[0]

	
	LowMaxVolume as double:
		get:
			return lowMaxVolume[0]

	
	IsHalfLowMaxVolume as bool:
		get:
			return (((bars.Volume.Count > 0) and (lowMaxVolume.Count > 0)) and ((bars.Volume[0] * 2) < lowMaxVolume[0]))

	
	IsHalfHighMaxVolume as bool:
		get:
			return (((bars.Volume.Count > 0) and (lowMaxVolume.Count > 0)) and ((bars.Volume[0] * 2) < highMaxVolume[0]))

	
	Direction as Trend:
		get:
			return direction

	
	DrawLines as bool:
		get:
			return drawLines
		set:
			drawLines = value

	
	ReDrawLines as bool:
		get:
			return reDrawLines
		set:
			reDrawLines = value

	
	Extend as int:
		get:
			return extend
		set:
			extend = value

	
	DrawSolidLines as bool:
		get:
			return drawSolidLines
		set:
			drawSolidLines = value

	
	DrawDashedLines as bool:
		get:
			return drawDashedLines
		set:
			drawDashedLines = value

	
	Chart as Chart:
		get:
			return chart
		set:
			chart = value

	
	BreakoutBar as int:
		get:
			return breakoutBar
		set:
			breakoutBar = value

	
	MinSlope as double:
		get:
			return minSlope
		set:
			minSlope = value

	
	MaxSlope as double:
		get:
			return maxSlope
		set:
			maxSlope = value

