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
using System.IO;
using System.Text;

namespace TickZoom.TickUtil
{
	public class TickMemoryStreamX : MemoryStream {
		byte[] buffer;
		int position;
		int size;
		public TickMemoryStreamX() : this(10240) {
		}
		
		public TickMemoryStreamX(int capacity) {
			buffer = new byte[capacity];
		}
		
		public sealed override void Write( byte[] bytes, int offset, int length) {
			Buffer.BlockCopy(bytes,offset,buffer,position,length);
			this.position+=length;
			if( position > size) {
				size=position;
			}
		}
		
		public sealed override byte[] GetBuffer()
		{
			return buffer;
		}
		
		public sealed override int Read( byte[] bytes, int offset, int length) {
			int ret;
			if( position+length > size) {
				ret = -1;
			} else {
				Buffer.BlockCopy(buffer,position,bytes,offset,length);
				this.position+=length;
				ret = length;
			}
			return ret;
		}
		
		public sealed override int ReadByte()
		{
			int ret;
			if( position>=size) {
				return -1;
			} else {
				ret = buffer[position];
				position++;
			}
			return ret;
		}
		
		public sealed override void WriteByte(byte value)
		{
			buffer[position] = value;
			position++;
			if( position>size) {
				size = position;
			}
		}
		
		public sealed override void SetLength(long value) {
			size = (int) value;
		}
		
		public sealed override long Seek(long offset, SeekOrigin origin) {
			switch(origin) {
				case SeekOrigin.Begin:
					position = (int) offset;
					break;
				case SeekOrigin.End:
					position = size - (int) offset;
					break;
				case SeekOrigin.Current:
					position += (int) offset;
					break;
			}
			return position;
		}
		
		public sealed override void Flush() {
			
		}
		
		public sealed override bool CanWrite {
			get { return true; }
		}
		
		public sealed override bool CanRead {
			get { return true; }
		}
		
		public sealed override bool CanSeek {
			get { return true; }
		}
		
		public override long Length {
			get { return size; }
		}
		
		public sealed override int Capacity {
			get { return buffer.Length; }
			set { throw new NotImplementedException(); }
		}
		
		public sealed override long Position {
			get { return position; }
			set { position = (int) value; }
		}
		
		public sealed override byte[] ToArray()
		{
			byte[] newBuff = new byte[size];
			Buffer.BlockCopy(buffer,0,newBuff,0,size);
			return newBuff;
		}
	}
}
