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

using TickZoom.Api;

namespace TickZoom.Transactions
{
	internal class PagePool<T>
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(PagePool<>));
		private Stack<T> stack = new Stack<T>();
	    private object locker = new object(); 
	    private Func<T> constructor;
	    
	    public PagePool(Func<T> constructor) {
	    	this.constructor = constructor;
	    }
	
	    public T Create()
	    {
	        lock (locker)
	        {
	            if (stack.Count == 0)
	            {
	            	return constructor();
	            }
	            else
	            {
	            	return stack.Pop();
	            }
	        }
	    }
	
	    public void Free(T item)
	    {
	        lock (locker)
	        {
	        	if( stack.Contains(item)) {
	        		throw new ApplicationException("Object has already been freed.");
	        	}
            	stack.Push(item);
	        }
	    }
	}
}
