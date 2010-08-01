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
using System.Threading;

namespace TickZoom.Api
{
	public class TaskLock : IDisposable {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TaskLock));
	    private int isLocked = 0;
	    private int lockCount = 0;
	    
		public bool AlreadyLocked {
			get { return isLocked != 0 && isLocked == Thread.CurrentThread.ManagedThreadId; }
		}
	    
		public bool WillBlock {
			get { return isLocked != 0 && isLocked != Thread.CurrentThread.ManagedThreadId; }
		}
	    
		public bool TryLock() {
	    	if( isLocked == Thread.CurrentThread.ManagedThreadId) {
	    		Thread.BeginCriticalRegion();
	    		lockCount++;
	    		return true;
	    	} else if( isLocked == 0 && Interlocked.CompareExchange(ref isLocked,Thread.CurrentThread.ManagedThreadId,0) == 0) {
	    		Thread.BeginCriticalRegion();
	    		lockCount++;
	    		return true;
	    	} else {
	    		return false;
	    	}
	    }
	    
		public void Lock() {
			while( !TryLock()) {
				Factory.Parallel.Yield();
	    	}
	    }
	    
	    public TaskLock Using() {
	    	Lock();
	    	return this;
	    }
	    
	    public void Unlock() {
	    	if( isLocked == Thread.CurrentThread.ManagedThreadId) {
	    		lockCount --;
	    		if( lockCount <= 0) {
	    			ForceUnlock();
	    		}
	    	} else {
	    		string message = "Attempt to unlock from a different managed thread.";
	    		log.Error( message + "\n" + Environment.StackTrace);
	    		throw new ApplicationException( message);
	    	}
	    }
	    
	    public void ForceUnlock() {
	    	Thread.EndCriticalRegion();
	    	Interlocked.Exchange(ref isLocked, 0);
   			lockCount = 0;
	    }
		
		public void Dispose()
		{
			Unlock();
		}
	}

}
