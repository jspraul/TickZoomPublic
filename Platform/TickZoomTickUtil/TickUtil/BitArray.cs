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
	public class BitArray : IComparable<BitArray> {
		public byte[] Bytes;
		public int bitCount;
		public BitArray( int capacity) {
			Bytes = new byte[capacity/8];
			bitCount = 0;
		}
		public void Add(bool value) {
			this[BitCount]=value;
			BitCount++;
		}
		public void Clear() {
			for( int i=0;i<ByteCount;i++) {
				Bytes[i] = 0;
			}
			BitCount = 0;
		}
		public bool this[int pos] {
			get { 
				if( pos > BitCount) {
					throw new IndexOutOfRangeException("index " + pos + " too high");
				}
				return (Bytes[pos/8] & ( (byte)(1 << (pos%8)) )) != 0;
			}
			set { 
				if( pos > BitCount) {
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
	}
	

}
