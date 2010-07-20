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

#region Namespaces


#endregion

namespace TickZoom

import System
import TickZoom.Common

class ExampleOrderStrategy(StrategyCommon):
	
	def constructor():
		Performance.GraphTrades = true
		Performance.Equity.GraphEquity = true
		ExitStrategy.ControlStrategy = false
		ExitStrategy.BreakEven = 0.3
		ExitStrategy.StopLoss = 0.45
		PositionSize.Size = 10000
	
	def OnInitialize():
		pass
		
	def OnIntervalClose() as bool:
		Log.Notice( "close: " + Ticks[0] + " " + Minutes.Close[0] + " " + Minutes.Time[0])
		if Bars.Close[0] > Bars.Open[0]:
			if Position.IsFlat:
				Enter.BuyStop(Bars.Close[0] + 0.1)
				Exit.SellStop(Bars.Close[0] - 0.1)
			if Position.IsShort:
				Exit.BuyLimit(Bars.Close[0] - 0.03)
		if Position.IsLong:
			Exit.SellStop(Bars.Close[0] - 0.1)
		if Bars.Close[0] < Bars.Open[0]:
			if Position.IsFlat:
				Enter.SellLimit(Bars.Close[0] + 0.3)
				ExitStrategy.StopLoss = 0.45
		if Bars.Close[0] < Bars.Open[0] and Bars.Open[0] < Bars.Close[1]:
			if Position.IsFlat:
				Enter.SellMarket()
				ExitStrategy.StopLoss = 0.15
		return true