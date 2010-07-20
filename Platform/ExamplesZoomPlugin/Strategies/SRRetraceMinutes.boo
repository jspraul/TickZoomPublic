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

class SRRetraceMinutes(StrategyCommon):

	private retrace as Retrace

	private contractSize = 1

	private stretch as IndicatorCommon

	private domRatio as DOMRatio

	private velocity as IndicatorCommon

	private lrGraph as LRGraph

	
	def constructor():
		ExitStrategy.ControlStrategy = false

	
	override def OnInitialize():
		
		super.OnInitialize()
		retrace = Retrace()
		retrace.Drawing.Color = Color.Red
		AddIndicator(retrace)
		
		velocity = IndicatorCommon()
		velocity.Drawing.PaneType = PaneType.Secondary
		velocity.Drawing.GraphType = GraphType.Histogram
		velocity.Drawing.GroupName = 'Velocity'
		AddIndicator(velocity)
		
		lrGraph = LRGraph()
		AddIndicator(lrGraph)
		
		stretch = IndicatorCommon()
		stretch.Drawing.Color = Color.Red
		stretch.Drawing.GroupName = 'Stretch'
		AddIndicator(stretch)
		
		domRatio = DOMRatio()
		domRatio.Drawing.Color = Color.Green
		AddIndicator(domRatio)
		
		Reset()
		// Initialize

	
	private positionsize as double

	private newPositions as double

	override def OnProcessTick(tick as Tick) as bool:
		if Bars.Count == 1:
			Exit.GoFlat()
			Reset()
			return true
		for i in range(0, Ticks.Count):
		//			if( tick.AskDepth + tick.BidDepth < 3000) {
		//			 TradeSignal.GoFlat();
		//				Reset();
		//				return;
		//			}			
		
			diff as Elapsed = (tick.Time - Ticks[i].Time)
			if diff.TotalSeconds > 120:
				velocity[0] = ((tick.Ask + tick.Bid) - (Ticks[i].Ask + Ticks[i].Bid))
				break 
		
		if Position.HasPosition:
			// TODO: Handle commission costs correctly inside Trade object.
			profitTarget = cast(int, (Position.Size * 30))
			// *2 to double cost of commission.
			current as int = Performance.ComboTrades.Current
			if Performance.ComboTrades.ProfitInPosition(current, Performance.Equity.CurrentEquity) >= profitTarget:
				Exit.GoFlat()
				Reset()
				return true
		
		positionsize = Math.Max(0, (retrace.Stretch / 100))
		newPositions = Position.Size
		
		if positionsize > Position.Size:
			newPositions = (Position.Size + contractSize)
			//				if( retrace.Stretch > 1000) { newPositions+=contractSize; };
			//				if( retrace.Stretch > 1500) { newPositions+=contractSize; };
			//				if( retrace.Stretch > 2000) { newPositions+=contractSize; };
			//				if( retrace.Stretch > 2500) { newPositions+=contractSize; };
			//				if( retrace.Stretch > 3000) { newPositions+=contractSize; };
			//				if( retrace.Stretch > 3500) { newPositions+=contractSize; };
			
			if (Position.IsFlat or Position.IsLong) and (tick.Bid < retrace[0]):
				if velocity[0] > 0:
					Enter.BuyMarket(newPositions)
			
			if (Position.IsFlat or Position.IsShort) and (tick.Ask > retrace[0]):
				if velocity[0] < 0:
					Enter.SellMarket(newPositions)
		return true

	
	override def OnIntervalOpen(timeFrame as Interval) as bool:
		if timeFrame.Equals(Intervals.Day1):
			tick as Tick = Ticks[0]
			gap as double = Math.Abs((tick.Bid - tick.Bid))
			if gap > 500:
				Reset()
		
		if timeFrame.Equals(IntervalDefault):
			stretch[0] = retrace.Stretch
		return true

	
	private lastResetTime as TimeStamp

	def Reset():
		lastResetTime = Ticks[0].Time
		if retrace.Count > 0:
			retrace.Reset()

