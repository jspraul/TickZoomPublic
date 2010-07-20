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

class ExampleScannerStrategy(PortfolioCommon):

	
	def constructor():
		pass

	
	override def OnInitialize():
		Performance.GraphTrades = true
		Performance.Equity.GraphEquity = true
		PositionSize.Size = 10000
		for i in range(0, Markets.Count):
			Markets[i].Performance.GraphTrades = true
			Markets[i].Performance.Equity.GraphEquity = true

	
	override def OnIntervalClose() as bool:
		for i in range(0, Markets.Count):
		// Example log message.
		//Log.WriteLine( "close: " + Ticks[0] + " " + Minutes.Close[0] + " " + Minutes.Time[0]);
		
			if Markets[i].Position.HasPosition:
				Markets[i].Exit.GoFlat()
			if Markets[i].Position.IsFlat:
				if Markets[i].Bars.Close[0] > Markets[i].Bars.High[1]:
					Markets[i].Enter.BuyMarket()
				if Markets[i].Bars.Close[0] < Markets[i].Bars.Low[1]:
					Markets[i].Enter.SellMarket()
		return true

