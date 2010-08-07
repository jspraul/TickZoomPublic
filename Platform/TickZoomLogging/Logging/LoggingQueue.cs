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
using System.Threading;

using log4net.Core;
using TickZoom.Api;

namespace TickZoom.Logging
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class LoggingQueue
	{
	    System.Collections.Generic.Queue<LoggingEvent> queue =
	    	new System.Collections.Generic.Queue<LoggingEvent>();
	    bool terminate = false;
	    int maxSize = int.MaxValue;
	    object listLock = new object();
	    
	    public LoggingQueue() {
	    }
	    
	    public LoggingQueue(int maxSize) {
	    	this.maxSize = maxSize;
	    }
	    
	    public void EnQueue(LoggingEvent o)
	    {
	    	lock (listLock) {
	            // If the queue is full, wait for an item to be removed
	            while (queue.Count>=maxSize) {
	            	if( terminate) {
	            		throw new CollectionTerminatedException();
	            	}
	                // This releases listLock, only reacquiring it
	                // after being woken up by a call to Pulse
	                System.Threading.Monitor.Wait(listLock);
	            }
	            
	            queue.Enqueue(o);
	
	            // We always need to pulse, even if the queue wasn't
	            // empty before. Otherwise, if we add several items
	            // in quick succession, we may only pulse once, waking
	            // a single thread up, even if there are multiple threads
	            // waiting for items.            
	            System.Threading.Monitor.Pulse(listLock);
	    	}
	    }
	    
	    public LoggingEvent Dequeue()
	    {
	    	lock( listLock) {
	            // If the queue is empty, wait for an item to be added
	            // Note that this is a while loop, as we may be pulsed
	            // but not wake up before another thread has come in and
	            // consumed the newly added object. In that case, we'll
	            // have to wait for another pulse.
	            while (queue.Count==0)
	            {
	            	if( terminate) {
	            		throw new CollectionTerminatedException();
	            	}
	                // This releases listLock, only reacquiring it
	                // after being woken up by a call to Pulse
	                System.Threading.Monitor.Wait(listLock);
	            }
	            LoggingEvent retVal = queue.Dequeue();
	            
	            // We always need to pulse, in case the queue was full
	            // and a feeder thread is waiting to write more data.
	            System.Threading.Monitor.Pulse(listLock);
	            return retVal;
	    	}
	    }
	    
	    public void Clear() {
	    	lock (listLock) {
	        	queue.Clear();
	    	}
	    }
	    
	    public void Terminate() {
	    	lock( listLock) {
		    	terminate = true;
	            // empty before. Otherwise, if we add several items
	            // in quick succession, we may only pulse once, waking
	            // a single thread up, even if there are multiple threads
	            // waiting for items.            
	            System.Threading.Monitor.Pulse(listLock);
	    	}
	    }

	    public int Count {
	    	get { return queue.Count; }
	    }
	
	}

}
