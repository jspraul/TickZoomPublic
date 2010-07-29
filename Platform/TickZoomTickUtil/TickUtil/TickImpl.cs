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
using System.Text;
using System.Text.RegularExpressions;
using TickZoom.Api;

namespace TickZoom.TickUtil
{
	/// <summary>
	/// Description of TickDOM.
	/// </summary>
	/// <inheritdoc/>
	unsafe public struct TickImpl : TickIO
	{
		public const byte TickVersion = 8;
		public const int minTickSize = 256;
		
		public static long ToLong( double value) { return value.ToLong(); }
		public static double ToDouble( long value) { return value.ToDouble(); }
		public static double Round( double value) { return value.Round() ; }
		private static string TIMEFORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		
		// Older formats were already multiplied by 1000.
		public const long OlderFormatConvertToLong = 1000000;
		private static Log log = Factory.SysLog.GetLogger(typeof(TickImpl));
//		public static double UtcOffset = TimeStamp.UtcOffset;

		byte dataVersion;
		TickBinary binary;
		SymbolTimeZone timeZone;
		TimeStamp localTime;
		TimeStamp nextUtcOffsetUpdate;
		long utcOffset;

		public void Initialize() {
			binary = default(TickBinary);
			localTime = default(TimeStamp);
		}
		
		/// <inheritdoc/>
		public void SetTime(TimeStamp utcTime)
		{
			binary.UtcTime = utcTime.Internal;
			if( utcTime.Internal >= nextUtcOffsetUpdate.Internal) {
				if( timeZone == null) {
					if( binary.Symbol == 0) {
						throw new ApplicationException("Please call SetSymbol() prior to SetTime() method.");
					}
					SymbolInfo symbol = Factory.Symbol.LookupSymbol(binary.Symbol);
					timeZone = new SymbolTimeZone(symbol);
				}
				utcOffset = timeZone.UtcOffset(UtcTime);
				nextUtcOffsetUpdate = utcTime;
				int dayOfWeek = nextUtcOffsetUpdate.GetDayOfWeek();
				nextUtcOffsetUpdate.AddDays( 7 - dayOfWeek);
				nextUtcOffsetUpdate.SetDate(nextUtcOffsetUpdate.Year,nextUtcOffsetUpdate.Month,nextUtcOffsetUpdate.Day);
			}
			localTime = new TimeStamp(binary.UtcTime);
			localTime.AddSeconds(utcOffset);
		}
		
		public void SetQuote(double dBid, double dAsk)
		{
			SetQuote( dBid.ToLong(), dAsk.ToLong());
		}
		
		public void SetQuote(double dBid, double dAsk, ushort bidSize, ushort askSize)
		{
			try {
				SetQuote( dBid.ToLong(), dAsk.ToLong(), bidSize, askSize);
			} catch( OverflowException) {
				throw new ApplicationException("Overflow exception occurred when converting either bid: " + dBid + " or ask: " + dAsk + " to long.");
			}
		}
		
		public void SetQuote(long lBid, long lAsk) {
			IsQuote=true;
			binary.Bid = lBid;
			binary.Ask = lAsk;
		}
		
		public void SetQuote(long lBid, long lAsk, ushort bidSize, ushort askSize) {
			IsQuote=true;
			HasDepthOfMarket=true;
			binary.Bid = lBid;
			binary.Ask = lAsk;
			fixed( ushort *b = binary.DepthBidLevels)
			fixed( ushort *a = binary.DepthAskLevels) {
				*b = bidSize;
				*a = askSize;
			}
		}
		
		public void SetTrade(double price, int size)
		{
			SetTrade(TradeSide.Unknown,price.ToLong(),size);
		}
		
		public void SetTrade(TradeSide side, double price, int size)
		{
			SetTrade(side,price.ToLong(),size);
		}
		
		public void SetTrade(TradeSide side, long lPrice, int size) {
			IsTrade=true;
			binary.Side = (byte) side;
			binary.Price = lPrice;
			binary.Size = size;
		}
		
		public void SetDepth(ushort[] bidSize, ushort[] askSize)
		{
			HasDepthOfMarket = true;
			fixed( ushort *b = binary.DepthBidLevels)
			fixed( ushort *a = binary.DepthAskLevels) {
				for(int i=0;i<TickBinary.DomLevels;i++) {
					*(b+i) = bidSize[i];
					*(a+i) = askSize[i];
				}
			}
		}
		
		public void SetSymbol( ulong lSymbol) {
			binary.Symbol = lSymbol;
		}
		
		/// <summary>
		/// Obsolete: Please use Copy() instead.
		/// </summary>
		[Obsolete("Please use Copy() instead.",true)]
		public void init(TickIO tick) {
			Copy(tick);
		}
		
		public void Copy(TickIO tick) {
			if( tick is TickImpl) {
				this = (TickImpl) tick;
			} else {  
				Copy( tick, tick.ContentMask);
			}
		}
		
		/// <summary>
		/// Obsolete: Please use Copy() instead.
		/// </summary>
		[Obsolete("Please use Copy() instead.",true)]
		public void init(TickIO tick, byte contentMask) {
			Copy(tick,contentMask);
		}
		
		public void Copy(TickIO tick, byte contentMask) {
			bool dom = (contentMask & ContentBit.DepthOfMarket) != 0;
			bool simulateTicks = (contentMask & ContentBit.SimulateTicks) != 0;
			bool quote = (contentMask & ContentBit.Quote) != 0;
			bool trade = (contentMask & ContentBit.TimeAndSales) != 0;
			Initialize();
			SetSymbol(tick.lSymbol);
			SetTime(tick.UtcTime);
			IsSimulateTicks = simulateTicks;
			if( quote) {
				SetQuote(tick.lBid, tick.lAsk);
			}
			if( trade) {
				SetTrade(tick.Side, tick.lPrice, tick.Size);
			}
			if( dom) {
				fixed( ushort *b = binary.DepthBidLevels)
				fixed( ushort *a = binary.DepthAskLevels)
				for(int i=0;i<TickBinary.DomLevels;i++) {
					*(b+i) = tick.BidLevel(i);
					*(a+i) = tick.AskLevel(i);
				}
			}
			binary.ContentMask = contentMask;
			dataVersion = tick.DataVersion;
		}
		
		/// <summary>
		/// Use for setting quote data on the tick.
		/// </summary>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.",true)]
		public void init(TimeStamp utcTime, double dBid, double dAsk) {
			init( utcTime, dBid.ToLong(), dAsk.ToLong());
		}
		
		private void ClearContentMask() {
			binary.ContentMask = 0;
		}
		
		/// <summary>
		/// For internal use to set quote only data.
		/// </summary>
		/// <param name="utcTime"></param>
		/// <param name="lBid"></param>
		/// <param name="lAsk"></param>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.",true)]
		internal void init(TimeStamp utcTime, long lBid, long lAsk) {
			ClearContentMask();
			dataVersion = TickVersion;
			binary.UtcTime = utcTime.Internal;
			IsQuote=true;
			binary.Bid = lBid;
			binary.Ask = lAsk;
		}
		
		/// <summary>
		/// Use to set last trade data on the tick.
		/// </summary>
		/// <param name="utcTime"></param>
		/// <param name="price"></param>
		/// <param name="size"></param>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.",true)]
		public void init(TimeStamp utcTime, double price, int size) {
			init( utcTime, (byte) TradeSide.Unknown, price, size);
		}

		/// <summary>
		/// Sets the last trade data on the tick with the option
		/// of setting the "side". The side of the trade is only useful
		/// in advance data feed analysis to figure out which size of
		/// the Bid/Ask spread absorbed this trade.
		/// </summary>
		/// <param name="utcTime"></param>
		/// <param name="side"></param>
		/// <param name="price"></param>
		/// <param name="size"></param>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.",true)]
		public void init(TimeStamp utcTime, byte side, double price, int size) {
			init( utcTime, side, price.ToLong(), size);
		}
		
		/// <summary>
		/// For internal use in setting last trade data with side.
		/// </summary>
		/// <param name="utcTime"></param>
		/// <param name="side"></param>
		/// <param name="lPrice"></param>
		/// <param name="size"></param>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.",true)]
		internal void init(TimeStamp utcTime, byte side, long lPrice, int size) {
			ClearContentMask();
			IsTrade=true;
			binary.Side = side;
			binary.Price = lPrice;
			binary.Size = size;
		}
		
		/// <summary>
		/// Obsolete: Please use multiple init methods instead to set last trade and quote data in two method calls.
		/// </summary>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls.",true)]
		public void init(TimeStamp utcTime, byte side, double dPrice, int size, double dBid, double dAsk) {
			init(utcTime,side,dPrice.ToLong(),size,dBid.ToLong(),dAsk.ToLong());
		}

		/// <summary>
		/// Obsolete: For setting tick values internally.
		/// </summary>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.",true)]
		internal void init(TimeStamp utcTime, byte side, long lPrice, int size, long lBid, long lAsk) {
			init(utcTime,lBid,lAsk);
			ClearContentMask();
			IsQuote = true;
			IsTrade = true;
			binary.Side = side;
			binary.Price = lPrice;
			binary.Size = size;
		}

		/// <summary>
		/// Obsolete: Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.
		/// </summary>
		[Obsolete("Please use multiple init methods instead to set last trade and quote data in two method calls. This greatly simplifies the API.",true)]
		public void init(TimeStamp utcTime, byte side, double price, int size, double dBid, double dAsk, ushort[] bidSize, ushort[] askSize) {
			init(utcTime,dBid,dAsk);
			ClearContentMask();
			IsQuote = true;
			IsTrade = true;
			HasDepthOfMarket = true;
			binary.Side = side;
			binary.Price = price.ToLong();
			
			binary.Size = size;
			fixed( ushort *b = binary.DepthBidLevels) {
			fixed( ushort *a = binary.DepthAskLevels) {
				for(int i=0;i<TickBinary.DomLevels;i++) {
					*(b+i) = bidSize[i];;
					*(a+i) = askSize[i];;
				}
			}
			}
		}
		
		
		public int BidDepth {
			get { int total = 0;
				fixed( ushort *p = binary.DepthBidLevels) {
				    for(int i=0;i<TickBinary.DomLevels;i++) {
						total += *(p+i);
					}
				}
				return total;
			}
		}
		
		public int AskDepth {
			get { int total = 0;
				fixed( ushort *p = binary.DepthAskLevels) {
				 	for(int i=0;i<TickBinary.DomLevels;i++) {
						total += *(p+i);
					}
				}
				return total;
			}
		}
		
		public override string ToString() {
			string output = Time.ToString(TIMEFORMAT) + " " +
				(IsTrade ? (Side != TradeSide.Unknown ? Side.ToString() + "," : "") + Price.ToString(",0.000") + "," + binary.Size + ", " : "") +
				Bid.ToString(",0.000") + "/" + Ask.ToString(",0.000") + " ";
			fixed( ushort *p = binary.DepthBidLevels) {
				for(int i=TickBinary.DomLevels-1; i>=0; i--) {
					if( i!=TickBinary.DomLevels-1) { output += ","; }
					output += *(p + i);
				}
			}
			output += "|";
			fixed( ushort *p = binary.DepthAskLevels) {
				for(int i=0; i<TickBinary.DomLevels; i++) {
					if( i!=0) { output += ","; }
					output += *(p + i);
				}
			}
			return output;
		}
		
		
		unsafe public void ToWriter(MemoryStream writer) {
			dataVersion = TickVersion;
			writer.SetLength( writer.Position+minTickSize);
			byte[] buffer = writer.GetBuffer();
			fixed( byte *fptr = &buffer[writer.Position]) {
				byte *ptr = fptr;
				*(ptr) = dataVersion; ptr++;
				*(long*)(ptr) = binary.UtcTime; ptr+=sizeof(double);
				*(ptr) = binary.ContentMask; ptr++;
				if( IsQuote) {
					*(long*)(ptr) = binary.Bid; ptr += sizeof(long);
					*(long*)(ptr) = binary.Ask; ptr += sizeof(long);
				}
				if( IsTrade) {
					*ptr = binary.Side; ptr ++;
					*(long*)(ptr) = binary.Price; ptr += sizeof(long);
					*(int*)(ptr) = binary.Size; ptr += sizeof(int);
				}
				if( HasDepthOfMarket ) {
					fixed( ushort *p = binary.DepthBidLevels) {
						for( int i=0; i<TickBinary.DomLevels; i++) {
							*(ushort*)(ptr) = *(p + i); ptr+=sizeof(ushort);
						}
					}
					fixed( ushort *p = binary.DepthAskLevels) {
						for( int i=0; i<TickBinary.DomLevels; i++) {
							*(ushort*)(ptr) = *(p + i); ptr+=sizeof(ushort);
						}
					}
				}
				writer.Position += ptr - fptr;
				writer.SetLength(writer.Position);
			}
		}
		
		private unsafe int FromFileVersion8(byte *fptr) {
			byte *ptr = fptr;
	    	binary.UtcTime = *(long*)ptr; ptr+=sizeof(double);
			binary.ContentMask = *ptr; ptr++;
			if( IsQuote ) {
				binary.Bid = * (long*) ptr; ptr+=sizeof(long);
				binary.Ask = * (long*) ptr; ptr+=sizeof(long);
			}
			if( IsTrade) {
				binary.Side = *ptr; ptr++;
				binary.Price = * (long*) ptr; ptr+=sizeof(long);
				binary.Size = * (int*) ptr; ptr+=sizeof(int);
			}
			if( HasDepthOfMarket) {
				fixed( ushort *p = binary.DepthBidLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = * (ushort*) ptr; ptr += 2;
					}
				}
				fixed( ushort *p = binary.DepthAskLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = * (ushort*) ptr; ptr += 2;
					}
				}
			}
			int len = (int) (ptr - fptr);
			return len;
		}

		private int FromFileVersion7(BinaryReader reader) {
			int position = 0;
			double d = reader.ReadDouble(); position += 8;
//			if( d < 0) {
//				int x = 0;
//			}
			binary.UtcTime = new TimeStamp(d).Internal;
//			if( binary.UtcTime.ToString().Contains("Error")) {
//				int x = 0;
//			}
			binary.ContentMask = reader.ReadByte(); position += 1;
			if( IsQuote ) {
				binary.Bid = reader.ReadInt64(); position += 8;
				binary.Ask = reader.ReadInt64(); position += 8;
				if( !IsTrade) {
					binary.Price = (binary.Bid+binary.Ask)/2;
				}
			}
			if( IsTrade) {
				binary.Side = reader.ReadByte(); position += 1;
				binary.Price = reader.ReadInt64(); position += 8;
				binary.Size = reader.ReadInt32(); position += 4;
				if( binary.Price == 0) {
					binary.Price = (binary.Bid+binary.Ask)/2;
				}
				if( !IsQuote) {
					binary.Bid = binary.Ask = binary.Price;
				}
			}
			if( HasDepthOfMarket) {
				fixed( ushort *p = binary.DepthBidLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
				fixed( ushort *p = binary.DepthAskLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
			}
			return position;
		}
		
		private int FromFileVersion6(BinaryReader reader) {
			int position = 0;
			binary.UtcTime = new TimeStamp(reader.ReadDouble()).Internal; position += 8;
			binary.Bid = reader.ReadInt64(); position += 8;
			binary.Ask = reader.ReadInt64(); position += 8;
			ClearContentMask();
			IsQuote = true;
			bool dom = reader.ReadBoolean(); position += 1;
			if( dom) {
				IsTrade = true;
				HasDepthOfMarket = true;
				binary.Side = reader.ReadByte(); position += 1;
				binary.Price = reader.ReadInt64(); position += 8;
				if( binary.Price == 0) { binary.Price = (binary.Bid+binary.Ask)/2; }
				binary.Size = reader.ReadInt32(); position += 4;
				fixed( ushort *p = binary.DepthBidLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
				fixed( ushort *p = binary.DepthAskLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
			}
			return position;
		}

		private int FromFileVersion5(BinaryReader reader) {
			int position = 0;
			binary.UtcTime = new TimeStamp(reader.ReadDouble()).Internal; position += 8;
			binary.Bid = reader.ReadInt32(); position += 4;
			sbyte spread = reader.ReadSByte();	position += 1;
			binary.Ask = binary.Bid + spread;
			binary.Bid*=OlderFormatConvertToLong;
			binary.Ask*=OlderFormatConvertToLong;
			ClearContentMask();
			IsQuote = true;
			bool hasDOM = reader.ReadBoolean(); position += 1;
			if( hasDOM) {
				IsTrade = true;
				HasDepthOfMarket = true;
				binary.Price = reader.ReadInt32(); position += 4;
				binary.Price*=OlderFormatConvertToLong;
				if( binary.Price == 0) { binary.Price = (binary.Bid+binary.Ask)/2; }
				binary.Size = reader.ReadUInt16(); position += 2;
				fixed( ushort *p = binary.DepthBidLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
				fixed( ushort *p = binary.DepthAskLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
			}
			return position;
		}

		private int FromFileVersion4(BinaryReader reader) {
			int position = 0;
			reader.ReadByte(); position += 1;
 			// throw away symbol
			for( int i=0; i<TickBinary.SymbolSize; i++) {
				reader.ReadChar(); position += 2;
			}
 			binary.UtcTime = new TimeStamp(reader.ReadDouble()).Internal; position += 8;
			binary.Bid = reader.ReadInt32(); position += 4;
			sbyte spread = reader.ReadSByte();	position += 1;
			binary.Ask = binary.Bid + spread;
			binary.Bid*=OlderFormatConvertToLong;
			binary.Ask*=OlderFormatConvertToLong;
			ClearContentMask();
			IsQuote = true;
			bool hasDOM = reader.ReadBoolean(); position += 1;
			if( hasDOM) {
				IsTrade = true;
				HasDepthOfMarket = true;
				binary.Side = reader.ReadByte(); position += 1;
				binary.Price = reader.ReadInt32(); position += 4;
				binary.Price*=OlderFormatConvertToLong;
				if( binary.Price == 0) { binary.Price = (binary.Bid+binary.Ask)/2; }
				binary.Size = reader.ReadUInt16(); position += 2;
				fixed( ushort *p = binary.DepthBidLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
				fixed( ushort *p = binary.DepthAskLevels) {
					for( int i=0; i<TickBinary.DomLevels; i++) {
						*(p+i) = reader.ReadUInt16(); position += 2;
					}
				}
			}
			return position;
		}

		private int FromFileVersion3(BinaryReader reader) {
			int position = 0;
			DateTime tickTime = DateTime.FromBinary(reader.ReadInt64()); position += 8;
			binary.UtcTime = new TimeStamp(tickTime.ToLocalTime()).Internal;
			binary.Bid = reader.ReadInt32(); position += 4;
			sbyte spread = reader.ReadSByte();	position += 1;
			binary.Ask = binary.Bid+spread;
			binary.Bid*=OlderFormatConvertToLong;
			binary.Ask*=OlderFormatConvertToLong;
			binary.Side = reader.ReadByte(); position += 1;
			binary.Price = reader.ReadInt32(); position += 4;
			binary.Price*=OlderFormatConvertToLong;
			if( binary.Price == 0) { binary.Price = (binary.Bid+binary.Ask)/2; }
			binary.Size = reader.ReadUInt16(); position += 2;
			fixed( ushort *p = binary.DepthBidLevels) {
				for( int i=0; i<TickBinary.DomLevels; i++) {
					*(p+i) = reader.ReadUInt16(); position += 2;
				}
			}
			fixed( ushort *p = binary.DepthAskLevels) {
				for( int i=0; i<TickBinary.DomLevels; i++) {
					*(p+i) = reader.ReadUInt16(); position += 2;
				}
			}
			ClearContentMask();
			IsQuote = true;
			IsTrade = true;
			HasDepthOfMarket = true;
			return position;
		}
		
		private int FromFileVersion2(BinaryReader reader) {
			int position = 0;
			DateTime tickTime = DateTime.FromBinary(reader.ReadInt64()); position += 8;
			binary.UtcTime = new TimeStamp(tickTime.ToLocalTime()).Internal;
			binary.Bid = reader.ReadInt32(); position += 4;
			sbyte spread = reader.ReadSByte();	position += 1;
			binary.Ask = binary.Bid+spread;
			binary.Bid*=OlderFormatConvertToLong;
			binary.Ask*=OlderFormatConvertToLong;
			fixed( ushort *p = binary.DepthBidLevels) {
				*p = (ushort) reader.ReadInt32(); position += 4;
			}
			fixed( ushort *p = binary.DepthAskLevels) {
				*p = (ushort) reader.ReadInt32(); position += 4;
			}
			ClearContentMask();
			IsQuote = true;
			HasDepthOfMarket = true;
			binary.Side = (byte) TradeSide.Unknown;
			binary.Price = (binary.Bid+binary.Ask)/2;
			binary.Size = 0;
			return position;
		}
		
		private int FromFileVersion1(BinaryReader reader) {
			int position = 0;
			
			long int64 = reader.ReadInt64() ^ -9223372036854775808L;
			DateTime tickTime = DateTime.FromBinary(int64); position += 8;
			TimeStamp timeStamp = (TimeStamp) tickTime.AddHours(-4);
			binary.UtcTime = timeStamp.Internal;
			
			binary.Bid = reader.ReadInt32(); position += 4;
			sbyte spread = reader.ReadSByte();	position += 1;
			binary.Ask = binary.Bid+spread;
			binary.Bid*=OlderFormatConvertToLong;
			binary.Ask*=OlderFormatConvertToLong;
			ClearContentMask();
			IsQuote = true;
			binary.Price = (binary.Bid+binary.Ask)/2;
			return position;
		}
		
		public void FromReader(MemoryStream reader) {
			binary = default(TickBinary);
			fixed( byte *fptr = reader.GetBuffer()) {
				byte *sptr = fptr + reader.Position;
				byte *ptr = sptr;
				dataVersion = *ptr; ptr ++;
				switch( dataVersion) {
					case 8:
						ptr += FromFileVersion8(ptr);
						break;
					default:
						throw new ApplicationException("Unknown Tick Version Number " + dataVersion);
				}
				reader.Position += (int) (ptr - sptr);
			}
		}

		/// <summary>
		/// Old style FormatReader for legacy versions of TickZoom tck
		/// data files.
		/// </summary>
		public int FromReader(byte dataVersion, BinaryReader reader) {
			binary = default(TickBinary);
			this.dataVersion = dataVersion;
			int position = 0;
			switch( dataVersion) {
				case 1:
					position += FromFileVersion1(reader);
					break;
				case 2:
					position += FromFileVersion2(reader);
					break;
				case 3:
					position += FromFileVersion3(reader);
					break;
				case 4:
					position += FromFileVersion4(reader);
					break;
				case 5:
					position += FromFileVersion5(reader);
					break;
				case 6:
					position += FromFileVersion6(reader);
					break;
				case 7:
					position += FromFileVersion7(reader);
					break;
				default:
					throw new ApplicationException("Unknown Tick Version Number " + dataVersion);
			}
			return position;
		}
		
		public bool memcmp(ushort* array1, ushort* array2) {
			for( int i=0; i<TickBinary.DomLevels; i++) {
				if( *(array1+i) != *(array2+i)) return false;
			}
			return true;
		}
		
		public int CompareTo(ref TickImpl other)
		{
			fixed( ushort*a1 = binary.DepthAskLevels) {
			fixed( ushort*a2 = other.binary.DepthAskLevels) {
			fixed( ushort*b1 = binary.DepthBidLevels) {
			fixed( ushort*b2 = other.binary.DepthBidLevels) {
				return binary.ContentMask == other.binary.ContentMask &&
					binary.UtcTime == other.binary.UtcTime &&
					binary.Bid == other.binary.Bid &&
					binary.Ask == other.binary.Ask &&
					binary.Side == other.binary.Side &&
					binary.Price == other.binary.Price &&
					binary.Size == other.binary.Size &&
					memcmp( a1, a2) &&
					memcmp( b1, b2) ? 0 :
					binary.UtcTime > other.binary.UtcTime ? 1 : -1;
				}
			}
			}
			}
		}
		
		public byte DataVersion {
			get { return dataVersion; }
		}
		
		public double Bid {
			get { return binary.Bid.ToDouble(); }
		}
		
		public double Ask {
			get { return binary.Ask.ToDouble(); }
		}
		
		public TradeSide Side {
			get { return (TradeSide) binary.Side; }
		}
		
		public double Price {
			get {
				if( IsTrade) {
					return binary.Price.ToDouble();
				} else {
					string msg = "Sorry. The Price property on a tick can only by accessed\n" +
					             "if it has trade data. Please, check the IsTrade property.";
					log.Error("msg");
					throw new ApplicationException(msg);
				}
			}
		}
		
		public int Size {
			get { return binary.Size; }
		}
		
		public int Volume {
			get { return Size; }
		}
		
		public ushort AskLevel(int level) {
			fixed( ushort *p = binary.DepthAskLevels) {
				return *(p+level);
			}
		}
		
		public ushort BidLevel(int level) {
			fixed( ushort *p = binary.DepthBidLevels) {
				return *(p+level);
			}
		}
		
		public TimeStamp Time {
			get { return localTime; }
		}
		
		public TimeStamp UTCTime {
			get { return new TimeStamp(binary.UtcTime); }
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override bool Equals(object obj)
		{
			TickImpl other = (TickImpl) obj;
			return CompareTo(ref other) == 0;
		}
		
		public bool Equals(TickImpl other)
		{
			return CompareTo(ref other) == 0;
		}
		
		public byte ContentMask {
			get { return binary.ContentMask; }
		}
		
		public long lBid {
			get { return binary.Bid; }
		}
		public long lAsk {
			get { return binary.Ask; }
		}
		
		public long lPrice {
			get { return binary.Price; }
		}
		
		public TimeStamp UtcTime {
			get { return new TimeStamp(binary.UtcTime); }
		}

		public ulong lSymbol {
			get { return binary.Symbol; }
		}
		
		public string Symbol {
			get { return binary.Symbol.ToSymbol(); }
		}
		
		public int DomLevels {
			get { return TickBinary.DomLevels; }
		}
		
		public bool IsQuote {
			get { return (binary.ContentMask & ContentBit.Quote) > 0; }
			set {
				if( value ) {
					binary.ContentMask |= ContentBit.Quote;
				} else {
					binary.ContentMask &= ContentBit.Quote;
				}
			}
		}
		
		public bool IsSimulateTicks {
			get { return (binary.ContentMask & ContentBit.SimulateTicks) > 0; }
			set {
				if( value ) {
					binary.ContentMask |= ContentBit.SimulateTicks;
				} else {
					binary.ContentMask &= ContentBit.SimulateTicks;
				}
			}
		}
		
		public bool IsTrade {
			get { return (binary.ContentMask & ContentBit.TimeAndSales) > 0; }
			set {
				if( value ) {
					binary.ContentMask |= ContentBit.TimeAndSales;
				} else {
					binary.ContentMask &= ContentBit.TimeAndSales;
				}
			}
		}
		
		public bool HasDepthOfMarket {
			get { return (binary.ContentMask & ContentBit.DepthOfMarket) > 0; }
			set {
				if( value ) {
					binary.ContentMask |= ContentBit.DepthOfMarket;
				} else {
					binary.ContentMask &= ContentBit.DepthOfMarket;
				}
			}
		}
		
		public object ToPosition() {
			return new TimeStamp(binary.UtcTime).ToString();
		}
		
		#if DEBUG
		public ushort[] DebugBidDepth {
			get { ushort[] depth = new ushort[TickBinary.DomLevels];
				fixed( ushort *a = this.binary.DepthBidLevels) {
					for( int i= 0; i<TickBinary.DomLevels; i++) {
						depth[i] = *(a+i);
					}
				}
				return depth;
			}
		}
		public ushort[] DebugAskDepth {
			get { ushort[] depth = new ushort[TickBinary.DomLevels];
				fixed( ushort *a = this.binary.DepthAskLevels) {
					for( int i= 0; i<TickBinary.DomLevels; i++) {
						depth[i] = *(a+i);
					}
				}
				return depth;
			}
		}
		#endif
		
		public int Sentiment {
			get { return 0; }
		}
		
		public TickBinary Extract()
		{
			return binary;
		}

		public void Inject(TickBinary tick) {
			binary = tick;
			SetTime( new TimeStamp(binary.UtcTime));
//			string x = ToString();
		}
		
		public bool IsRealTime {
			get { return false; }
			set { }
		}
	}
}
