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
import TickZoom.Common
import TickZoom.Api

class DailySMA(IndicatorCommon):

	private period = 5
	private displace = 0
	private price as Doubles
	private externalPrice as Prices
	private top as IndicatorCommon
	private bottom as IndicatorCommon
	private center as IndicatorCommon

	def constructor():
		self.price = Doubles()

	def constructor(period as int, displace as int):
		self.price = Doubles()
		self.period = period
		self.displace = displace

	def constructor(price as Prices, period as int, displace as int):
		self(period, displace)
		self.externalPrice = price
	override def OnInitialize():
		if externalPrice is null:
			externalPrice = Bars.Close
		original as PaneType = Drawing.PaneType
		top = IndicatorCommon()
		top.Drawing.PaneType = original
		AddIndicator(top)
		bottom = IndicatorCommon()
		bottom.Drawing.PaneType = original
		AddIndicator(bottom)
		center = IndicatorCommon()
		center.Drawing.PaneType = original
		AddIndicator(center)
		Drawing.IsVisible = false

	
	override def OnProcessTick(tick as Tick) as bool:
		if Chart.IsDynamicUpdate:
			UpdateAverage()
		return true

	
	Width as double:
		get:
			return (top[0] - bottom[0])

	
	def IsUpper(tick as Tick) as bool:
		return ((tick.Ask < top[0]) and (tick.Bid > (top[0] - (Width / 3))))

	
	def IsLower(tick as Tick) as bool:
		return ((tick.Ask < (bottom[0] + (Width / 3))) and (tick.Bid > bottom[0]))

	
	private countBars as int

	def Reset():
		if Count > 1:
			level as double = price[0]
			i = 1
			goto converterGeneratedName1
			while true:
				i += 1
				:converterGeneratedName1
				break  unless ((i < price.Count) and (i < period))
				price[i] = cast(int, level)
			self[1] = level
			self[0] = level

	
	
	override def OnIntervalClose() as bool:
		price.Add(externalPrice[0])
		UpdateAverage()
		if Count > 2:
			top[0] = (self[displace] + (IntervalDefault.Period * 2))
			bottom[0] = (self[displace] - (IntervalDefault.Period * 2))
			center[0] = self[displace]
		return true

	
	
	def UpdateAverage():
		if Count == 1:
			self[0] = price[0]
			countBars = 1
		else:
			countBars += 1
			
			last = self[1]
			sum = (last * Math.Min((countBars - 1), period))
			
			if (countBars > period) and (price.BarCount > period):
				x = (((sum + price[0]) - price[period]) / Math.Min(countBars, period))
				self[0] = x
			elif price.BarCount > 0:
				self[0] = ((sum + price[0]) / Math.Min(countBars, period))

	
	Period as int:
		get:
			return period
		set:
			period = value

	
	Top as IndicatorCommon:
		get:
			return top

	
	Bottom as IndicatorCommon:
		get:
			return bottom

	
	Center as IndicatorCommon:
		get:
			return center

