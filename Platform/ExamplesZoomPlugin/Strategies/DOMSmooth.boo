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

class DOMSmooth(StrategyCommon):

	private ratio as IndicatorCommon

	private bid as IndicatorCommon

	private ask as IndicatorCommon

	
	
	def constructor():
		pass

	
	override def OnInitialize():
		ratio = IndicatorCommon()
		ratio.Drawing.GraphType = GraphType.Histogram
		ratio.Drawing.GroupName = 'Ratio'
		AddIndicator(ratio)
		
		bid = IndicatorCommon()
		bid.Drawing.GroupName = 'Bid/Ask'
		bid.Drawing.PaneType = PaneType.Secondary
		AddIndicator(bid)
		
		ask = IndicatorCommon()
		ask.Drawing.GroupName = 'Bid/Ask'
		ask.Drawing.PaneType = PaneType.Secondary
		ask.Drawing.Color = Color.Red
		AddIndicator(ask)

	
	private sumBidSizes as int

	private countBidLevels as int

	private sumAskSizes as int

	private countAskLevels as int

	override def OnProcessTick(tick as Tick) as bool:
		for i in range(0, 5):
			sumBidSizes += tick.BidLevel(i)
			sumAskSizes += tick.AskLevel(i)
		countAskLevels += 5
		countBidLevels += 5
		
		bid[0] += tick.BidLevel(0)
		// + tick.BidLevels(1);
		ask[0] += tick.AskLevel(0)
		// + tick.AskLevels(1);
		return true

	
	override def OnIntervalOpen() as bool:
		bid[0] = 0
		ask[0] = 0
		sumBidSizes = 0
		countBidLevels = 0
		sumAskSizes = 0
		countAskLevels = 0
		return true

	
	override def OnIntervalClose() as bool:
		//			bid[0] = sumBidSizes / countBidLevels;
		//			ask[0] = sumAskSizes/countAskLevels;
		
		if bid[0] > ask[0]:
			ratio[0] = (0 if (ask[0] == 0) else ((cast(double, bid[0]) / cast(double, ask[0])) - 1))
		elif ask[0] > bid[0]:
			ratio[0] = (0 if (bid[0] == 0) else (((-cast(double, ask[0])) / cast(double, bid[0])) + 1))
		else:
			ratio[0] = 0
		return true

