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
using System.Collections.Generic;
using TickZoom.Api;

namespace TickZoom.Examples
{
	public class PointFigureBars : BarLogic
	{
#region Variables
		bool isUpBar = true;
		long boxSize;
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
			this.boxSize = boxSize * symbolInfo.MinimumTick.ToLong();
			this.reversal = reversal;
		}
		
		public void InitializeTick(Tick tick, BarData data) {
			long price = tick.IsTrade ? tick.lPrice : (tick.lBid + tick.lAsk)/2;
			volume = tick.IsTrade ? tick.Volume : 0;
			prevHigh = prevLow = price;
			open = high = low =	close = price;
			tickCount = 1;
			data.NewBar(open, high, low, close, volume, tick.Time, tickCount);
		}

		public bool IsNewBarNeeded(Tick tick) {
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
		
		public void ProcessTick(Tick tick, BarData data) {
			if( newBarNeeded ) {
				newBarNeeded = false;
				AddBar(tick,data);
			} else {
				UpdateBar(tick,data);
			}
		}
		
		private void UpdateBar(Tick tick,BarData data) {
			long price = tick.IsTrade ? tick.lPrice : (tick.lAsk + tick.lBid) / 2;
			volume += tick.IsTrade ? tick.Volume : 0;
			if( price >= high+boxSize) {
				high = high+boxSize;
			}
			if( price <= low-boxSize) {
				low = low-boxSize;
			}
			close = price;
			tickCount++;
			data.UpdateBar(high, low, close, volume, tick.Time, tickCount);
		}
		

		private void AddBar(Tick tick, BarData data)
		{
			prevHigh = high;
			prevLow = low;
			long price = tick.IsTrade ? tick.lPrice : (tick.lAsk + tick.lBid) / 2;
			volume += tick.IsTrade ? tick.Volume : 0;
			open = high = low =	close = price;
			tickCount = 1;
			data.NewBar(open, high, low, close, volume, tick.Time, tickCount);
			
			long highPrice = (prevHigh / boxSize) * boxSize;
			long lowPrice = (prevLow / boxSize) * boxSize + boxSize;
			if( isUpBar) {
				high = highPrice;
			} else {
				low = lowPrice;
			}
			isUpBar = !isUpBar;
		}
		
		public bool IsEndBarNeeded(Tick tick) {
			throw new NotImplementedException();
		}
		
		public void Dispose() {
			
		}
	}
}
