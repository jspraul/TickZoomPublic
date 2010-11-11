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
using System.IO;

namespace TickZoom.Api
{
	// Binary Values with amount data stored in the the tick.
	// This makes more efficent reading and writing of data.
	public class ContentBit {
		public const byte Quote=1;
		public const byte TimeAndSales=2;
		public const byte DepthOfMarket=4;
		public const byte SimulateTicks=8;
		public const byte FakeTick=16;
	}
	
	/// <summary>
	/// Description of TickDOM.
	/// </summary>
	public interface Tick 
	{
		int BidDepth {
			get;
		}
		
		int AskDepth {
			get;
		}
		
		double Bid {
			get;
		}
		
		double Ask {
			get;
		}
		
		long lBid {
			get;
		}
		
		long lAsk {
			get;
		}
		
		TradeSide Side {
			get;
		}
		
		double Price {
			get;
		}
		
		long lPrice {
			get;
		}
		
		int Size {
			get;
		}
		
		int Volume {
			get;
		}
		
		short AskLevel(int level);
		
		short BidLevel(int level);
		
		int DomLevels {
			get;
		}
		
		TimeStamp Time {
			get;
		}
		
		TimeStamp UtcTime {
			get;
		}
		
		byte ContentMask {
			get;
		}
		
		bool IsTrade {
			get;
		}
		
		bool IsQuote {
			get;
		}
		
		TickBinary Extract();
	}
}
