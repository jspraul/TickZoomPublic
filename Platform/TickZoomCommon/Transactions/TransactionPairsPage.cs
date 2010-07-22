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
using TickZoom.Api;
using System.Runtime.InteropServices;

namespace TickZoom.Transactions
{	
	public class TransactionPairsPage
	{
		volatile int pageNumber;
		byte[] buffer;
		int capacity = 0;
		int count = 0;
		int structSize = Marshal.SizeOf(typeof(TransactionPairBinary));
		
		public TransactionPairsPage() {
			
		}
		
		public void SetCapacity(int capacity) {
			int bufferLength = structSize*capacity;
			if( buffer == null || buffer.Length != bufferLength) {
				buffer = new byte[bufferLength];
			}
			this.capacity = capacity;
			this.count = 0;
		}
		
		public void SetPageSize(int pageSize)
		{
			if( buffer == null || buffer.Length != pageSize) {
				buffer = new byte[pageSize];
			}
			this.capacity = pageSize / structSize;
			this.count = capacity;
		}
		
		public unsafe void Add(TransactionPairBinary trade) {
			if( count >= capacity) {
				throw new ApplicationException("Only " + capacity + " items allows per transaction page.");
			}
			fixed( byte *bptr = buffer) {
				TransactionPairBinary *tptr = (TransactionPairBinary *) bptr;
				*(tptr+count) = trade;
			}
			count++;
		}
		
		private void CheckIndex(int index) {
			if( index >= count || index < 0) {
				throw new ApplicationException("Index " + index + " must be greater than zero and less than " + count + ".");
			}
		}
		
		public unsafe TransactionPairBinary this [int index] {
		    get { 
				CheckIndex(index);
				fixed( byte *bptr = buffer) {
					TransactionPairBinary *tptr = (TransactionPairBinary *) bptr;
					return *(tptr+index);
				}
			}
		    set {
				CheckIndex(index);
				fixed( byte *bptr = buffer) {
					TransactionPairBinary *tptr = (TransactionPairBinary *) bptr;
					*(tptr+index) = value;
				}
			}
		}
		
		public unsafe int PageNumber {
			get { return pageNumber; }
			set { pageNumber = value; }
		}
		
		public byte[] Buffer {
			get { return buffer; }
		}
	}
}