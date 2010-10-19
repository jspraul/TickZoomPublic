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
		public const int minTickSize = 256;
		
		public static long ToLong( double value) { return value.ToLong(); }
		public static double ToDouble( long value) { return value.ToDouble(); }
		public static double Round( double value) { return value.Round() ; }
		private static string TIMEFORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		
		// Older formats were already multiplied by 1000.
		public const long OlderFormatConvertToLong = 1000000;
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TickImpl));
		private static readonly bool trace = log.IsTraceEnabled;
		private static readonly bool debug = log.IsDebugEnabled;
		private bool isCompressStarted;
		private long minimumTick;

		byte dataVersion;
		TickBinary binary;
		TickBinary lastBinary;
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
		
		public void SetQuote(double dBid, double dAsk, short bidSize, short askSize)
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
		
		public void SetQuote(long lBid, long lAsk, short bidSize, short askSize) {
			IsQuote=true;
			HasDepthOfMarket=true;
			binary.Bid = lBid;
			binary.Ask = lAsk;
			fixed( ushort *b = binary.DepthBidLevels)
			fixed( ushort *a = binary.DepthAskLevels) {
				*b = (ushort) bidSize;
				*a = (ushort) askSize;
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
		
		public void SetDepth(short[] bidSize, short[] askSize)
		{
			HasDepthOfMarket = true;
			fixed( ushort *b = binary.DepthBidLevels)
			fixed( ushort *a = binary.DepthAskLevels) {
				for(int i=0;i<TickBinary.DomLevels;i++) {
					*(b+i) = (ushort) bidSize[i];
					*(a+i) = (ushort) askSize[i];
				}
			}
		}
		
		public void SetSymbol( long lSymbol) {
			binary.Symbol = lSymbol;
		}
		
		public void Copy(TickIO tick) {
			if( tick is TickImpl) {
				this = (TickImpl) tick;
			} else {  
				Copy( tick, tick.ContentMask);
			}
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
					*(b+i) = (ushort) tick.BidLevel(i);
					*(a+i) = (ushort) tick.AskLevel(i);
				}
			}
			binary.ContentMask = contentMask;
			dataVersion = tick.DataVersion;
		}
		
		private void ClearContentMask() {
			binary.ContentMask = 0;
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
		
		public enum BinaryField {
			Time=1,
			Bid,
			Ask,
			Price,
			Size,
			BidSize,
			AskSize,
			ContentMask,
			Reset=30,
			Empty=31
		}
		public enum FieldSize {
			Byte=1,
			Short,
			Int,
			Long,
		}
		
		private unsafe bool WriteField( BinaryField fieldEnum, byte** ptr, long diff) {
			var field = (byte) ((byte) fieldEnum << 3);
			if( diff == 0) {
				return false;
			} else if( diff >= byte.MinValue && diff <= byte.MaxValue) {
				field |= (byte) FieldSize.Byte;
				*(*ptr) = field; (*ptr)++;
				*(*ptr) = (byte) diff; (*ptr)++;
			} else if( diff >= short.MinValue && diff <= short.MaxValue) {
				field |= (byte) FieldSize.Short;
				*(*ptr) = field; (*ptr)++;
				*(short*)(*ptr) = (short) diff; (*ptr)+=sizeof(short);
			} else if( diff >= int.MinValue && diff <= int.MaxValue) {
				field |= (byte) FieldSize.Int;
				*(*ptr) = field; (*ptr)++;
				*(int*)(*ptr) = (int) diff; (*ptr)+=sizeof(int);
			} else {
				field |= (byte) FieldSize.Long;
				*(*ptr) = field; (*ptr)++;
				*(long*)(*ptr) = diff; (*ptr)+=sizeof(long);
			}
			return true;
		}
		
		private unsafe void WriteBidSize(byte field, int i, byte** ptr) {
			fixed( ushort *lp = lastBinary.DepthBidLevels)
			fixed( ushort *p = binary.DepthBidLevels) {
				var diff = *(p + i) - *(lp + i);
				if( diff != 0) {
					*(*ptr) = (byte) (field | i); (*ptr)++;
					*(short*)(*ptr) = (short) diff; (*ptr)+=sizeof(short);
				}
			}
		}
		
		private unsafe void WriteAskSize(byte field, int i, byte** ptr) {
			fixed( ushort *lp = lastBinary.DepthAskLevels)
			fixed( ushort *p = binary.DepthAskLevels) {
				var diff = *(p + i) - *(lp + i);
				if( diff != 0) {
					*(*ptr) = (byte) (field | i); (*ptr)++;
					*(short*)(*ptr) = (short) diff; (*ptr)+=sizeof(short);
				}
			}
		}
		private unsafe void ToWriterVersion9(MemoryStream writer) {
			dataVersion = 9;
			writer.SetLength( writer.Position+minTickSize);
			byte[] buffer = writer.GetBuffer();
			fixed( byte *fptr = &buffer[writer.Position]) {
				byte *ptr = fptr;
				ptr++; // Save space for size header.
				*(ptr) = dataVersion; ptr++;
				ptr++; // Save space for checksum.
				if( minimumTick == 0L) {
					var symbol = Factory.Symbol.LookupSymbol(binary.Symbol);
					minimumTick = Math.Max(1,symbol.MinimumTick.ToLong());
				}
				if( !isCompressStarted) {
					if( debug) log.Debug("Writing Reset token during tick compression.");
					WriteField( BinaryField.Reset, &ptr, 1);
					var ts = new TimeStamp( binary.UtcTime);
					isCompressStarted = true;
				}
				WriteField( BinaryField.ContentMask, &ptr, binary.ContentMask - lastBinary.ContentMask);
				WriteField( BinaryField.Time, &ptr, binary.UtcTime - lastBinary.UtcTime);
				if( IsQuote) {
					WriteField( BinaryField.Bid, &ptr, (binary.Bid - lastBinary.Bid) / minimumTick);
					WriteField( BinaryField.Ask, &ptr, (binary.Ask - lastBinary.Ask) / minimumTick);
				}
				if( IsTrade) {
					WriteField( BinaryField.Price, &ptr, (binary.Price - lastBinary.Price) / minimumTick);
					WriteField( BinaryField.Size, &ptr, binary.Size - lastBinary.Size);
				}
				if( HasDepthOfMarket) {
					var field = (byte) ((byte) BinaryField.BidSize << 3);
					for( int i=0; i<TickBinary.DomLevels; i++) {
						WriteBidSize( field, i, &ptr);
					}
					field = (byte) ((byte) BinaryField.AskSize << 3);
					for( int i=0; i<TickBinary.DomLevels; i++) {
						WriteAskSize( field, i, &ptr);
					}
				}
				writer.Position += ptr - fptr;
				writer.SetLength(writer.Position);
				*fptr = (byte) (ptr - fptr);
				byte checksum = 0;
				for( var p = fptr+3; p < ptr; p++) {
					checksum ^= *p;	
				}
				*(fptr+2) = (byte) (checksum ^ lastChecksum);
				lastChecksum = checksum;
				lastBinary = binary;
//				log.Info("Length = " + (ptr - fptr));
			}
		}
		private byte lastChecksum;
		
		public unsafe void ToWriter(MemoryStream writer) {
			ToWriterVersion9(writer);
//			ToWriterVersion8(writer);
		}
		
		public unsafe void ToWriterVersion8(MemoryStream writer) {
			dataVersion = 8;
			writer.SetLength( writer.Position+minTickSize);
			byte[] buffer = writer.GetBuffer();
			fixed( byte *fptr = &buffer[writer.Position]) {
				byte *ptr = fptr;
				ptr++; // Save space for size header.
				*(ptr) = dataVersion; ptr++;
				*(long*)(ptr) = binary.UtcTime; ptr+=sizeof(long);
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
				*fptr = (byte) (ptr - fptr);
			}
		}
		
		private unsafe long ReadField(byte** ptr) {
			long result = 0L;
			var size = (FieldSize) (**ptr & 0x07);
			(*ptr)++;
			switch( size) {
				case FieldSize.Byte:
					result = (**ptr); (*ptr)++;
					break;
				case FieldSize.Short:
					result = (*(short*)(*ptr)); (*ptr)+= sizeof(short);
					break;
				case FieldSize.Int:
					result = (*(int*)(*ptr)); (*ptr)+= sizeof(int);
					break;
				case FieldSize.Long:
					result = (*(long*)(*ptr)); (*ptr)+= sizeof(long);
					break;
			}
			return result;
		}
		
		private unsafe void ReadBidSize(byte** ptr) {
			fixed( ushort *p = binary.DepthBidLevels) {
				var index = **ptr & 0x07;
				(*ptr)++;
				*(p+index) = (ushort) (*(p+index) + *(short*)(*ptr));
				(*ptr)+= sizeof(short);
			}
		}
		
		private unsafe void ReadAskSize(byte** ptr) {
			fixed( ushort *p = binary.DepthAskLevels) {
				var index = **ptr & 0x07;
				(*ptr)++;
				*(p+index) = (ushort) (*(p+index) + *(short*)(*ptr));
				(*ptr)+= sizeof(short);
			}
		}
		
		private unsafe int FromFileVersion9(byte *fptr, int length) {
			length --;
			byte *ptr = fptr;
			var checksum = *ptr; ptr++;
			var end = fptr + length;
			byte testchecksum = 0;
			for( var p = fptr+1; p<end; p++) {
				testchecksum ^= *p;	
			}
			if( minimumTick == 0L) {
				var symbol = Factory.Symbol.LookupSymbol(binary.Symbol);
				minimumTick = symbol.MinimumTick.ToLong();
			}
			
			while( (ptr - fptr) < length) {
				var field = (BinaryField) (*ptr >> 3);
				switch( field) {
					case BinaryField.Reset:
						if( debug) log.Debug("Processing Reset during tick de-compression.");
						ReadField( &ptr);
						var symbol = binary.Symbol;
						binary = default(TickBinary);
						binary.Symbol = symbol;
						lastChecksum = 0;
						break;
					case BinaryField.ContentMask:
						binary.ContentMask += (byte) ReadField( &ptr);
						break;
					case BinaryField.Time:
						binary.UtcTime += ReadField( &ptr);
						var ts = new TimeStamp(binary.UtcTime);
						break;
					case BinaryField.Bid:
						binary.Bid += ReadField( &ptr) * minimumTick;
						break;
					case BinaryField.Ask:
						binary.Ask += ReadField( &ptr) * minimumTick;
						break;
					case BinaryField.Price:
						binary.Price += ReadField( &ptr) * minimumTick;
						break;
					case BinaryField.Size:
						binary.Size += (int) ReadField( &ptr);
						break;
					case BinaryField.BidSize:
						ReadBidSize( &ptr);
						break;
					case BinaryField.AskSize:
						ReadAskSize( &ptr);
						break;
					default:
						throw new ApplicationException("Unknown tick field type: " + field);
				}
			}
			
			if( (byte) (testchecksum ^ lastChecksum) != checksum) {
				System.Diagnostics.Debugger.Break();
				throw new ApplicationException("Checksum mismatch " + checksum + " vs. " + (byte) (testchecksum ^ lastChecksum) + ". This means integrity checking of tick compression failed.");
			}
			lastChecksum = testchecksum;

			int len = (int) (ptr - fptr);
			return len;
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
			fixed( byte *fptr = reader.GetBuffer()) {
				byte *sptr = fptr + reader.Position;
				byte *ptr = sptr;
				byte size = *ptr; ptr ++;
				dataVersion = *ptr; ptr ++;
				switch( dataVersion) {
					case 8:
						ptr += FromFileVersion8(ptr);
						break;
					case 9:
						ptr += FromFileVersion9(ptr,(short)(size-1));
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
			var symbol = binary.Symbol;
			binary = default(TickBinary);
			binary.Symbol = symbol;
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
					log.Error(msg);
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
		
		public short AskLevel(int level) {
			fixed( ushort *p = binary.DepthAskLevels) {
				return (short) *(p+level);
			}
		}
		
		public short BidLevel(int level) {
			fixed( ushort *p = binary.DepthBidLevels) {
				return (short) *(p+level);
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

		public long lSymbol {
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
		}
		
		public bool IsRealTime {
			get { return false; }
			set { }
		}
	}
}
