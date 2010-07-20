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

class MedianBreakout(StrategyCommon):

	private breakoutLength = 0

	private averageLength = 0

	private indicatorColor as Color = Color.Blue

	private entryPrice as double

	private stopLoss = 400

	//		int riskLimit = 400;
	
	private sma as SMA

	
	def constructor():
		pass

	
	override def OnInitialize():
		sma = SMA(Bars.Close, averageLength)
		AddIndicator(sma)

	
	override def OnIntervalClose() as bool:
		if Bars.BarCount > 1:
			if Position.IsFlat and (Bars.Typical[1] > Formula.Highest(Bars.High, breakoutLength, 2)):
				//					risk < riskLimit &&	risk > 0) {
				
				Enter.BuyMarket()
				entryPrice = Ticks[0].Bid
			
		return true

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if ((timeFrame.BarUnit == BarUnit.Hour) and (timeFrame.Period == 1)) and (Bars.Count > 1):
			if Position.IsLong:
				if Hours.Typical[1] < (sma[0] - stopLoss):
					Exit.GoFlat()
		return true

	
	BreakoutLength as int:
		get:
			return breakoutLength
		set:
			breakoutLength = value

	
	override def ToString() as string:
		return ((((breakoutLength + ',') + averageLength) + ',') + stopLoss)

	
	AverageLength as int:
		get:
			return averageLength
		set:
			averageLength = value

	
	StopLoss as int:
		get:
			return stopLoss
		set:
			stopLoss = value
	

