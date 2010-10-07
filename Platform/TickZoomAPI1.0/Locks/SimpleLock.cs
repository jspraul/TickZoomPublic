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
	public class SimpleLock : IDisposable {
		private static Log log = Factory.SysLog.GetLogger(typeof(SimpleLock));
		private string lastLock = "";
		private static bool debug = log.IsDebugEnabled;
	    private int isLocked = 0;
	    
		public bool IsLocked {
			get { return isLocked == 1; }
		}
	    
		public bool TryLock() {
	    	bool retVal = isLocked == 0 && Interlocked.CompareExchange(ref isLocked,1,0) == 0;
	    	if( debug && retVal) {
	    		lastLock = Environment.StackTrace;
	    	}
	    	return retVal;
	    }
	    
		public void Lock() {
			while( !TryLock()) {
	    	}
	    }
	    
	    public SimpleLock Using() {
	    	Lock();
	    	return this;
	    }
	    
	    public void Unlock() {
	    	Interlocked.Exchange(ref isLocked, 0);
	    }
		
		public void Dispose()
		{
			Unlock();
		}
		
		public string LastLock {
			get { return lastLock; }
		}
	    
	}
}
