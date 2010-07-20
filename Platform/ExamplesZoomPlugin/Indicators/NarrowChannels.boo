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


class NarrowChannels(IndicatorCommon):

	private strategy as StrategyCommon

	private tapes as Series[of NarrowChannel]

	private channels as Series[of WideChannel]

	
	private reDrawLines = true

	private drawDashedLines = false

	private drawSolidLines = true

	private drawLines = true

	private highs as Series[of ChartPoint]

	private lows as Series[of ChartPoint]

	private extend = 10

	private longColor as Color = Color.Green

	private shortColor as Color = Color.Red

	//		bool hasNewChannel;
	private isActivated = true

	
	//		Trend trend = Trend.Flat;
	def constructor():
		tapes = Series[of NarrowChannel]()
		channels = Series[of WideChannel]()
		highs = Series[of ChartPoint]()
		lows = Series[of ChartPoint]()

	
	def constructor(strategy as StrategyCommon):
		self.strategy = strategy

	
	override def OnInitialize():
		Extend = 2
		LongColor = Color.LightGreen
		ShortColor = Color.Pink

	
	def Reset():
		tapes.Clear()

	
	override def OnIntervalClose() as bool:
		
		if tapes.Count == 0:
			TryNewTape(Bars.CurrentBar, Trend.Flat)
			return true
		
		ContinueOrEnd()
		
		if tapes.Count > 1:
			line as NarrowChannel = tapes[1]
			if line.IsUp and (line.Direction != tapes[0].Direction):
				lastHighBar as int = line.LastHighBar
				lastHigh as double = Bars.High[(Bars.CurrentBar - lastHighBar)]
				if (lows.Count == 0) or ((lastHighBar - lows[0].X) > 1):
					if (highs.Count == 0) or (lastHighBar != highs[0].X):
						NewHigh(lastHighBar, lastHigh)
			elif line.IsDown and (line.Direction != tapes[0].Direction):
			
				lastLowBar as int = line.LastLowBar
				lastLow as double = Bars.Low[(Bars.CurrentBar - lastLowBar)]
				
				if (highs.Count == 0) or ((lastLowBar - highs[0].X) > 1):
					if (lows.Count == 0) or (lastLowBar != lows[0].X):
						NewLow(lastLowBar, lastLow)
		return true

	
	def FindPoint(points as Series[of ChartPoint], bar as double) as ChartPoint:
		for i in range(0, points.Count):
			if points[i].X < bar:
				return points[i]
		raise ApplicationException('No Point found.')

	
	def NewHigh(bar as int, price as double):
		// If no low since last high and this high is higher,
		// the replace the last high point, else add it.
		if ((highs.Count > 0) and (lows.Count > 0)) and (lows[0].X < highs[0].X):
			if price > highs[0].Y:
				// Make the replaced one Red.
				Chart.DrawBox(Color.Red, cast(int, highs[0].X), highs[0].Y)
				highs[0] = ChartPoint(bar, price)
			else:
				// Skip a lower high without a low in between.
				return 
		else:
			highs.Add(ChartPoint(bar, price))
		Chart.DrawBox(Color.Blue, bar, price)
		
		if (highs.Count > 1) and (lows.Count > 1):
			try:
				point1 as ChartPoint = highs[0]
				point2 as ChartPoint = FindPoint(lows, point1.X)
				point3 as ChartPoint = FindPoint(highs, point2.X)
				if point1.Y < point3.Y:
					wchannel = WideChannel(Trend.Down, Bars)
					wchannel.Chart = Chart
					wchannel.addHigh(point1.X)
					wchannel.addLow(point2.X)
					wchannel.addHigh(point3.X)
					wchannel.TryDraw()
					channels.Add(wchannel)
					//					Chart.DrawLine(Color.Red,highs[1].X,highs[1].Y,highs[0].X,highs[0].Y,LineStyle.Solid);
			except converterGeneratedName1 as ApplicationException:
			// Return if FindPoint failed.
				return 

	
	def NewLow(bar as int, price as double):
		// If no low since last high and this high is higher,
		// the replace the last high point, else add it.
		if ((highs.Count > 0) and (lows.Count > 0)) and (highs[0].X < lows[0].X):
			if price < lows[0].Y:
				Chart.DrawBox(Color.Red, cast(int, lows[0].X), lows[0].Y)
				lows[0] = ChartPoint(bar, price)
			else:
				// Skip a high low with out a high in between.
				return 
		else:
			lows.Add(ChartPoint(bar, price))
		
		Chart.DrawBox(Color.Blue, bar, price)
		if lows.Count > 1:
			try:
				point1 as ChartPoint = lows[0]
				point2 as ChartPoint = FindPoint(highs, point1.X)
				point3 as ChartPoint = FindPoint(lows, point2.X)
				if point1.Y > point3.Y:
					wchannel = WideChannel(Trend.Up, Bars)
					wchannel.Chart = Chart
					wchannel.addLow(point1.X)
					wchannel.addHigh(point2.X)
					wchannel.addLow(point3.X)
					wchannel.TryDraw()
					channels.Add(wchannel)
					//					Chart.DrawLine(Color.Red,lows[1].X,lows[1].Y,lows[0].X,lows[0].Y,LineStyle.Solid);
			except converterGeneratedName2 as ApplicationException:
			// Return if FindPoint failed.
				return 

	
	virtual def ContinueOrEnd():
		channel as NarrowChannel = Tapes[0]
		top as int = channel.High
		bottom as int = channel.Low
		close as double = Bars.Close[0]
		isSuccess = false
		if Tapes[0].IsDown:
			if close > top:
				isSuccess = TryNewTape(Tapes[0].LastLowBar, Trend.Up)
			elif close < bottom:
				isSuccess = TryNewTape(Tapes[0].LastHighBar, Trend.Down)
		elif Tapes[0].IsUp:
			if close < bottom:
				isSuccess = TryNewTape(Tapes[0].LastHighBar, Trend.Down)
			elif close > top:
				isSuccess = TryNewTape(Tapes[0].LastLowBar, Trend.Up)
		elif close < bottom:
			isSuccess = TryNewTape(Tapes[0].LastHighBar, Trend.Down)
		elif close > top:
			isSuccess = TryNewTape(Tapes[0].LastLowBar, Trend.Up)
		if not isSuccess:
			channel.TryExtend()
			return 

	
	protected def TryNewTape(lastLineBar as int, trend as Trend) as bool:
		channel = NarrowChannel(Bars)
		channel.BreakoutBar = Bars.CurrentBar
		channel.LongColor = LongColor
		channel.ShortColor = ShortColor
		channel.Chart = Chart
		channel.DrawLines = drawLines
		channel.ReDrawLines = RedrawLines
		channel.DrawSolidLines = drawSolidLines
		channel.DrawDashedLines = drawDashedLines
		channel.Extend = extend
		if channel.TryInitialize(lastLineBar, trend):
			tapes.Add(channel)
			//				hasNewChannel = true;
			//				if( tapes.Count < 2 || channel.Direction != tapes[1].Direction) {
			//					strategy.FireEvent(new NewChannel().SetDirection(channel.Direction).ExpirySeconds(30));
			//				}
			return true
		return false

	
	
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

	
	Tapes as Series[of NarrowChannel]:
		get:
			return tapes

	
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

	
	//		bool HasNewChannel {
	//			get { return hasNewChannel; }
	//		}
	//		
	IsActivated as bool:
		get:
			return isActivated
		set:
			isActivated = value

	
	Channels as Series[of WideChannel]:
		get:
			return channels

