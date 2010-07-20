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

using TickZoom.Api;
using TickZoom.TickUtil;

namespace TickZoom.TickUtil
{
	public class BitBuffer {
		public byte[] Bytes;
		public int bitCount;
		public BitBuffer( int capacity) {
			Bytes = new byte[capacity];
			bitCount = 0;
		}
		
		public void Add(bool value) {
			if( value) {
				Bytes[bitCount>>3] |= (byte) (1 << (bitCount%8));
			}
			bitCount++;
		}
		
		public void Add(byte[] buffer, int bits) {
			int length = bits>>3;			
			int extra = bits&7;
			for(int i=0; i<length; i++) {
				Add(buffer[i]);
			}
			if( extra != 0) {
				Add( buffer[length], extra);
			}
		}
		
		public void Add(byte value) {
			Add(value,8);
		}
		public void Add(byte value,int bits) {
			int lShift=bitCount&7; // modulus of 8
			int offset = bitCount>>3;
			if( lShift == 0) {
				// Copy byte directly
				Bytes[offset] = value;
				bitCount+=bits;
			} else {
				int rShift=bits-lShift;
				Bytes[offset] |= (byte) (value << lShift);
				Bytes[offset+1] = (byte) (value >> rShift);
				bitCount+=bits;
			}
		}
		
		public void Clear() {
			for( int i=0;i<ByteCount;i++) {
				Bytes[i] = 0;
			}
			BitCount = 0;
		}
		public bool this[int pos] {
			get { 
				if( pos >= BitCount) {
					throw new IndexOutOfRangeException("index " + pos + " too high");
				}
				return (Bytes[pos/8] & ( (byte)(1 << (pos%8)) )) != 0;
			}
			set { 
				if( pos >= BitCount) {
					throw new IndexOutOfRangeException("index " + pos + " too high");
				}
				if( value) {
					Bytes[pos/8] |= (byte) (1 << (pos%8));
				}
			}
		}
		public override bool Equals(object obj)
		{
			return CompareTo( (BitArray) obj) == 0;
		}
		public override int GetHashCode()
		{
			int hashCode = 0;
			for( int i=0;i<ByteCount;i++) {
				hashCode ^= Bytes[i] << (i%4);
			}
			return hashCode;
		}
		
		public int CompareTo(BitArray other)
		{
			for( int i=0;i<ByteCount;i++) {
				int val = Bytes[i] - other.Bytes[i];
				if( val != 0 ) {
					return val;
				}
			}
			return 0;
		}
		public int BitCount {
			get { return bitCount; }
			set { for( int i=value/8+1;i<ByteCount;i++) {
					Bytes[i] = 0;
				}
				bitCount = value;
			}
		}
		
		public int ByteCount {
			get { return BitCount/8 + (BitCount%8>0 ? 1 : 0); }
		}
		
		public string DebugBytes {
			get {
				string result="";
				byte[] temp = new byte[ByteCount];
				Array.Copy(Bytes,temp,ByteCount);
				if (BitConverter.IsLittleEndian)
	    			Array.Reverse(temp);

				for( int i=0; i<ByteCount; i++) {
					if( i!=0) { result+="-"; }
					result+=ByteToString(temp[i]);
				}
				return result;
			}
		}
		
		private string ByteToString(byte ba)
		{
			
			return LeadingZeros(Convert.ToString(ba,2));
		}
		
		private string LeadingZeros(string x)
		{
			if( x.Length<8) {
				x = new string('0',8-x.Length) + x;
			}
			return x;
//		    char[] charArray = new char[x.Length];
//		    int len = x.Length - 1;
//		    for (int i = 0; i <= len; i++)
//		        charArray[i] = x[len-i];
//		    return new string(charArray);
		}
	}
}
