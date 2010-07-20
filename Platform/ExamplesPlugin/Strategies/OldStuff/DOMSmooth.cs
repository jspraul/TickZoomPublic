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
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of RandomStrategy.
	/// </summary>
	public class DOMSmooth : Strategy
	{
		IndicatorCommon ratio, bid, ask;
		
		
		public DOMSmooth()
		{
		}
		
		public override void OnInitialize() {
			ratio = new IndicatorCommon();
			ratio.Drawing.GraphType = GraphType.Histogram;
			ratio.Drawing.GroupName = "Ratio";
			AddIndicator(ratio);
			
			bid = new IndicatorCommon();
			bid.Drawing.GroupName = "Bid/Ask";
			bid.Drawing.PaneType = PaneType.Secondary;
			AddIndicator(bid);
			
			ask = new IndicatorCommon();
			ask.Drawing.GroupName = "Bid/Ask";
			ask.Drawing.PaneType = PaneType.Secondary;
			ask.Drawing.Color = Color.Red;
			AddIndicator(ask);
		}
		
		int sumBidSizes;
		int countBidLevels;
		int sumAskSizes;
		int countAskLevels;
		public override bool OnProcessTick(Tick tick)
		{
			for( int i=0; i<5; i++) {
				sumBidSizes += tick.BidLevel(i);
				sumAskSizes += tick.AskLevel(i);
			}
			countAskLevels += 5;
			countBidLevels += 5;
			
			bid[0] += tick.BidLevel(0);// + tick.BidLevels(1);
			ask[0] += tick.AskLevel(0);// + tick.AskLevels(1);
			return true;
		}
		
		public override bool OnIntervalOpen()
		{
			bid[0] = 0;
			ask[0] = 0;
			sumBidSizes = 0;
			countBidLevels = 0;
			sumAskSizes = 0;
			countAskLevels = 0;
			return true;
		}

		public override bool OnIntervalClose()
		{
//			bid[0] = sumBidSizes / countBidLevels;
//			ask[0] = sumAskSizes/countAskLevels;
			if( bid[0] > ask[0] ) {
				ratio[0] = ask[0] == 0 ? 0 : (double) bid[0] / (double) ask[0] - 1;
			} else if( ask[0] > bid[0] ) {
				ratio[0] = bid[0] == 0 ? 0 : - (double) ask[0] / (double) bid[0] + 1;
			} else {
				ratio[0] = 0;
			}
			return true;
		}
	}
}
