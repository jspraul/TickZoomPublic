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
using System.Threading;

namespace TickZoom.TickUtil
{
	/// <summary>
	/// Description of TaskRunner.
	/// </summary>
	public class TaskRunner
	{
   		static readonly Log log = Factory.Log.GetLogger(typeof(TickReader));
   		static readonly bool debug = log.IsDebugEnabled;
		private TaskQueue waitingQueue;
		private bool cancel = false;
		private ThreadTask currentTask = null;
		internal TaskRunner(TaskQueue waitingQueue)
		{
			this.waitingQueue = waitingQueue;
		}
		public void Run() {
			try { 
		    	if( debug) log.Debug("Thread started.");
				while( !cancel) {
					currentTask = waitingQueue.Dequeue();
					currentTask.Run();
				}
    		} catch (ThreadAbortException) {
    			log.Warn("WARN: Thread is being aborted");
			} catch( CollectionTerminatedException ) {
		    	if( debug) log.Debug("Thread ended - CollectionTerminatedException");
			} catch( Exception ex) {
				log.Error("ERROR: Thread had an exception:",ex);
			}
		}
		
		public void Stop() {
	    	if( debug) log.Debug("Stop called");
			cancel = true;
			waitingQueue.Terminate();
			if( currentTask!= null) {
				currentTask.Stop();
			}
		}
	}
}
