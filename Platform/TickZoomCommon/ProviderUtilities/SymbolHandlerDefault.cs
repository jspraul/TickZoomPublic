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
using TickZoom.Api;

namespace TickZoom.Common
{
	public class SymbolHandlerDefault : SymbolHandler {
		private TickIO tickIO = Factory.TickUtil.TickIO();
		private Receiver receiver;
		private SymbolInfo symbol;
		private bool isTradeInitialized = false;
		private bool isQuoteInitialized = false;
		private double position = 0D;
		public int bidSize;
		public double bid = 0D;
		public int askSize;
		public double ask = 0D;
		public int lastSize;
		public double last = 0D;
        private LogicalOrderHandler logicalOrderHandler;
        private bool isRunning = false;
		public void Start()
		{
			isRunning = true;
		}
		
		public void Stop()
		{
			isRunning = false;
		}
        
		public SymbolHandlerDefault(SymbolInfo symbol, Receiver receiver) {
        	this.symbol = symbol;
			this.receiver = receiver;
		}
		
		public void SendQuote() {
			if( isQuoteInitialized) {
				if( isRunning && symbol.QuoteType == QuoteType.Level1) {
					tickIO.Initialize();
					tickIO.SetSymbol(symbol.BinaryIdentifier);
					tickIO.SetTime(TimeStamp.UtcNow);
					tickIO.SetQuote(Bid,Ask,(ushort)BidSize,(ushort)AskSize);
					TickBinary binary = tickIO.Extract();
					receiver.OnEvent(symbol,(int)EventType.Tick,binary);
				}
			} else {
				VerifyQuote();
			}
		}
        
        public void AddPosition( double position) {
        	this.position += position;
        }
	        	
        public void SetPosition( double position) {
        	if( this.position != position) {
	        	this.position = position;
//	        	LogicalFillBinary fill = new LogicalFillBinary(position,tickIO.Bid,tickIO.Time,0);
//	        	receiver.OnEvent(symbol,(int)EventType.LogicalFill,fill);
        	}
        }
        
		private void VerifyQuote() {
			if(BidSize > 0 && Bid > 0 && AskSize > 0 && Ask > 0) {
				isQuoteInitialized = true;
			}
		}
        
		private void VerifyTrade() {
			if(LastSize > 0 & Last > 0) {
				isTradeInitialized = true;
			}
		}
        
		public void SendTimeAndSales() {
			if( !isRunning || symbol.TimeAndSales != TimeAndSales.ActualTrades ) {
				return;
			}
			if( !isTradeInitialized ) {
				VerifyTrade();
				return;
			}
			if( symbol.QuoteType == QuoteType.Level1 && !isQuoteInitialized) {
				VerifyQuote();
				return;
			}
			if( symbol.TimeAndSales == TimeAndSales.ActualTrades) {
				tickIO.Initialize();
				tickIO.SetSymbol(symbol.BinaryIdentifier);
				tickIO.SetTime(TimeStamp.UtcNow);
				tickIO.SetTrade(Last,LastSize);
				if( symbol.QuoteType == QuoteType.Level1) {
					tickIO.SetQuote(Bid,Ask,(ushort)BidSize,(ushort)AskSize);
				}
				TickBinary binary = tickIO.Extract();
				receiver.OnEvent(symbol,(int)EventType.Tick,binary);
			}
		}
        
		public LogicalOrderHandler LogicalOrderHandler {
			get { return logicalOrderHandler; }
			set { logicalOrderHandler = value; }
		}
		
		public double Position {
			get { return position; }
		}
		
		public int LastSize {
			get { return lastSize; }
			set { lastSize = value; }
		}
		
		public int BidSize {
			get { return bidSize; }
			set { bidSize = value; }
		}
		
		private void AssureValue(double value) {
			if( double.IsInfinity(value) || double.IsNaN(value) ) {
				throw new ApplicationException("Value must not be infinity or NaN.");
			}
		}
		
		public double Bid {
			get { return bid; }
			set { AssureValue(value); bid = value; }
		}
		
		public int AskSize {
			get { return askSize; }
			set { askSize = value; }
		}
		
		public double Ask {
			get { return ask; }
			set { AssureValue(value); ask = value; }
		}
		
		public double Last {
			get { return last; }
			set { last = value; }
		}
		
		public bool IsRunning {
			get { return isRunning; }
		}
	}
}
