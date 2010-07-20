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
using System.Threading;

namespace TickZoom.Api
{
	public class CheckedMemoryStream : MemoryStream {
		private const int growth = 0x100;
		private byte[] store = new byte[growth];
		private volatile int position = 0;
		private volatile int length = 0;
		
		public override void Close()
		{
			position = 0;
			length = 0;
		}
		
		protected override void Dispose(bool disposing)
		{
			Close();
		}
		
		
		public override int Capacity {
			get { return store.Length; }
			set { EnsureCapacity(value); }
		}
		
		public override byte[] GetBuffer()
		{
			return store;
		}
		
		private void EnsureCapacity(int capacity) {
			if( capacity > store.Length) {
				int newSize = ((capacity/growth)+1)*growth;
				byte[] newStore = new byte[newSize];
				Buffer.BlockCopy(store,0,newStore,0,length);
				store = newStore;
			} else if( capacity < 0) {
				throw new Exception("Capacity must be greater than zero instead of: " + capacity);
			}
		}
		
		public override void Flush()
		{
			throw new NotImplementedException();
		}
		
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch( origin) {
				case SeekOrigin.Begin:
					if( offset > length) {
						throw new InvalidOperationException("Seek offset: " + offset + " is greater than length: " + length );
					}
					position = (int) offset;
					break;
				case SeekOrigin.Current:
					if( offset + position > length) {
						throw new InvalidOperationException("Seek offset: " + offset + " plus position: " + position + " is greater than length: " + length );
					}
					position += (int) offset;
					break;
				case SeekOrigin.End:
					if( position - offset < 0) {
						throw new InvalidOperationException("Seek offset: " + offset + " minus position: " + position + " is less than zero.");
					}
					position = length - (int) offset;
					break;
				default:
					throw new InvalidOperationException("Unknown SeekOrigin value: " + origin);
			}
			return position;
		}
		
		public override void SetLength(long value)
		{
			EnsureCapacity((int) value);
			if( value < length) {
				Array.Clear(store,(int)value,length - (int)value);
			}
			length = (int) value;
		}
		
		public override int ReadByte()
		{
			if( position >= length) {
				throw new InvalidOperationException("Cannot read past end of stream. Position: " + position + " is greater than length: " + length);
			}
			int result = (int) store[position];
			position+=1;
			return result;
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			if( position >= length) {
				throw new InvalidOperationException("Cannot read past end of stream. Position: " + position + " is greater than length: " + length);
			}
			if( position + count > length) {
				count = length - position;
			}
			Buffer.BlockCopy(store,position,buffer,offset,count);
			position+=count;
			return count;
		}
		
		public override void WriteByte(byte value)
		{
			EnsureCapacity(position+1);
			store[position] = value;
			position ++;
			if( position > length) {
				length = position;
			}
		}
		
		public override void Write(byte[] buffer, int offset, int count)
		{
			EnsureCapacity(position+count);
			Buffer.BlockCopy(buffer,offset,store,position,count);
			position+=count;
			if( position > length) {
				length = position;
			}
		}
		
		public override bool CanRead {
			get { return true; }
		}
		
		TaskLock dataLock = new TaskLock();
		string otherStack = null;
		public override long Position {
			get {
				if( !dataLock.TryLock() ) {
					Thread.Sleep(1000);
					throw new ApplicationException("Two threads accessing memory position getter. Other thread stack:\n" + otherStack + "\nCurrent thread stack:");
				}
				try { 
					return position;
				} finally {
					dataLock.Unlock();
				}
			}
			set {
				if( !dataLock.TryLock() ) {
					Thread.Sleep(1000);
					throw new ApplicationException("Two threads accessing memory position setter. Other thread stack:\n" + otherStack + "\nCurrent thread stack:");
				}
				try { 
					EnsureCapacity(position);
					position = (int)value;
				} finally {
					dataLock.Unlock();
				}
			}
		}
		
		public override long Length {
			get { return length; }
		}
		
		public override bool CanWrite {
			get { return true; }
		}
		
		public override bool CanSeek {
			get { return true; }
		}
	}
}
