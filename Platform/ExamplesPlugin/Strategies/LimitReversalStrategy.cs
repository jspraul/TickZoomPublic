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

#region Namespaces
using System;
using System.ComponentModel;
using System.Drawing;

using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples.Indicators;
using TickZoom.Statistics;

#endregion

namespace TickZoom.Examples
{
	public class LimitReversalStrategy : Strategy
	{
		IndicatorCommon bidLine;
		IndicatorCommon askLine;
		bool isFirstTick = true;
		double minimumTick;
		double spread;
		int lotSize;
		double ask;
		double bid;
		
		public LimitReversalStrategy() {
		}
		
		public override void OnInitialize()
		{
			Performance.Equity.GraphEquity = true;
			
			minimumTick = Data.SymbolInfo.MinimumTick;
			lotSize = Data.SymbolInfo.Level2LotSize;
			spread = 5 * minimumTick;
			
			bidLine = Formula.Indicator();
			bidLine.Drawing.IsVisible = true;
			AddDependency(bidLine);
			
			askLine = Formula.Indicator();
			askLine.Drawing.IsVisible = true;
			AddDependency(askLine);
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			if( isFirstTick) {
				isFirstTick = false;
				ask = tick.Ask + spread;
				bid = tick.Bid - spread;
			}
			
			if( tick.Ask < ask) {
				ask = tick.Ask + spread;
			}
			
			if( tick.Bid > bid) {
				bid = tick.Bid - spread;
			}
			
			if( Position.IsLong) {
				Orders.Reverse.ActiveNow.SellLimit(ask, lotSize);
			} else if( Position.IsFlat) {
				Orders.Enter.ActiveNow.SellLimit(ask, lotSize);
			}
			
			if( Position.IsShort) {
				Orders.Reverse.ActiveNow.BuyLimit(bid, lotSize);
			} else if( Position.IsFlat) {
				Orders.Enter.ActiveNow.BuyLimit(bid, lotSize);
			}
			
			bidLine[0] = bid;
			askLine[0] = ask;
			return true;
		}
		
		public override void OnEnterTrade()
		{
			ask = Ticks[0].Ask + spread;
			bid = Ticks[0].Bid - spread;
		}
		
		public override void OnChangeTrade()
		{
			ask = Ticks[0].Ask + spread;
			bid = Ticks[0].Bid - spread;
		}
		public override void OnExitTrade()
		{
			ask = Ticks[0].Ask + spread;
			bid = Ticks[0].Bid - spread;
		}
	}
}
