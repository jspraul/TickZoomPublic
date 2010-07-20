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

namespace TickZoom.StarterTest
{
	/// <summary>
	/// Description of Symbol.
	/// </summary>
	public class InstrumentImpl
	{
		string symbol;
		int level2Rounding;
		int lotSize;
		int decimalFactor;
		int depthIncrement;
		int tradeSize;
		int lotSizeDomLimit;

		public InstrumentImpl(string symbol, int decimalPlaces, int lotSize, int lotSizeDomLimit, int tradeSize, int level2Rounding, int depthIncrement)
		{
			this.symbol = symbol;
			this.level2Rounding = level2Rounding;
			this.lotSize = lotSize;
			this.lotSizeDomLimit = lotSizeDomLimit;
			this.tradeSize = tradeSize;
			this.decimalFactor = decimalPlaces;
			this.depthIncrement = depthIncrement;
		}

		public int TradeSize {
			get { return tradeSize; }
		}

		public string Symbol {
			get { return symbol; }
		}

		public int Level2Rounding {
			get { return level2Rounding; }
		}

		public int LotSize {
			get { return lotSize; }
		}

		public int DecimalFactor {
			get { return decimalFactor; }
		}

		public int DepthIncrement {
			get { return depthIncrement; }
		}

		public static void Add(string symbol, int decimalFactor, int lotSize, int lotSizeDOMLimit, int tradeSize, int level2Rounding, int depthIncrement)
		{
			instruments.Add(symbol, new InstrumentImpl(symbol, decimalFactor, lotSize, lotSizeDOMLimit, tradeSize, level2Rounding, depthIncrement));
		}

		private static Dictionary<string, InstrumentImpl> instruments = new Dictionary<string, InstrumentImpl>();
		public static InstrumentImpl Get(string symbol)
		{
			if (instruments.Count == 0) {
				Add("/ESU8", 100, 1, 1, 100, 2, 25);
				Add("/ESZ8", 100, 1, 1, 100, 2, 25);
				Add("MSFT", 100, 100, 1, 100, 2, 1);
				Add("GOOG", 100, 100, 1, 100, 2, 1);

				Add("USD/JPY", 1000, 10000, 100, 1000, 2, 10);
				Add("USD/CHF", 100000, 10000, 100, 1000, 4, 10);

				Add("USD/CAD", 100000, 10000, 100, 1000, 4, 10);
				Add("AUD/USD", 100000, 10000, 100, 1000, 4, 10);
				Add("CHF/JPY", 1000, 10000, 100, 1000, 2, 10);
				Add("USD/NOK", 100000, 10000, 100, 1000, 4, 10);
				Add("EUR/USD", 100000, 10000, 100, 1000, 4, 10);

				Add("USD/SEK", 100000, 10000, 100, 1000, 4, 10);
				Add("USD/DKK", 100000, 10000, 100, 1000, 4, 10);
				Add("GBP/USD", 100000, 10000, 100, 1000, 4, 10);
				Add("EUR/CHF", 100000, 10000, 100, 1000, 4, 10);
				Add("EUR/JPY", 1000, 10000, 100, 1000, 2, 10);

				Add("GBP/JPY", 1000, 10000, 100, 1000, 2, 10);
				Add("EUR/GBP", 100000, 10000, 100, 1000, 4, 10);
				Add("EUR/NOK", 100000, 10000, 100, 1000, 4, 10);
				Add("EUR/SEK", 100000, 10000, 100, 1000, 4, 10);
				Add("GBP/CHF", 100000, 10000, 100, 1000, 4, 10);

				Add("AUD/JPY", 1000, 10000, 100, 1000, 2, 10);
				Add("CAD/JPY", 1000, 10000, 100, 1000, 2, 10);
				Add("NZD/USD", 100000, 10000, 100, 1000, 4, 10);
				Add("AUD/CHF", 100000, 10000, 100, 1000, 4, 10);
				Add("AUD/CAD", 100000, 10000, 100, 1000, 4, 10);
			}
			return instruments[symbol];
		}

		public int LotSizeDomLimit {
			get { return lotSizeDomLimit; }
		}
	}
}
