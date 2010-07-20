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
using TickZoom.Api;
using TickZoom.TickUtil;

namespace TickZoom.Test
{
	public class VerifyFeed : Receiver {
		private static readonly Log log = Factory.Log.GetLogger(typeof(VerifyFeed));
		private static readonly bool debug = log.IsDebugEnabled;
		private TickQueue tickQueue = Factory.TickUtil.TickQueue(typeof(VerifyFeed));
		private object taskLocker = new object();
		
		public ReceiverState OnGetReceiverState(SymbolInfo symbol) {
			return ReceiverState.Ready;
		}
		
		public VerifyFeed() {
			tickQueue.StartEnqueue = Start;
		}
		
		public void Start() {
		}
        public long Verify(Action<TickIO, TickIO, ulong> assertTick, SymbolInfo symbol, int timeout) {
			return Verify(2, assertTick, symbol, timeout);
		}
    	TickImpl lastTick = new TickImpl();
    	int countLog = 0;
    	TickBinary tickBinary = new TickBinary();
    	TickImpl tick = new TickImpl();
        public long Verify(int expectedCount, Action<TickIO, TickIO, ulong> assertTick, SymbolInfo symbol, int timeout) {
			if( debug) log.Debug("VerifyFeed");
            int startTime = Environment.TickCount;
            count = 0;
			while( Environment.TickCount - startTime < timeout * 1000 ) {
            	if( propagateException != null) {
            		throw propagateException;
            	}
    			if( HandleTick(expectedCount, assertTick,symbol)) {
    				break;
    			}
        	}
            return count;
		}

		private bool HandleTick(int expectedCount, Action<TickIO, TickIO, ulong> assertTick, SymbolInfo symbol) {
			try { 
    			while( !tickQueue.TryDequeue(ref tickBinary)) {
	            	if( propagateException != null) {
	            		throw propagateException;
	            	}
    				Thread.Sleep(1);
    			}
            	tick.Inject(tickBinary);
				if( debug && countLog < 5)
				{
					log.Debug("Received a tick " + tick);
					countLog++;
				}
	            startTime = Environment.TickCount;
            	count++;
            	if( count > 0) {
            		assertTick(tick,lastTick,symbol.BinaryIdentifier);
            	}
            	lastTick.Copy(tick);
            	if( propagateException != null) {
            		throw propagateException;
            	}
            	if( count >= expectedCount) return true;
			} catch( QueueException ex) {
				switch( ex.EntryType) {
					case EventType.EndHistorical:
					case EventType.StartRealTime:
					case EventType.EndRealTime:
						break;
					case EventType.Terminate:
						return true;
					default:
						throw new ApplicationException("Unexpected QueueException: " + (EventType)ex.EntryType);
				}
			}
    		return false;
		}
		
       	long count = 0;
       	Task task;
       	int startTime;
		public void StartTimeTheFeed() {
            startTime = Environment.TickCount;
           	count = 0;
           	countLog = 0;
           	task = Factory.Parallel.Loop(this,OnException,TimeTheFeedTask);
        }
       	
       	private Exception propagateException = null;
       	private void OnException( Exception ex) {
       		propagateException = ex;	
       	}
           	
        public int EndTimeTheFeed() {
  			task.Join();
        	if( propagateException != null) {
        		throw propagateException;
        	}
            int endTime = Environment.TickCount;
            int elapsed = endTime - startTime;
            log.Notice("Processed " + count + " ticks in " + elapsed + "ms or " + (count*1000/elapsed) + "ticks/sec");
			Factory.TickUtil.TickQueue("Stats").LogStats();
			return elapsed/1000;
		}		
	
		public Yield TimeTheFeedTask() {
			try {
       			if( !tickQueue.TryDequeue(ref tickBinary)) {
	       			return Yield.NoWork.Repeat;
       			}
       			tick.Inject(tickBinary);
				if( debug && count < 5)
				{
					log.Debug("Received a tick " + tick);
					countLog++;
				}
				count++;
				if( count%1000000 == 0) {
					log.Notice("Read " + count + " ticks");
				}
				return Yield.DidWork.Repeat;
           	} catch( QueueException ex) {
       			if( EventType.EndHistorical != ex.EntryType) {
       				throw new ApplicationException( "Unexpected QueueException: " + ex.EntryType);
       			}
            	log.Debug("Queue Terminated");
            	Factory.Parallel.CurrentTask.Stop();
            	return Yield.NoWork.Repeat;
        	}
		}
       	
		public bool OnRealTime(SymbolInfo symbol) {
       		return true;
		}
		
		public bool OnHistorical(SymbolInfo symbol) {
       		return true;
		}
		
		public bool OnSend(ref TickBinary o)
		{
			try {
				return tickQueue.TryEnQueue(ref o);
			} catch( QueueException) {
				// Queue already terminated.
			}
			return true;
		}

	    public bool OnPositionChange(LogicalFillBinary fill)
	    {
	        throw new NotImplementedException();
	    }

	    public bool OnStop()
		{
			try {
	    		return tickQueue.TryEnQueue(EventType.Terminate, (SymbolInfo) null);
			} catch( QueueException) {
				// Queue already terminated.
			}
	    	return true;
		}
		
	    public bool OnError( string error) {
	    	log.Error( error);
	    	tickQueue.Terminate();
	    	return true;
	    }
		public bool Close() {
			tickQueue.Terminate();
			return true;
		}
		
		public bool OnEndHistorical(SymbolInfo symbol)
		{
			return tickQueue.TryEnQueue(EventType.EndHistorical, symbol);
		}
		
		public bool OnEndRealTime(SymbolInfo symbol)
		{
       		try {
				return tickQueue.TryEnQueue(EventType.EndRealTime, symbol);
       		} catch ( QueueException) {
       			// Queue was already ended.
       		}
			return true;
		}
		public bool OnEvent(SymbolInfo symbol, int eventType, object eventDetail) {
			if( isDisposed) return false;
			bool result = false;
			try {
				switch( (EventType) eventType) {
					case EventType.Tick:
						TickBinary binary = (TickBinary) eventDetail;
						result = OnSend(ref binary);
						break;
					case EventType.EndHistorical:
						result = OnEndHistorical(symbol);
						break;
					case EventType.StartRealTime:
						result = OnRealTime(symbol);
						break;
					case EventType.StartHistorical:
						result = OnHistorical(symbol);
						break;
					case EventType.EndRealTime:
						result = OnEndRealTime(symbol);
						break;
					case EventType.Error:
						result = OnError((string)eventDetail);
						break;
					case EventType.LogicalFill:
						result = OnPositionChange((LogicalFillBinary)eventDetail);
						break;
					case EventType.Terminate:
						result = OnStop();
			    		break;
					case EventType.Initialize:
					case EventType.Open:
					case EventType.Close:
					case EventType.PositionChange:
					default:
			    		throw new ApplicationException("Unexpected EventType: " + eventType);
				}
			} catch( QueueException) {
				log.Warn("Already terminated.");
			}
			return result;
		}
		
 		private volatile bool isDisposed = false;
	    public void Dispose() 
	    {
	        Dispose(true);
	        GC.SuppressFinalize(this);      
	    }
	
	    protected virtual void Dispose(bool disposing)
	    {
       		if( !isDisposed) {
	    		lock( taskLocker) {
		            isDisposed = true;   
		            if (disposing) {
		            	if( tickQueue != null) {
		            		tickQueue.Terminate();
		            	}
		            }
	    		}
    		}
	    }
	    
	}
}
