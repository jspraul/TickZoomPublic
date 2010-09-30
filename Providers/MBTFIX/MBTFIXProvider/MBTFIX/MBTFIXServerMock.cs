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
using System.Text;

using TickZoom.Api;
using TickZoom.FIX;
using TickZoom.MBTQuotes;

namespace TickZoom.MBTFIX
{
	public enum ServerState {
		Startup,
		LoggedIn
	}
	
	public class MBTFIXServerMock : FIXServerMock {
		private static Log log = Factory.SysLog.GetLogger(typeof(FIXServerMock));
		private static bool trace = log.IsTraceEnabled;
		private static bool debug = log.IsDebugEnabled;
		private ServerState fixState = ServerState.Startup;
		private ServerState quoteState = ServerState.Startup;
		
		public MBTFIXServerMock(ushort fixPort, ushort quotesPort, PacketFactory fixPacketFactory, PacketFactory quotePacketFactory) 
			: base( fixPort, quotesPort, fixPacketFactory, quotePacketFactory) {			
		}
			
		public override void StartFIXSimulation()
		{
			base.StartFIXSimulation();
		}

		public override void StartQuoteSimulation()
		{
			base.StartQuoteSimulation();
		}
		
		public override Yield ParseFIXMessage(Packet packet)
		{
			var packetFIX = (PacketFIX4_4) packet;
			var result = Yield.NoWork.Repeat;
			switch( packetFIX.MessageType) {
				case "A": // Login
					result = FIXLogin( packetFIX);
					break;
				case "AF": // Request Orders
					result = FIXOrderList( packetFIX);
					break;
				case "AN": // Request Positions
					result = FIXPositionList( packetFIX);
					break;
				case "G":
				case "D":
					break;
			}			
			return result;
		}
		
		public override Yield ParseQuotesMessage(Packet packet)
		{
			var packetQuotes = (PacketMBTQuotes) packet;
			var result = Yield.NoWork.Repeat;
			char firstChar = (char) packetQuotes.Data.GetBuffer()[packetQuotes.Data.Position];
			switch( firstChar) {
				case 'L': // Login
					result = QuotesLogin( packetQuotes);
					break;
				case 'S':
					result = SymbolRequest( packetQuotes);
					break;
			}			
			return result;
		}
		
		private Yield FIXOrderList(PacketFIX4_4 packet) {
			fixWritePacket = fixSocket.CreatePacket();			
			var mbtMsg = new FIXMessage4_4(packet.Target,packet.Sender);
			mbtMsg.SetText("END");
			mbtMsg.AddHeader("8");
			string message = mbtMsg.ToString();
			fixWritePacket.DataOut.Write(message.ToCharArray());
			
			if(debug) log.Debug("Sending end of order list: " + message);

			return Yield.DidWork.Invoke(WriteToFIX);
		}
		
		private Yield FIXPositionList(PacketFIX4_4 packet) {
			fixWritePacket = fixSocket.CreatePacket();			
			var mbtMsg = new FIXMessage4_4(packet.Target,packet.Sender);
			mbtMsg.SetText("DONE");
			mbtMsg.AddHeader("AO");
			string message = mbtMsg.ToString();
			fixWritePacket.DataOut.Write(message.ToCharArray());
			
			if(debug) log.Debug("Sending end of position list: " + message);

			return Yield.DidWork.Invoke(WriteToFIX);
		}
		
		private Yield FIXLogin(PacketFIX4_4 packet) {
			if( fixState != ServerState.Startup) {
				return CloseWithFixError(packet, "Invalid login request. Already logged in.");
			}
			fixState = ServerState.LoggedIn;
			fixWritePacket = fixSocket.CreatePacket();
			
			var mbtMsg = new FIXMessage4_4(packet.Target,packet.Sender);
			mbtMsg.SetEncryption(0);
			mbtMsg.SetHeartBeatInterval(30);
			mbtMsg.AddHeader("A");
			string login = mbtMsg.ToString();
			fixWritePacket.DataOut.Write(login.ToCharArray());
			
			if(debug) log.Debug("Sending login response: " + login);

			return Yield.DidWork.Invoke(WriteToFIX);
		}
		
		private Yield QuotesLogin(PacketMBTQuotes packet) {
			if( quoteState != ServerState.Startup) {
				return CloseWithQuotesError(packet, "Invalid login request. Already logged in.");
			}
			quoteState = ServerState.LoggedIn;
			quoteWritePacket = quoteSocket.CreatePacket();
			string message = "G|100=DEMOXJSP;8055=demo01\n";
			if( debug) log.Debug("Login response: " + message);
			quoteWritePacket.DataOut.Write(message.ToCharArray());
			return Yield.DidWork.Invoke(WriteToQuotes);
		}
		
		private unsafe Yield SymbolRequest(PacketMBTQuotes packet) {
			var data = packet.Data;
			data.Position += 2;
			SymbolInfo symbolInfo = null;
			fixed( byte *bptr = data.GetBuffer()) {
				byte *ptr = bptr + data.Position;
				while( ptr - bptr < data.Length) {
					int key = packet.GetKey(ref ptr);
					switch( key) {
						case 1003: // Symbol
							var symbol = packet.GetString( ref ptr);
							symbolInfo = Factory.Symbol.LookupSymbol(symbol);
							log.Info("Received symbol request for " + symbolInfo);
							AddSymbol(symbol, OnTick);
							break;
						case 2000: // Type of data.
							var feedType = packet.GetString( ref ptr);
							switch( feedType) {
								case "20000": // Level 1
									if( symbolInfo.QuoteType != QuoteType.Level1) {
										throw new ApplicationException("Requested data feed of Level1 but Symbol.QuoteType is " + symbolInfo.QuoteType);
									}
									break;
								case "20001": // Level 2
									if( symbolInfo.QuoteType != QuoteType.Level2) {
										throw new ApplicationException("Requested data feed of Level2 but Symbol.QuoteType is " + symbolInfo.QuoteType);
									}
									break;
								case "20002": // Level 1 & Level 2
									if( symbolInfo.QuoteType != QuoteType.Level2) {
										throw new ApplicationException("Requested data feed of Level1 and Level2 but Symbol.QuoteType is " + symbolInfo.QuoteType);
									}
									break;
								case "20003": // Trades
									if( symbolInfo.TimeAndSales != TimeAndSales.ActualTrades) {
										throw new ApplicationException("Requested data feed of Trades but Symbol.TimeAndSale is " + symbolInfo.TimeAndSales);
									}
									break;
								case "20004": // Option Chains
									break;
							}
							break;
					}
				}
			}
			return Yield.DidWork.Repeat;
		}
		
		private Yield OnTick( SymbolInfo symbol, Tick tick) {
			if( trace) log.Trace("Sending tick: " + tick);
			quoteWritePacket = quoteSocket.CreatePacket();
			StringBuilder sb = new StringBuilder();
			if( tick.IsTrade) {
				sb.Append("3|"); // Trade
			} else {
				sb.Append("1|"); // Level 1
			}
			sb.Append("2026=USD;"); //Currency
			sb.Append("1003="); //Symbol
			sb.Append(symbol.Symbol);
			sb.Append(';');
			sb.Append("2037=0;"); //Open Interest
			sb.Append("2085=.144;"); //Unknown
			sb.Append("2048=00/00/2009;"); //Unknown
			sb.Append("2049=00/00/2009;"); //Unknown
			sb.Append("2002="); //Last Trade.
			sb.Append(tick.Price);
			sb.Append(';');
			sb.Append("2050=0;"); //Unknown
			sb.Append("2003="); // Last Bid
			sb.Append(tick.Bid);
			sb.Append(';');
			sb.Append("2051=0;"); //Unknown
			sb.Append("2004="); //Last Ask 
			sb.Append(tick.Ask);
			sb.Append(';');
			sb.Append("2052=00/00/2010;"); //Unknown
			sb.Append("2005="); 
			sb.Append(tick.AskLevel(0));
			sb.Append(';');
			sb.Append("2053=00/00/2010;"); //Unknown
			sb.Append("2006=");
			sb.Append(tick.BidLevel(0));
			sb.Append(';');
			sb.Append("2007=");
			sb.Append(tick.Size);
			sb.Append(';');
			sb.Append("2008=0.0;"); // Yesterday Close
			sb.Append("2056=0.0;"); // Unknown
			sb.Append("2009=0.0;"); // High today
			sb.Append("2057=0;"); // Unknown
			sb.Append("2010=0.0"); // Low today
			sb.Append("2058=1;"); // Unknown
			sb.Append("2011=0.0;"); // Open Today
			sb.Append("2012=6828928;"); // Volume Today
			sb.Append("2013=20021;"); // Up/Down Tick
			sb.Append("2014="); // Time
			sb.Append(tick.Time.TimeOfDay);
			sb.Append(';');
			sb.Append("2015=");
			sb.Append(tick.Time.Month.ToString("00"));
			sb.Append('/');
			sb.Append(tick.Time.Day.ToString("00"));
			sb.Append('/');
			sb.Append(tick.Time.Year);
			sb.Append('\n');
			var message = sb.ToString();
			if( trace) log.Trace("Tick response: " + message);
			quoteWritePacket.DataOut.Write(message.ToCharArray());
			return Yield.DidWork.Invoke(WriteToQuotes);
		}
		
		private Yield CloseWithQuotesError(PacketMBTQuotes packet, string message) {
			return Yield.DidWork.Repeat;
		}
		
		private Yield CloseWithFixError(PacketFIX4_4 packet, string message) {
			fixWritePacket = fixSocket.CreatePacket();
			var fixMsg = new FIXMessage4_4(packet.Target,packet.Sender);
			TimeStamp timeStamp = TimeStamp.UtcNow;
			fixMsg.SetAccount(packet.Account);
			fixMsg.SetText( message);
			fixMsg.AddHeader("j");
			string errorMessage = fixMsg.ToString();
			fixWritePacket.DataOut.Write(errorMessage.ToCharArray());
			return Yield.DidWork.Invoke(WriteToFIX);
		}
	}
}
