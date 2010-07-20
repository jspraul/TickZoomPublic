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
import System.Collections.Generic
import System.Drawing
import TickZoom.Api
import TickZoom.Common

struct CodeStats:

	Count as int

	CountBars as int

	ProfitLoss as double


class SRTraverseMinutes(StrategyCommon):

	private contractSize = 1

	private stretch as IndicatorCommon

	private domRatio as DOMRatio

	private velocity as IndicatorCommon

	private longRetrace as IndicatorCommon

	private shortRetrace as IndicatorCommon

	private trend as IndicatorCommon

	private lrGraph as LRGraph

	
	def constructor():
		ExitStrategy.ControlStrategy = false
		lastCodes = Integers(3)
		lastPrices = Doubles(3)

	
	override def OnInitialize():
		
		velocity = IndicatorCommon()
		velocity.Drawing.PaneType = PaneType.Secondary
		velocity.Drawing.GraphType = GraphType.Histogram
		velocity.Drawing.GroupName = 'Velocity'
		AddIndicator(velocity)
		
		longRetrace = IndicatorCommon()
		longRetrace.Drawing.Color = Color.Blue
		longRetrace.Drawing.GroupName = 'retrace'
		AddIndicator(longRetrace)
		
		lrGraph = LRGraph()
		AddIndicator(lrGraph)
		
		shortRetrace = IndicatorCommon()
		shortRetrace.Drawing.Color = Color.Blue
		shortRetrace.Drawing.GroupName = 'retrace'
		AddIndicator(shortRetrace)
		
		stretch = IndicatorCommon()
		stretch.Drawing.Color = Color.Red
		stretch.Drawing.PaneType = PaneType.Secondary
		stretch.Drawing.GroupName = 'Stretch'
		AddIndicator(stretch)
		
		trend = IndicatorCommon()
		trend.Drawing.Color = Color.Red
		trend.Drawing.GroupName = 'Trend'
		AddIndicator(trend)
		
		domRatio = DOMRatio()
		domRatio.Drawing.Color = Color.Green
		AddIndicator(domRatio)

	
	private positionsize as int

	override def OnProcessTick(tick as Tick) as bool:
		//			ProcessOrders(tick);
		return true

	
	protected def ProcessOrders(tick as Tick):
		if Bars.Count == 1:
			Exit.GoFlat()
			Reset()
			return 
		//			if( tick.AskDepth + tick.BidDepth < 3000) {
		//			 TradeSignal.GoFlat();
		//				Reset();
		//				return;
		//			}			
		
		positionsize = cast(int, Math.Max(0, (Math.Abs(stretch[0]) / 50)))
		
		
		profitTarget as double = (Position.Size * 50)
		current as int = Performance.ComboTrades.Current
		profitInPosition as double = (Performance.ComboTrades.ProfitInPosition(current, Performance.Equity.CurrentEquity) if Position.HasPosition else 0)
		if trendUp:
			if Position.IsShort:
				Exit.GoFlat()
			if (Position.IsLong and (positionsize == 0)) and (profitInPosition > profitTarget):
				if velocity[0] < (-20):
					Exit.GoFlat()
			if (positionsize > Position.Size) and (velocity[0] > 10):
				newPositions = cast(int, (Position.Size + contractSize))
				Enter.BuyMarket(newPositions)
		elif Position.IsLong and (velocity[0] < (-20)):
			Exit.GoFlat()
			//				if( positionsize > TradeSignal.Positions) {
			//					int newPositions = (int) (TradeSignal.Positions+contractSize);
			//				 TradeSignal.GoShort()(newPositions);
			//				}

	
	def xNewPeriod(timeFrame as Interval):
		if timeFrame.Equals(Intervals.Day1):
			tick as Tick = Ticks[0]
			gap as double = Math.Abs((tick.Bid - tick.Bid))
			if gap > 500:
				Reset()
		
		if timeFrame.Equals(IntervalDefault):
			//				for( int i=0; i<Ticks.Count; i++) {
			//					Elapsed diff = tick.Time - Ticks[i].Time;
			//					if( diff.TotalSeconds > 300) {
			//						velocity[0] = (tick.Ask+tick.Bid) - (Ticks[i].Ask+Ticks[i].Bid);
			//						break;
			//					}
			//				}
			velocity[0] = (Formula.Middle(Minutes, 0) - Formula.Middle(Minutes, 3))
			
			longStretch = cast(int, (Bars.Low[0] - lastShortPrice))
			shortStretch = cast(int, (lastLongPrice - Bars.High[0]))
			
			if Bars.High[0] > longRetrace[0]:
				longRetrace[0] = Bars.High[0]
				lastLongBar = Bars.CurrentBar
				lastLongPrice = Bars.High[0]
			elif double.IsNaN(longRetrace[0]):
				longRetrace[0] = Bars.High[0]
			else:
				longRetrace[0] -= 1
			
			
			if Bars.Low[0] < shortRetrace[0]:
				shortRetrace[0] = Bars.Low[0]
				lastShortBar = Bars.CurrentBar
				lastShortPrice = Bars.Low[0]
			elif double.IsNaN(shortRetrace[0]):
				shortRetrace[0] = Bars.Low[0]
			else:
				shortRetrace[0] += 1
			
			if longStretch <= 0:
				longStretchSum = 0
			longStretchSum += longStretch
			if shortStretch <= 0:
				shortStretchSum = 0
			shortStretchSum += shortStretch
			
			trendUp = (longStretchSum > shortStretchSum)
			stretch[0] = (shortStretch if trendUp else longStretch)
			//				trend[0] = trendUp ? 1 : -1;
			trend[0] = (longStretch - shortStretch)

	private trendUp = false

	private longStretchSum as long = 0

	private shortStretchSum as long = 0

	private lastShortBar = 0

	private lastShortPrice as double = 0

	private lastLongBar = 0

	private lastLongPrice as double = 0

	
	private codes as Dictionary[of long, CodeStats] = Dictionary[of long, CodeStats]()

	private lastCodes as Integers

	private lastPrices as Doubles

	private lastCode = 0

	//		int lastPricex = 0;
	private lastBar = 0

	
	
	override def OnIntervalClose() as bool:
		//			switch( newCode) {
		//				case 1: newCode = 0; break;
		//				case 6: newCode = 7; break;
		//			}
		// Got a code change, close out the previous one.
		combinedCode as int
		newCode as int = lrGraph.Code
		stretch[0] = newCode
		if newCode != lastCode:
			if lastCodes.Count > 2:
				combinedCode = ((lastCodes[1] * 8) + lastCodes[0])
				codeStats as CodeStats
				if codes.TryGetValue(combinedCode, codeStats):
					codeStats.Count += 1
					codeStats.CountBars += (Bars.CurrentBar - lastBar)
					codeStats.ProfitLoss += (Bars.Close[0] - lastPrices[0])
					codes[combinedCode] = codeStats
				else:
					codeStats.Count = 1
					codeStats.CountBars = 1
					codeStats.ProfitLoss = 0
					codes[combinedCode] = codeStats
			// Alright start the new code.
			lastCodes.Add(newCode)
			lastPrices.Add(Bars.Close[0])
			if lastCodes.Count > 2:
				combinedCode = ((lastCodes[1] * 8) + lastCodes[0])
				TradeDecision(combinedCode)
			lastBar = Bars.CurrentBar
			lastCode = newCode
		else:
			pass
			
		//			switch( lrGraph.Code) {
		//				case 0: TradeSignal.GoShort(); break;
		//				case 1: TradeSignal.GoShort(); break;
		//				case 2: TradeSignal.GoShort(); break;
		//				case 3: TradeSignal.GoShort(); break;
		//				case 4: TradeSignal.GoLong(); break;
		//				case 5: TradeSignal.GoLong(); break;
		//				case 6: TradeSignal.GoLong(); break;
		//				case 7: TradeSignal.GoLong(); break;
		//			}
		return true

	
	def TradeDecision(combinedCode as int):
		converterGeneratedName1 = combinedCode
		if (((((converterGeneratedName1 == 1) or (converterGeneratedName1 == 8)) or (converterGeneratedName1 == 11)) or (converterGeneratedName1 == 26)) or (converterGeneratedName1 == 19)) or (converterGeneratedName1 == 16):
			Enter.SellMarket()
		else:
			Exit.GoFlat()

	
	override def OnEndHistorical():
		Log.Notice(('Bars Processed: ' + Bars.CurrentBar))
		Log.Notice(('Code combo count = ' + codes.Count))
		Log.Notice('From,From,To,Count,Bars,AverageBars,ProfitLoss,AveragePL')
		for kvp as KeyValuePair[of long, CodeStats] in codes:
			Log.Notice((((((((((((((((kvp.Key / 64) + ',') + ((kvp.Key % 64) / 8)) + ',') + (kvp.Key % 8)) + ',') + kvp.Value.Count) + ',') + kvp.Value.CountBars) + ',') + (kvp.Value.CountBars / kvp.Value.Count)) + ',') + kvp.Value.ProfitLoss) + ',') + (kvp.Value.ProfitLoss / kvp.Value.Count)))

	private lastResetTime as TimeStamp

	def Reset():
		lastResetTime = Ticks[0].Time

