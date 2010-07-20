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

class DetrendOSC(IndicatorCommon):

	private period = 5

	private price as Prices = null

	private sma as SMA

	
	def constructor():
		pass

	
	def constructor(period as int):
		self.period = period

	
	def constructor(price as Prices, period as int):
		self.price = price
		self.period = period

	
	override def OnInitialize():
		if price is null:
			price = Bars.Close
		sma = SMA(price, period)
		sma.IntervalDefault = IntervalDefault
		sma.Drawing.IsVisible = false
		AddIndicator(sma)

	
	override def OnIntervalClose() as bool:
		if Count < ((period / 2) + 1):
			self[0] = 0
		else:
			self[0] = (price[0] - sma[((period / 2) + 1)])
		return true

	Period as int:
		get:
			return period
		set:
			period = value

