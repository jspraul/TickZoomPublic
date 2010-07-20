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
using System.Threading;
using System.Windows.Forms;
using TickZoom.Api;

//using System.Data;
namespace TickZoom.eSignal
{
	public class SymbolHandler {
		private TickIO tickIO = Factory.TickUtil.TickIO();
		private Receiver receiver;
		private SymbolInfo symbol;
		private bool isInitialized = false;
		public int Position;
		public int BidSize;
		public double Bid;
		public int AskSize;
		public double Ask;
		public int LastSize;
		public double Last;
		public SymbolHandler(SymbolInfo symbol, Receiver receiver) {
			this.symbol = symbol;
			this.receiver = receiver;
		}
		public void SendQuote() {
			if( isInitialized) {
				if( symbol.QuoteType == QuoteType.Level1) {
					tickIO.Initialize();
					tickIO.SetSymbol(symbol.BinaryIdentifier);
					tickIO.SetTime(TimeStamp.UtcNow);
					tickIO.SetQuote(Bid,Ask,(ushort)BidSize,(ushort)AskSize);					
					TickBinary binary = tickIO.Extract();
					receiver.OnSend(ref binary);
				}
			} else {
				VerifyInitialized();
			}
		}
		private void VerifyInitialized() {
			if(BidSize > 0 && Bid > 0 && AskSize > 0 && Ask > 0 && LastSize > 0 & Last > 0) {
				isInitialized = true;
			}
			else
			{
				if(Last > 0 && LastSize > 0)
				{
					Bid = Ask = Last;
					BidSize = AskSize = LastSize;
					isInitialized = true;
				}
			}
		}
		
		private void VerifyQuoteInitialized() {
			if(BidSize > 0 && Bid > 0 && AskSize > 0 && Ask > 0) {
				isInitialized = true;
			}
		}
		
		private bool VerifyQuote() {
			if(BidSize > 0 && Bid > 0 && AskSize > 0 && Ask > 0) {
				return true;
			}
			else return false;
		}
		public void SendTimeAndSales() {
			if( isInitialized) {
				if( symbol.TimeAndSales == TimeAndSales.ActualTrades) {
					tickIO.Initialize();
					
					tickIO.SetTrade(Last,LastSize);
					
					if( symbol.QuoteType == QuoteType.Level1) {
						if(Bid == 0)
						{
							Bid = Last;
							BidSize = LastSize;
						}
						if(Ask == 0)
						{
							Ask = Last;
							AskSize = LastSize;
						}
						tickIO.SetQuote(Bid,Ask,(ushort)BidSize,(ushort)AskSize);
					}
					
					tickIO.SetSymbol(symbol.BinaryIdentifier);
					tickIO.SetTime(TimeStamp.UtcNow);					
					
					TickBinary binary = tickIO.Extract();
					receiver.OnSend(ref binary);
				}
			} else {
				VerifyInitialized();
			}
		}
	}
}
