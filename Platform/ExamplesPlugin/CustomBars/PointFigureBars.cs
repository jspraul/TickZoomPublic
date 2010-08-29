#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General internal License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General internal License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Examples
{
	public class PointFigureBars : BarLogic
	{
#region Field Variables
		bool isUpBar = true;
		long boxSize;
		long minimumTick;
		
		public double BoxSize {
			get { return boxSize / minimumTick; }
			set { boxSize = (long) value * minimumTick; }
		}
		
		long reversal;
		bool newBarNeeded = false;
		long open;
		long high;
		long low;
		long close;
		int volume;
		int tickCount;
		long prevHigh;
		long prevLow;
#endregion

		public PointFigureBars(SymbolInfo symbolInfo, long boxSize, long reversal)
		{
			this.minimumTick = symbolInfo.MinimumTick.ToLong();
			this.boxSize = boxSize * minimumTick;
			this.reversal = reversal;
			IntervalDefault = Intervals.Define(BarUnit.Tick,100);
		}
		
		public override void InitializeBar(Tick tick, BarData data) {
			long price = tick.IsTrade ? tick.lPrice : (tick.lBid + tick.lAsk)/2;
			volume = tick.IsTrade ? tick.Volume : 0;
			prevHigh = prevLow = price;
			open = high = low =	close = price;
			tickCount = 1;
			data.NewBar(open, high, low, close, volume, tick.Time, tickCount);
		}
		
		public override bool IsNewBarNeeded(Tick tick) {
			long price = tick.IsTrade ? tick.lPrice : (tick.lBid + tick.lAsk)/2;
			newBarNeeded = false;
			long tempHigh = (long ) (high / boxSize) * boxSize;
			long tempLow = (long ) (low / boxSize) * boxSize + boxSize;
			if( isUpBar) {
				newBarNeeded = price < (tempHigh - boxSize * reversal );
			} else {
			    newBarNeeded = price  > (tempLow + boxSize* reversal );
			}
			return newBarNeeded;
		}
		
		public override void UpdateBar(Tick tick, BarData data) {
			if( newBarNeeded ) {
				newBarNeeded = false;
				AddBarInternal(tick,data);
			} else {
				UpdateBarInternal(tick,data);
			}
		}
		
		private void UpdateBarInternal(Tick tick, BarData data) {
			long price = tick.IsTrade ? tick.lPrice : (tick.lAsk + tick.lBid) / 2;
			volume += tick.IsTrade ? tick.Volume : 0;
			if( price >= high + boxSize) {
				high = high + boxSize;
				close = high;
			}
			if( price <= low - boxSize) {
				low = low - boxSize;
				close = low;
			}
			tickCount++;
			data.UpdateBar(high, low, close, volume, tick.Time, tickCount);
		}
		

		private void AddBarInternal(Tick tick, BarData data)
		{
			if( isUpBar) {
				open = high = high - boxSize;
				close = low = open - boxSize * reversal;
			} else {
				open = low = low + boxSize;
				close = high = open + boxSize * reversal;
			}
			volume += tick.IsTrade ? tick.Volume : 0;
			tickCount = 1;
			data.NewBar(open, high, low, close, volume, tick.Time, tickCount);
			isUpBar = !isUpBar;
		}
		
	}
}
