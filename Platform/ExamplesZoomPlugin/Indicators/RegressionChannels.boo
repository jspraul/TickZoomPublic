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


class RegressionChannels(IndicatorCommon):

	private lines as Series[of Channel]
	private slope as IndicatorCommon
	private reDrawLines = false
	private drawDashedLines = true

	private drawLines = true

	private highs as Series[of ChartPoint]

	private lows as Series[of ChartPoint]

	private extend = 10

	private longColor as Color = Color.Green

	private shortColor as Color = Color.Red

	private hasNewChannel as bool

	private isActivated = true

	
	//		Trend trend = Trend.Flat;
	
	def constructor():
		lines = Series[of Channel]()
		highs = Series[of ChartPoint]()
		lows = Series[of ChartPoint]()
	
	override def OnInitialize():
		slope = IndicatorCommon()
		slope.Drawing.GroupName = 'Slope'
		AddIndicator(slope)

	
	def Reset():
		lines.Clear()

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		//			if( !isActivated ) { return; }
		
		if timeFrame.Equals(IntervalDefault):
			hasNewChannel = false
			
			
			if lines.Count == 0:
				NewChannel(Bars.CurrentBar, Trend.Flat)
				return true
			
			ContinueOrEnd()
			//				if( lines[0].IsUp && direction != Trend.Up) {
			//					NewChannel(lines[0].LastLowBar,Trend.Up);					
			//				}
			//				if( lines[0].IsDown && direction != Trend.Down) {
			//					NewChannel(lines[0].LastHighBar,Trend.Down);					
			//				}
			
			if lines.Count > 1:
				line as Channel = lines[1]
				if line.IsUp:
					lastHighBar as int = line.LastHighBar
					NewHigh(lastHighBar, Bars.High[(Bars.CurrentBar - lastHighBar)])
				if line.IsDown:
					lastLowBar as int = line.LastLowBar
					NewLow(lastLowBar, Bars.Low[(Bars.CurrentBar - lastLowBar)])
				slope[0] = lines[0].Slope
		return true

	
	virtual def ContinueOrEnd():
		if lines[0].IsDown:
			if Bars.Close[0] > lines[0].Top:
				NewChannel(lines[0].LastLowBar, Trend.Flat)
				return 
			if Bars.Volume[0] > 266000:
				NewChannel(lines[0].LastLowBar, Trend.Flat)
				return 
		if lines[0].IsUp:
			if Bars.Close[0] < lines[0].Bottom:
				NewChannel(lines[0].LastHighBar, Trend.Flat)
				return 
			if Bars.Volume[0] > 266000:
				NewChannel(lines[0].LastHighBar, Trend.Flat)
				return 
		TryExtendChannel()
		return 

	
	def NewHigh(bar as int, price as double):
		if (highs.Count > 0) and (bar == highs[0].X):
			return 
		//			Chart.DrawBox(Color.Blue,bar,price);
		highs.Add(ChartPoint(bar, price))
		//			if( false && highs.Count > 1) {
		//				Line line = new Line(Bars.CurrentBar);
		//				line.setPoints(highs[0],highs[1]);
		//				line.calculate();
		//				line.extend( 20);
		//				Chart.DrawLine(Color.Blue,highs[0].X,highs[0].Y,line.Bar1,line.Y1,LineStyle.Solid);
		//			}

	
	def NewLow(bar as int, price as double):
		if (lows.Count > 0) and (bar == lows[0].X):
			return 
		//			Chart.DrawBox(Color.Blue,bar,price);
		lows.Add(ChartPoint(bar, price))
		//			if( false && lows.Count > 1) {
		//				Line line = new Line(Bars.CurrentBar);
		//				line.setPoints(lows[0],lows[1]);
		//				line.calculate();
		//				line.extend( 20);
		//				Chart.DrawLine(Color.Red,lows[0].X,lows[0].Y,line.Bar1,line.Y1,LineStyle.Solid);
		//			}

	
	protected def NewChannel(lastLineBar as int, trend as Trend) as bool:
		channel = Channel(Bars)
		channel.LongColor = LongColor
		channel.ShortColor = ShortColor
		bar as int = Bars.CurrentBar
		lookBack = 1
		channel.InterceptBar = lastLineBar
		goto converterGeneratedName1
		
		while true:
			bar -= 1
			:converterGeneratedName1
			break  unless ((bar > (Bars.CurrentBar - Bars.Capacity)) and (bar >= lastLineBar))
			converterGeneratedName2 = trend
			if converterGeneratedName2 == Trend.Flat:
				channel.addPoint(bar, Formula.Middle(Bars, (Bars.CurrentBar - bar)))
			elif converterGeneratedName2 == Trend.Up:
				channel.addPoint(bar, Bars.Low[(Bars.CurrentBar - bar)])
				channel.addPoint2(bar, Bars.High[(Bars.CurrentBar - bar)])
			elif converterGeneratedName2 == Trend.Down:
				channel.addPoint(bar, Bars.High[(Bars.CurrentBar - bar)])
				channel.addPoint2(bar, Bars.Low[(Bars.CurrentBar - bar)])
		if channel.CountPoints > lookBack:
			channel.Calculate()
			channel.UpdateEnds()
			// If new channel isn't in the direction expected
			// then forget it.
			if (trend == Trend.Up) and (not channel.IsUp):
				return false
			elif (trend == Trend.Down) and (not channel.IsDown):
				return false
			channel.calcMaxDev(Bars)
			if drawLines:
				channel.DrawChannel(Chart, extend, drawDashedLines)
			lines.Add(channel)
			hasNewChannel = true
			return true
		return false

	
	protected def TryExtendChannel() as bool:
		recalculate = true
		if lines.Count == 0:
			return false
		lr as Channel = lines[0]
		if lr.CountPoints > 100:
			return false
		bar as int = Bars.CurrentBar
		
		newPoint as ChartPoint
		converterGeneratedName3 = lr.Direction
		if converterGeneratedName3 == Trend.Flat:
			newPoint = ChartPoint(bar, Formula.Middle(Bars, (Bars.CurrentBar - bar)))
		elif converterGeneratedName3 == Trend.Up:
			newPoint = ChartPoint(bar, Bars.Low[(Bars.CurrentBar - bar)])
		elif converterGeneratedName3 == Trend.Down:
			newPoint = ChartPoint(bar, Bars.High[(Bars.CurrentBar - bar)])
		else:
			newPoint = ChartPoint(bar, Formula.Middle(Bars, (Bars.CurrentBar - bar)))
		newSlope as double = lr.CheckNewSlope(newPoint)
		converterGeneratedName4 = lr.Direction
		if converterGeneratedName4 == Trend.Flat:
			if newSlope != 0:
				recalculate = false
		elif converterGeneratedName4 == Trend.Up:
			//					if( newSlope < lr.Slope) recalculate = false;
			if newSlope <= 0:
				recalculate = false
		elif converterGeneratedName4 == Trend.Down:
			//					if( newSlope > lr.Slope) recalculate = false;
			if newSlope >= 0:
				recalculate = false
		elif newSlope != 0:
			recalculate = false
		lr.addPoint(newPoint.Bar, cast(int, newPoint.Y))
		try:
			if recalculate:
				lr.Calculate()
			lr.UpdateEnds()
			// Was before calcMaxDev
			lr.calcHighest(Bars)
			if recalculate:
				lr.calcMaxDev(Bars)
			if drawLines:
				if reDrawLines:
					lr.RedrawChannel(Chart, extend, drawDashedLines)
				else:
					lr.DrawChannel(Chart, extend, drawDashedLines)
		except ex as ApplicationException:
			Log.Notice(ex.ToString())
			return false
		return true

	
	protected def TryShortenChannel(lastLineBar as int) as bool:
		if lines.Count == 0:
			return false
		lr as Channel = lines[0]
		lr.removePointsBefore(lastLineBar)
		
		try:
			lr.Calculate()
			lr.UpdateEnds()
			lr.calcMaxDev(Bars)
			if drawLines:
				lr.DrawChannel(Chart, extend, drawDashedLines)
		except ex as ApplicationException:
			Log.Notice(ex.ToString())
			return false
		return true

	
	RedrawLines as bool:
		get:
			return reDrawLines
		set:
			reDrawLines = value

	
	DrawLines as bool:
		get:
			return drawLines
		set:
			drawLines = value

	
	Lines as Series[of Channel]:
		get:
			return lines

	
	Extend as int:
		get:
			return extend
		set:
			extend = value

	
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

	
	HasNewChannel as bool:
		get:
			return hasNewChannel

	
	IsActivated as bool:
		get:
			return isActivated
		set:
			isActivated = value

