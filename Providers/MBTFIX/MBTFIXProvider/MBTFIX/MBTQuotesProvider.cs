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
using System.Security.Cryptography;

using TickZoom.Api;

namespace TickZoom.MBTQuotes
{
	[SkipDynamicLoad]
	public class MBTQuotesProvider : MBTQuoteProviderSupport
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(MBTQuotesProvider));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
        private Dictionary<long,SymbolHandler> symbolHandlers = new Dictionary<long,SymbolHandler>();	
		
		public MBTQuotesProvider(string name)
		{
			ProviderName = "MBTQuotesProvider";
			if( name.Contains(".config")) {
				throw new ApplicationException("Please remove .config from config section name.");
			}
			ConfigName = name;
			RetryStart = 1;
			RetryIncrease = 1;
			RetryMaximum = 30;
  			HeartbeatDelay = 15;
        }
		
		public override void PositionChange(Receiver receiver, SymbolInfo symbol, double signal, Iterable<LogicalOrder> orders)
		{
		}
		
		public override void OnDisconnect()
		{
		}
		
		public override void OnRetry()
		{
		}
        
		public override Yield OnLogin()
		{
			Socket.PacketFactory = new PacketFactoryMBTQuotes();
			
			Packet packet = Socket.CreatePacket();
			string hashPassword = Hash(Password);
			string login = "L|100="+UserName+";133="+hashPassword+"\n";
			if( trace) log.Trace( "Sending: " + login);
			packet.DataOut.Write(login.ToCharArray());
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return Yield.NoWork.Repeat;
				Factory.Parallel.Yield();
			}
			while( !Socket.TryGetPacket(out packet)) {
				if( IsInterrupted) return Yield.NoWork.Repeat;
				Factory.Parallel.Yield();
			}
			packet.BeforeRead();
			char firstChar = (char) packet.Data.GetBuffer()[packet.Data.Position];
			if( firstChar != 'G') {
				throw new ApplicationException("Invalid quotes login response: \n" + new string(packet.DataIn.ReadChars(packet.Remaining)));
			}
			if( trace) log.Trace( "Response: " + new string(packet.DataIn.ReadChars(packet.Remaining)));
			StartRecovery();
			return Yield.DidWork.Repeat;
        }
		
		protected override void OnStartRecovery()
		{
			SendStartRealTime();
			EndRecovery();
		}
		
		protected override Yield ReceiveMessage()
		{
			Packet packet;
			if(Socket.TryGetPacket(out packet)) {
				if( trace) log.Trace("Response: " + new string(packet.DataIn.ReadChars(packet.Remaining)));
//				log.Info("Response: " + new string(packet.DataIn.ReadChars(packet.Remaining)));
				packet.BeforeRead();
				while( packet.Remaining > 0) {
					char firstChar = (char) packet.Data.GetBuffer()[packet.Data.Position];
					switch( firstChar) {
						case '1':
							Level1Update( (PacketMBTQuotes) packet);
							break;
						case '2':
							log.Error( "Message type '2' unknown packet is: " + packet);
							break;
						case '3':
							TimeAndSalesUpdate( (PacketMBTQuotes) packet);
							break;
						default:
							throw new ApplicationException("MBTQuotes message type '" + firstChar + "' was unknown: \n" + new string(packet.DataIn.ReadChars(packet.Remaining)));
					}
				}
				return Yield.DidWork.Repeat;
			} else {
				return Yield.NoWork.Repeat;
			}
		}
		
		private unsafe void Level1Update( PacketMBTQuotes packet) {
			SymbolHandler handler = null;
			MemoryStream data = packet.Data;
			data.Position += 2;
			fixed( byte *bptr = data.GetBuffer()) {
				byte *ptr = bptr + data.Position;
				while( ptr - bptr < data.Length) {
					int key = packet.GetKey(ref ptr);
					switch( key) {
						case 1003: // Symbol
							string symbol = packet.GetString( ref ptr);
							SymbolInfo symbolInfo = Factory.Symbol.LookupSymbol(symbol);
							handler = symbolHandlers[symbolInfo.BinaryIdentifier];
							break;
						case 2003: // Bid
							handler.Bid = packet.GetDouble(ref ptr);
							break;
						case 2004: // Ask
							handler.Ask = packet.GetDouble(ref ptr);
							break;
						case 2005: // Bid Size
							handler.AskSize = packet.GetInt(ref ptr);
							break;
						case 2006: // Ask Size
							handler.BidSize = packet.GetInt(ref ptr);
							break;
						default:
							packet.SkipValue(ref ptr);
							break;
					}
					if( *(ptr-1) == 10) {
						handler.SendQuote();
						data.Position ++;
						return;
					}
				}
			}
			throw new ApplicationException("Expected Level 1 Quote to end with new line character. Packet:\n" + packet);
		}
		
		private unsafe void TimeAndSalesUpdate( PacketMBTQuotes packet) {
			SymbolHandler handler = null;
			MemoryStream data = packet.Data;
			data.Position += 2;
			fixed( byte *bptr = data.GetBuffer()) {
				byte *ptr = bptr + data.Position;
				while( ptr - bptr < data.Length) {
					int key = packet.GetKey(ref ptr);
					switch( key) {
						case 1003: // Symbol
							string symbol = packet.GetString( ref ptr);
							SymbolInfo symbolInfo = Factory.Symbol.LookupSymbol(symbol);
							handler = symbolHandlers[symbolInfo.BinaryIdentifier];
							break;
						case 2002: // Last Trade Price
							handler.Last = packet.GetDouble(ref ptr);
							if( trace) {
								log.Trace( "Got last trade price: " + handler.Last);// + "\n" + packet);
							}
							break;
						case 2007: // Last Trade Size
							handler.LastSize = packet.GetInt(ref ptr);
							break;
						case 2082: // Condition
							int condition = packet.GetInt(ref ptr);
							if( condition != 0 &&
							    condition != 53 &&
							    condition != 45) {
								log.Info( "Trade quote received with non-zero condition: " + condition);
							}
							break;
						case 2083: // Status
							int status = packet.GetInt(ref ptr);
							if( status != 0) {
								log.Info( "Trade quote received with non-zero status: " + status);
							}
							break;
						case 2084: // Type
							int type = packet.GetInt(ref ptr);
							if( type != 0) {
								log.Info( "Trade quote received with non-zero type: " + type);
							}
							break;
						default:
							packet.SkipValue(ref ptr);
							break;
					}
					if( *(ptr-1) == 10) {
						handler.SendTimeAndSales();
						data.Position ++;
						return;
					}
				}
			}
			throw new ApplicationException("Expected Trade Quote to end with new line character.");
		}
		
		private void OnException( Exception ex) {
			log.Error("Exception occurred", ex);
		}
        
		private void SendStartRealTime() {
			lock( symbolsRequestedLocker) {
				foreach( var kvp in symbolsRequested) {
					SymbolInfo symbol = kvp.Value;
					RequestStartSymbol(symbol);
				}
			}
		}
		
		private void SendEndRealTime() {
			lock( symbolsRequestedLocker) {
				foreach(var kvp in symbolsRequested) {
					SymbolInfo symbol = kvp.Value;
					RequestStopSymbol(symbol);
				}
			}
		}
		
		public override void OnStartSymbol(SymbolInfo symbol)
		{
			if( IsRecovered) {
				RequestStartSymbol(symbol);
			}
		}
		
		private void RequestStartSymbol(SymbolInfo symbol) {
			string quoteType = "";
			switch( symbol.QuoteType) {
				case QuoteType.Level1:
					quoteType = "20000";
					break;
				case QuoteType.Level2:
					quoteType = "20001";
					break;
				case QuoteType.None:
					quoteType = null;
					break;
				default:
					SendError("Unknown QuoteType " + symbol.QuoteType + " for symbol " + symbol + ".");
					return;
			}
			
			string tradeType = "";
			switch( symbol.TimeAndSales) {
				case TimeAndSales.ActualTrades:
					tradeType = "20003";
					break;
				case TimeAndSales.Extrapolated:
					tradeType = null;
					break;
				case TimeAndSales.None:
					tradeType = null;
					break;
				default:
					SendError("Unknown TimeAndSales " + symbol.TimeAndSales + " for symbol " + symbol + ".");
					return;
			}

			if( tradeType != null) {
				Packet packet = Socket.CreatePacket();
				string message = "S|1003="+symbol.Symbol+";2000="+tradeType+"\n";
				if( debug) log.Debug("Symbol request: " + message);
				packet.DataOut.Write(message.ToCharArray());
				while( !Socket.TrySendPacket(packet)) {
					if( IsInterrupted) return;
					Factory.Parallel.Yield();
				}
			}
			
			if( quoteType != null) {
				Packet packet = Socket.CreatePacket();
				string message = "S|1003="+symbol.Symbol+";2000="+quoteType+"\n";
				if( debug) log.Debug("Symbol request: " + message);
				packet.DataOut.Write(message.ToCharArray());
				while( !Socket.TrySendPacket(packet)) {
					if( IsInterrupted) return;
					Factory.Parallel.Yield();
				}
			}
			
            SymbolHandler handler = GetSymbolHandler(symbol,receiver);
            while( !receiver.OnEvent(symbol,(int)EventType.StartRealTime,null)) {
            	if( IsInterrupted) return;
            	Factory.Parallel.Yield();
            }
		}
		
		public override void OnStopSymbol(SymbolInfo symbol)
		{
			RequestStopSymbol(symbol);
		}
		
		private void RequestStopSymbol(SymbolInfo symbol) {
       		SymbolHandler buffer = symbolHandlers[symbol.BinaryIdentifier];
       		buffer.Stop();
			receiver.OnEvent(symbol,(int)EventType.EndRealTime,null);
		}
		
		
		
        private SymbolHandler GetSymbolHandler(SymbolInfo symbol, Receiver receiver) {
        	SymbolHandler symbolHandler;
        	if( symbolHandlers.TryGetValue(symbol.BinaryIdentifier,out symbolHandler)) {
        		symbolHandler.Start();
        		return symbolHandler;
        	} else {
    	    	symbolHandler = Factory.Utility.SymbolHandler(symbol,receiver);
    	    	symbolHandlers.Add(symbol.BinaryIdentifier,symbolHandler);
    	    	symbolHandler.Start();
    	    	return symbolHandler;
        	}
        }

        private void RemoveSymbolHandler(SymbolInfo symbol) {
        	if( symbolHandlers.ContainsKey(symbol.BinaryIdentifier) ) {
        		symbolHandlers.Remove(symbol.BinaryIdentifier);
        	}
        }
        
		public static string Hash(string password) {
			SHA256 hash = new SHA256Managed();
			char[] chars = password.ToCharArray();
			byte[] bytes = new byte[chars.Length];
			for( int i=0; i<chars.Length; i++) {
				bytes[i] = (byte) chars[i];
			}
			byte[] hashBytes = hash.ComputeHash(bytes);
			string hashString = BitConverter.ToString(hashBytes);
			return hashString.Replace("-","");
		}
		
	}
}
