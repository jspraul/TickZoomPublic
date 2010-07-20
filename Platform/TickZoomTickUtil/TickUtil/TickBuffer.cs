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
using TickZoom.TickUtil;

namespace TickZoom.TickUtil
{
	/// <summary>
	/// Description of TickBuffer.
	/// </summary>
	public class TickBuffer
	{
		private TickImpl[] buffer = new TickImpl[10000];
		private volatile int writeIndex = 0;
		private volatile int readIndex = 0;
		private volatile bool isReadable = false;
		public void AddTick(ref TickImpl tick) {
			if( !isReadable) {
				buffer[writeIndex] = tick;
				writeIndex ++;
			}
			if( writeIndex >= buffer.Length ) {
				readIndex = 0;
				isReadable = true;
			}
		}
		
		public TickImpl NextTick() {
			if( isReadable) {
				TickImpl tick = buffer[readIndex];
				readIndex++;
				if( readIndex >= buffer.Length || readIndex >= writeIndex) {
					writeIndex = 0;
					isReadable = false;
				}
				return tick;
			} else {
				throw new Exception( "TickBuffer overflow.");
			}
		}
		
		public bool IsReadable {
			get { return isReadable; }
		}
		
		public bool IsWritable {
			get { return !isReadable; }
		}
		
		public int Count {
			get { return writeIndex; }
		}
		
		public void Close() {
			// Switch to readable so the reader can read
			// any last ticks.
			readIndex = 0;
			isReadable = true;
		}
	}
}
