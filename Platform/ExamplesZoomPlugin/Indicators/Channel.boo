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

class Channel:

	private log as Log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

	private bars as Bars

	private interceptBar = 0

	private x1 as int

	private y1 as int

	private x2 as int

	private y2 as int

	private lr = LinearRegression()

	private lr2 = LinearRegression()

	private upperLineId = 0

	// for charts
	private lowerLineId = 0

	// for charts
	private extraLineId = 0

	// for reversal point, intrabar.
	private highestDev as double = 0

	private lowestDev as double = 0

	private highestX = 0

	private lowestX = 0

	private highestHigh as double = double.MinValue

	private lowestLow as double = double.MaxValue

	private fttHighestHigh as double = double.MinValue

	private fttLowestLow as double = double.MaxValue

	private longColor as Color = Color.LightGreen

	private shortColor as Color = Color.Magenta

	private isCalculated = false

	private direction as Trend = Trend.None

	
	private isDirectionSet = false

	private traverseDirection as Trend = Trend.Flat

	private highMaxVolume as Doubles = Factory.Engine.Doubles()

	private lowMaxVolume as Doubles = Factory.Engine.Doubles()

	
	def constructor(bars as Bars):
		self.bars = bars

	
	def ResetMaxVolume():
		highMaxVolume = Factory.Engine.Doubles()
		lowMaxVolume = Factory.Engine.Doubles()
		UpdateTick()

	
	def GetYFromBar(bar as int) as int:
		return GetY((bar - interceptBar))

	
	def addPoint(bar as int, y as double):
		lr.addPoint((bar - interceptBar), y)

	
	def addPoint2(bar as int, y as double):
		lr2.addPoint((bar - interceptBar), y)

	
	def removePointsBefore(bar as int):
		lr.removePointsBefore(bar)

	
	Correlation as double:
		get:
			return lr.Correlation

	
	def Calculate():
		isCalculated = true
		lr.calculate()
		if not Calculate2():
			pass
			//				double mult = (1 - (1 / (double) lr.Coord.Count));
			//				lr.Slope = lr.Slope * mult;
		if not isDirectionSet:
			direction = (Trend.Up if (lr.Slope > 0) else (Trend.Down if (lr.Slope < 0) else Trend.Flat))
			isDirectionSet = true

	
	private def Calculate2() as bool:
		if lr2.Coord.Count > 1:
			lr2.calculate()
			newSlope as double = lr2.Slope
			// (lr.Slope + lr2.Slope)/2;
			if IsUp and (newSlope > lr.Slope):
				lr.Slope = newSlope
				return true
			if IsDown and (newSlope < lr.Slope):
				lr.Slope = newSlope
				return true
		return false

	
	def CheckNewSlope(point as ChartPoint) as double:
		tempLR = LinearRegression(lr.Coord)
		tempLR.addPoint((point.Bar - interceptBar), point.Y)
		tempLR.calculate()
		mult as double = (1 - (1 / cast(double, tempLR.Coord.Count)))
		tempLR.Slope = (tempLR.Slope * mult)
		return tempLR.Slope

	
	def UpdateEnds():
		if not isCalculated:
			raise ApplicationException('Linear Regression was never calculated.')
		x1 = int.MinValue
		for i in range(0, lr.Coord.Count):
			if lr.Coord[i].X > x1:
				x1 = cast(int, lr.Coord[i].X)
		y1 = GetY(x1)
		x2 = x1
		for i in range(0, lr.Coord.Count):
			if lr.Coord[i].X < x2:
				x2 = cast(int, lr.Coord[i].X)
		y2 = GetY(x2)

	
	def GetY(x as int) as int:
		return cast(int, (lr.Intercept + (x * lr.Slope)))

	
	def calcHighest(bars as Bars):
		// Find max deviation from the line.
		highestHigh = int.MinValue
		lowestLow = int.MaxValue
		x as int = x2
		goto converterGeneratedName1
		while true:
			x += 1
			:converterGeneratedName1
			break  unless ((x <= x1) and ((x1 - x) < bars.Count))
			highY = bars.High[(x1 - x)]
			lowY = bars.Low[(x1 - x)]
			if (x1 - x) < 4:
				if highY > highestHigh:
					highestHigh = highY
					highestX = x
				if lowY < lowestLow:
					lowestLow = lowY
					lowestX = x

	
	def calcMaxDev(bars as Bars):
		calcHighest(bars)
		calcDeviation(bars)

	
	def calcDeviation(bars as Bars):
		// Find max deviation from the line.
		highestDev = 0
		lowestDev = 0
		x as int = x2
		goto converterGeneratedName2
		while true:
			x += 1
			:converterGeneratedName2
			break  unless ((x <= x1) and ((x1 - x) < bars.Count))
			lineY as int = GetY(x)
			highY as double = bars.High[(x1 - x)]
			lowY as double = bars.Low[(x1 - x)]
			highDev as double = (highY - lineY)
			lowDev as double = (lineY - lowY)
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

	
	def DrawChannel(chart as Chart, extend as int, drawDashedLines as bool):
		extendBar as int = ((x1 + interceptBar) + extend)
		extendY as int = GetYFromBar(extendBar)
		topColor as Color
		botColor as Color
		topColor = (botColor = (longColor if (direction == Trend.Up) else shortColor))
		if IsUp:
			upperLineId = chart.DrawLine(topColor, extendBar, (extendY + highestDev), (x2 + interceptBar), (y2 + highestDev), LineStyle.Solid)
			if drawDashedLines:
				lowerLineId = chart.DrawLine(botColor, extendBar, extendY, (x2 + interceptBar), y2, LineStyle.Dashed)
			extraLineId = chart.DrawLine(botColor, extendBar, (extendY - lowestDev), (x2 + interceptBar), (y2 - lowestDev), LineStyle.Solid)
		else:
			extraLineId = chart.DrawLine(topColor, extendBar, (extendY + highestDev), (x2 + interceptBar), (y2 + highestDev), LineStyle.Solid)
			if drawDashedLines:
				upperLineId = chart.DrawLine(topColor, extendBar, extendY, (x2 + interceptBar), y2, LineStyle.Dashed)
			lowerLineId = chart.DrawLine(botColor, extendBar, (extendY - lowestDev), (x2 + interceptBar), (y2 - lowestDev), LineStyle.Solid)

	
	def RedrawChannel(chart as Chart, extend as int, drawDashedLines as bool):
		extendBar as int = ((x1 + interceptBar) + extend)
		extendY as int = GetYFromBar(extendBar)
		topColor as Color
		botColor as Color
		topColor = (botColor = (longColor if (direction == Trend.Up) else shortColor))
		if IsUp:
			chart.ChangeLine(upperLineId, topColor, extendBar, (extendY + highestDev), (x2 + interceptBar), (y2 + highestDev), LineStyle.Solid)
			if drawDashedLines:
				chart.ChangeLine(lowerLineId, botColor, extendBar, extendY, (x2 + interceptBar), y2, LineStyle.Dashed)
			chart.ChangeLine(extraLineId, botColor, extendBar, (extendY - lowestDev), (x2 + interceptBar), (y2 - lowestDev), LineStyle.Solid)
		else:
			chart.ChangeLine(extraLineId, topColor, extendBar, (extendY + highestDev), (x2 + interceptBar), (y2 + highestDev), LineStyle.Solid)
			if drawDashedLines:
				chart.ChangeLine(upperLineId, topColor, extendBar, extendY, (x2 + interceptBar), y2, LineStyle.Dashed)
			chart.ChangeLine(lowerLineId, botColor, extendBar, (extendY - lowestDev), (x2 + interceptBar), (y2 - lowestDev), LineStyle.Solid)

	
	Slope as double:
		get:
			return lr.Slope
		set:
			lr.Slope = value

	
	Slope2 as double:
		get:
			return lr2.Slope
		set:
			lr2.Slope = value

	
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
			return (x1 + interceptBar)

	
	Y1 as int:
		get:
			return y1

	
	Bar2 as int:
		get:
			return (x2 + interceptBar)

	
	Y2 as int:
		get:
			return y2

	
	BarsInChannel as int:
		get:
			return CountPoints

	
	Middle as double:
		get:
			return ((Top + Bottom) / 2)

	
	UpperMiddle as double:
		get:
			return (Top - Math.Min((Width / 5), 50))

	
	LowerMiddle as double:
		get:
			return (Bottom + Math.Min((Width / 5), 50))

	
	Top as double:
		get:
			line as double = CurrentLine
			return (line + (highestDev if IsDown else highestDev))

	
	Width as double:
		get:
			return (Top - Bottom)

	
	protected CurrentLine as int:
		get:
			return cast(int, GetYFromBar(bars.CurrentBar))

	
	Bottom as double:
		get:
			line as double = CurrentLine
			return (line - (lowestDev if IsUp else lowestDev))

	
	Length as int:
		get:
			return (x1 - x2)

	
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

	
	LowerLineId as int:
		get:
			return lowerLineId
		set:
			lowerLineId = value

	
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

	
	LastHighBar as int:
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
		elif tick.Ask > UpperMiddle:
			status = ChannelStatus.Upper
		elif tick.Bid < (Bottom + 20):
			status = ChannelStatus.Below
		elif tick.Bid < LowerMiddle:
			status = ChannelStatus.Lower
		else:
			status = ChannelStatus.Inside
		converterGeneratedName3 = status
		if (converterGeneratedName3 == ChannelStatus.Above) or (converterGeneratedName3 == ChannelStatus.Upper):
			traverseDirection = Trend.Down
		elif (converterGeneratedName3 == ChannelStatus.Below) or (converterGeneratedName3 == ChannelStatus.Lower):
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
	

