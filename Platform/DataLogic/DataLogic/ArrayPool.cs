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

namespace TickZoom.PriceData
{
	public class ArrayPool<T>
	{
		private static readonly Log log = Factory.SysLog.GetLogger("TickZoom.Engine.CircularArrayPool");
		private Dictionary<int,Stack<T[]>> map = new Dictionary<int,Stack<T[]>>();
	    private object _sync = new object(); 
	
	    public T[] Create(int capacity)
	    {
	        lock (_sync)
	        {
	        	Stack<T[]> stack;
	        	if( map.TryGetValue(capacity,out stack)) {
		            if (stack.Count == 0)
		            {
		            	return new T[capacity];
		            }
		            else
		            {
		            	return stack.Pop();
		            }
	        	} else {
	        		stack = new Stack<T[]>();
	        		map[capacity] = stack;
	        		return new T[capacity];
	        	}
	        }
	    }
	
	    public void Free(T[] item)
	    {
	        lock (_sync)
	        {
	        	Stack<T[]> stack;
	        	if( map.TryGetValue(item.Length,out stack)) {
	            	stack.Push(item);
	        	} else {
	        		log.Error("Capacity " + item.Length + " not found in memory list.");
	        	}
	        }
	    }
	}
}
