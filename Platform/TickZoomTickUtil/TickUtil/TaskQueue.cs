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
using TickZoom.Api;

namespace TickZoom.TickUtil
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class TaskQueue
	{
		long enqueueSpins = 0;
		long dequeueSpins = 0;
	    int isLocked = 0;
	    System.Collections.Generic.Queue<ThreadTask> queue;
	    volatile bool terminate = false;
	    int processorCount = Environment.ProcessorCount;	    
	    int maxSize = 10000;
	    
		public TaskQueue() {
			queue = new System.Collections.Generic.Queue<ThreadTask>();
	    }
	    
	    public TaskQueue(int maxSize) {
			this.maxSize = maxSize;
			queue = new System.Collections.Generic.Queue<ThreadTask>(maxSize);
	    }
    
	    private int Lock(bool spin) {
	    	int spins=0;
	    	while( Interlocked.CompareExchange(ref isLocked,1,0) == 1 ) {
            	if( terminate) {
            		break;
            	}
	    		if( !spin || processorCount == 1) {
	    			Factory.Parallel.Yield();
	    		}
	    		spins++;
	    	} 
	    	return spins;
	    }
	    private void UnLock() {
	    	isLocked = 0;
	    }
	    
	    public void EnQueue(ThreadTask o) {
	    	EnQueue(o,false);
	    }
	    
	    public void EnQueue(ThreadTask o, bool spin)
	    {
            // If the queue is full, wait for an item to be removed
            while (queue.Count>=maxSize) {
            	if( terminate) {
            		throw new CollectionTerminatedException();
            	}
            	if(!spin || processorCount == 1) { Factory.Parallel.Yield(); }
            	enqueueSpins ++;
            }
	        enqueueSpins += Lock(spin);
	        ThreadTask task = (ThreadTask) o;
           	queue.Enqueue(task);

            // We always need to pulse, even if the queue wasn't
            // empty before. Otherwise, if we add several items
            // in quick succession, we may only pulse once, waking
            // a single thread up, even if there are multiple threads
            // waiting for items.            
            UnLock();
	    }

	    
	    public ThreadTask Dequeue()
	    {
	    	return Dequeue(false);
	    }
	    
	    public ThreadTask Dequeue(bool spin)
	    {
	    	ThreadTask retVal = null;
            // If the queue is empty, wait for an item to be added
            // Note that this is a while loop, as we may be pulsed
            // but not wake up before another thread has come in and
            // consumed the newly added object. In that case, we'll
            // have to wait for another pulse.
            while (retVal == null)
            {
            	if( terminate) {
            		throw new CollectionTerminatedException();
            	}
            	if(!spin || processorCount == 1) { Factory.Parallel.Yield(); }
            	dequeueSpins++;
		        enqueueSpins += Lock(spin);
		        if( queue.Count>0) {
			        retVal = queue.Dequeue();
		        }
	            UnLock();
            }
            return retVal;
	    }
	    
	    public void Clear() {
	    	Lock(false);
        	queue.Clear();
        	UnLock();
	    }
	    
	    public void Terminate() {
	    	Lock(false);
	    	terminate = true;
	    	queue.Clear();
            // empty before. Otherwise, if we add several items
            // in quick succession, we may only pulse once, waking
            // a single thread up, even if there are multiple threads
            // waiting for items.            
            UnLock();
	    }

	    public int Count {
	    	get { return queue.Count; }
	    }
	    
		public long EnqueueSpins {
			get { return enqueueSpins; }
		}
	    
		public long DequeueSpins {
			get { return dequeueSpins; }
		}
		
		StartEnqueue startEnqueue;
		
		public StartEnqueue StartEnqueue {
			get { return startEnqueue; }
			set { startEnqueue = value;	}
		}
		
		public void StartDequeue()
		{
			StartEnqueue();
		}
	}
}
