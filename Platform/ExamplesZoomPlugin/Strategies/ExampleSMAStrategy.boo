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
import System.Drawing
import TickZoom.Api
import TickZoom.Common

class ExampleSMAStrategy(StrategyCommon):

	private equity as IndicatorCommon

	private sma as SMA

	private length = 20

	private factor = 5

	private timeStamp = TimeStamp(2009, 3, 25)

	
	def constructor():
		pass

	
	override def OnInitialize():
		factor = 10
		Performance.GraphTrades = true
		PositionSize.Size = 10000
		
		sma = SMA(Bars.Close, length)
		sma.Drawing.IsVisible = true
		AddIndicator(sma)
		
		equity = IndicatorCommon()
		equity.Drawing.IsVisible = true
		equity.Drawing.PaneType = PaneType.Secondary
		equity.Drawing.GraphType = GraphType.FilledLine
		equity.Drawing.Color = Color.Green
		equity.Drawing.GroupName = 'SMAEquity'
		equity.Name = 'SMAEquity'
		AddIndicator(equity)
		

	
	override def OnIntervalClose() as bool:
		if Bars.Close[0] > sma[0]:
			Enter.BuyMarket(true)
		if Bars.Close[0] < sma[0]:
			Enter.SellMarket(true)
		equity[0] = Performance.Equity.CurrentEquity
		return true

	
	Length as int:
		get:
			return length
		set:
			length = value

	
	Factor as int:
		get:
			return factor
		set:
			factor = value

	
	TimeStamp as TimeStamp:
		get:
			return timeStamp
		set:
			timeStamp = value

