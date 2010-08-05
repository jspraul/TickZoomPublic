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
using TickZoom.Api;

namespace TickZoom.PriceData
{
	public class DataSeriesDefault<T> : DataSeries<T>
	{
		private static readonly ArrayPool<T> arrayPool = new ArrayPool<T>();
		unsafe private T[] array;
		
		public T[] Array {
			get { return array; }
			set { array = value; }
		}
		private int count;
		private int externalCount;
		private int index;
		private int size;
		
		public DataSeriesDefault(int size)
		{
			array = arrayPool.Create(size);
			this.size = size;
			Clear();
		}
		
		public void Clear() {
			System.Array.Clear(array,0,array.Length);
			count = 0;
			index = 0;
			externalCount = 0;
		}
		
		public void Add(T value) {
			unsafe {
				array[NextIndex()] = value;
			}
		}
		
		int offset;
		
		private int IndexOffset(int pos) {
			offset = index - pos;
			if( offset < 0) {
				return count + offset;
			} else {
				return offset;
			}
		}

		public int NextIndex() {
			if( count < array.Length) {
				index = count;
				count++;
			} else {
				index++;
				if( index >= count) {
					index = 0;
				}
			}
			externalCount++;
			return index;
		}
		
		public void Resize (int newSize) {
		   int oldSize = array.Length;
		   T[] newArray = arrayPool.Create(newSize);
		   int preserveLength = System.Math.Min(oldSize,newSize);
		   if (preserveLength > 0)
		   {
		      System.Array.Copy (array,newArray,preserveLength);
		   }
		   arrayPool.Free(array);
		}
		
		public int Index {
			get { return index; }
		}
		public int Count {
			get { return Math.Min(externalCount,array.Length); }
		}
		public int BarCount {
			get { return externalCount; }
		}
		public int CurrentBar {
			get { return externalCount-1; }
		}
		public int publicCount {
			get { return count; }
		}
		
		public int Capacity {
			get { return array.Length; }
		}
		
		public int CalcIndex(int position) {
			if( position >= externalCount) {
				BeyondCircularException e = new BeyondCircularException();
				e.CircularArray = this;
				e.Position = position;
				throw e;
			}
			if( position >= array.Length ) {
				string name = "DataSeries";
				throw new ApplicationException("PositionChange " + position + " greater than capacity of " + name);
			} 
			int index = IndexOffset(position);
			if( index < 0) {
				string name = "DataSeries";
				throw new ApplicationException("Index " + index + " must be non-negative for " + name);
			} else {
				return index;
			}
		}

		public T this[int position]
		{
			get { int index = CalcIndex(position);
				  return array[index];
			}
			set { int index = CalcIndex(position);
				array[index] = value;
			}
		}
		
		public void Release() {
			if( array!=null) {
				arrayPool.Free(array);
				array = null;
			}
		}
		
	}
}
