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

namespace TickZoom.Common
{
	public class VerifyFeedDefault : Receiver, VerifyFeed, IDisposable
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(VerifyFeed));
		private readonly bool debug = log.IsDebugEnabled;
		private TickQueue tickQueue = Factory.TickUtil.TickQueue(typeof(VerifyFeed));
		private volatile bool isRealTime = false;
		private TickSync tickSync;
		private volatile ReceiverState receiverState = ReceiverState.Ready;
		private volatile BrokerState brokerState = BrokerState.Disconnected;
		private Task task;
		private static object taskLocker = new object();
		private Pool<TickBinaryBox> tickPool = Factory.TickUtil.TickPool();
		private bool keepReceived = false;
		private List<TickBinary> received = new List<TickBinary>();
		private int pauseSeconds = 3;
		
		public List<TickBinary> GetReceived() {
			return received;
		}

		public TickQueue TickQueue {
			get { return tickQueue; }
		}

		public ReceiverState OnGetReceiverState(SymbolInfo symbol)
		{
			return receiverState;
		}

		public VerifyFeedDefault()
		{
			tickQueue.StartEnqueue = Start;
		}

		public void Start()
		{
		}
		
		public long VerifyEvent(Action<SymbolInfo,int,object> assertTick, SymbolInfo symbol, int timeout)
		{
			return VerifyEvent(1, assertTick, symbol, timeout);
		}
		
		public long Verify(Action<TickIO, TickIO, long> assertTick, SymbolInfo symbol, int timeout)
		{
			return Verify(2, assertTick, symbol, timeout);
		}
		TickIO lastTick = Factory.TickUtil.TickIO();
		
		int countLog = 0;
		TickBinary tickBinary = new TickBinary();
		TickIO tickIO = Factory.TickUtil.TickIO();
		public long Verify(int expectedCount, Action<TickIO, TickIO, long> assertTick, SymbolInfo symbol, int timeout) {
			return Verify( expectedCount, assertTick, symbol, timeout, null);
		}
		private bool actionAlreadyRun = false;
		public long Verify(int expectedCount, Action<TickIO, TickIO, long> assertTick, SymbolInfo symbol, int timeout, Action action)
		{
			if (debug) log.Debug("Verify");
			if( SyncTicks.Enabled) {
				tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
			}
			long endTime = Factory.Parallel.TickCount + timeout * 1000;
			count = 0;
			while (Factory.Parallel.TickCount < endTime) {
				if( propagateException != null) {
					throw propagateException;
				}
				try { 
					if( tickQueue.TryDequeue(ref tickBinary)) {
						tickIO.Inject(tickBinary);
						if (debug && countLog < 5) {
							log.Debug("Received a tick " + tickIO);
							countLog++;
						}
						startTime = Factory.TickCount;
						count++;
						if (count > 0 && assertTick != null) {
							assertTick(tickIO, lastTick, symbol.BinaryIdentifier);
						}
						lastTick.Copy(tickIO);
						if( !actionAlreadyRun && action != null) {
							actionAlreadyRun = true;
							action();
						}
						if( SyncTicks.Enabled) tickSync.RemoveTick();
						if (count >= expectedCount) {
							break;
						}
					} else {
						Thread.Sleep(100);
					}
				} catch (QueueException ex) {
					if( HandleQueueException(ex)) {
						break;
					}
				}
			}
			return count;
		}
		
		public long Wait(SymbolInfo symbol, int expectedTicks, int timeout)
		{
			if (debug) log.Debug("Wait");
			if( SyncTicks.Enabled) {
				tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
			}
			long startTime = Factory.Parallel.TickCount;
			count = 0;
			while (Factory.Parallel.TickCount - startTime < timeout * 1000) {
				if( propagateException != null) {
					throw propagateException;
				}
				try { 
					if( tickQueue.TryDequeue(ref tickBinary)) {
						tickIO.Inject(tickBinary);
						if (debug && countLog < 5) {
							log.Debug("Received a tick " + tickIO);
							countLog++;
						}
						count++;
						lastTick.Copy(tickIO);
						if( SyncTicks.Enabled) tickSync.RemoveTick();
						if( count >= expectedTicks) {
							break;
						}
					} else {
						Thread.Sleep(100);
					}
				} catch (QueueException ex) {
					if( HandleQueueException(ex)) {
						break;
					}
				}
			}
			return count;
		}
		
		public bool VerifyState(BrokerState expectedBrokerState,
		                                 ReceiverState expectedSymbolState,
		                                 SymbolInfo symbol,
		                                 int timeout) {
			if (debug) log.Debug("VerifyFeed");
			if( SyncTicks.Enabled) {
				tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
			}
			long startTime = Factory.TickCount;
			count = 0;
			TickBinary binary = new TickBinary();
			while (Factory.TickCount - startTime < timeout * 1000) {
				if( propagateException != null) {
					throw propagateException;
				}
				try { 
					if( !tickQueue.TryDequeue(ref binary)) {
						Thread.Sleep(100);
					} else {
						if( SyncTicks.Enabled) tickSync.RemoveTick();
					}
				} catch (QueueException ex) {
					if( HandleQueueException(ex)) {
						break;
					}
				}
				if( brokerState == expectedBrokerState && receiverState == expectedSymbolState) {
					return true;
				}
			}
			return false;
		}
		
		public long VerifyEvent(int expectedCount, Action<SymbolInfo,int,object> assertEvent, SymbolInfo symbol, int timeout)
		{
			if (debug) log.Debug("VerifyEvent");
			long startTime = Factory.TickCount;
			count = 0;
			while (Factory.TickCount - startTime < timeout * 1000) {
				if( propagateException != null) {
					throw propagateException;
				}
				try {
					// Remove ticks just so as to get to the event we want to see.
					if( tickQueue.TryDequeue(ref tickBinary)) {
						if (customEventType> 0) {
							assertEvent(customEventSymbol,customEventType,customEventDetail);
							count++;
						} else {
							Thread.Sleep(10);
						}
						if( SyncTicks.Enabled) tickSync.RemoveTick();
						if (count >= expectedCount) {
							break;
						}
					} else {
						Thread.Sleep(100);
					}
				} catch (QueueException ex) {
					if( HandleQueueException(ex)) {
						break;
					}
				}
			}
			return count;
		}
		
		public int VerifyPosition(int expectedPosition, SymbolInfo symbol, int timeout) {
			return VerifyPosition( expectedPosition, symbol, timeout, null);
		}
		
		public int VerifyPosition(int expectedPosition, SymbolInfo symbol, int timeout, Action action)
		{
			if (debug)
				log.Debug("VerifyFeed");
			log.Info("Sleeping " + pauseSeconds + " seconds to allow checking for over filling of orders.");
			Thread.Sleep( pauseSeconds * 1000);
			long startTime = Factory.TickCount;
			count = 0;
			int position;
			bool actionAlreadyRun = false;
			TickBinary binary = new TickBinary();
			while (Factory.TickCount - startTime < timeout * 1000) {
				if( propagateException != null) {
					throw propagateException;
				}
				try { 
					if( !tickQueue.TryDequeue(ref binary)) {
						Thread.Sleep(10);
					} else {
						if( !actionAlreadyRun && action != null) {
							actionAlreadyRun = true;
							action();
						}
						if( SyncTicks.Enabled) tickSync.RemoveTick();
					}
				} catch (QueueException ex) {
					if( HandleQueueException(ex)) {
						break;
					}
				}
				if( actualPositions.TryGetValue(symbol.BinaryIdentifier,out position)) {
					if( position == expectedPosition) {
						return expectedPosition;
					}
				}
			}
			if( actualPositions.TryGetValue(symbol.BinaryIdentifier,out position)) {
				return position;
			} else {
				return 0;
			}
		}

		private bool HandleQueueException( QueueException ex) {
			log.Notice("QueueException: " + ex.EntryType);
			switch (ex.EntryType) {
				case EventType.StartHistorical:
					receiverState = ReceiverState.Historical;
					isRealTime = false;
					break;
				case EventType.EndHistorical:
					receiverState = ReceiverState.Ready;
					isRealTime = false;
					break;
				case EventType.StartRealTime:
					receiverState = ReceiverState.RealTime;
					isRealTime = true;
					break;
				case EventType.EndRealTime:
					receiverState = ReceiverState.Ready;
					isRealTime = false;
					break;
				case EventType.StartBroker:
					brokerState = BrokerState.Connected;
					isRealTime = true;
					break;
				case EventType.EndBroker:
					brokerState = BrokerState.Disconnected;
					isRealTime = false;
					break;
				case EventType.Terminate:
					receiverState = ReceiverState.Stop;
					isRealTime = false;
					return true;
				default:
					throw new ApplicationException("Unexpected QueueException: " + ex.EntryType);
			}
			return false;
		}
		
		volatile int count = 0;
		long startTime;
		public void StartTimeTheFeed()
		{
			startTime = Factory.TickCount;
			count = 0;
			countLog = 0;
			task = Factory.Parallel.Loop(this, OnException, TimeTheFeedTask);
		}
		
		private Exception propagateException = null;
		
		private void OnException( Exception ex) {
			propagateException = ex;	
		}

		public int EndTimeTheFeed(int expectedTickCount, int timeoutSeconds)
		{
			while( count < expectedTickCount && Factory.TickCount < startTime + timeoutSeconds * 1000) {
				if( propagateException != null) {
					throw propagateException;
				}
				Thread.Sleep(100);
			}
			log.Notice("Expected " + expectedTickCount + " and received " + count + " ticks.");
			log.Notice("Last tick received at : " + tickIO.ToPosition());
			Factory.TickUtil.TickQueue("Stats").LogStats();
			Dispose();
			if( propagateException != null) {
				throw propagateException;
			}
			return count;
		}

		public Yield TimeTheFeedTask()
		{
			lock(taskLocker) {
				if( isDisposed) {
					return Yield.Terminate;
				}
				try {
					if (!tickQueue.TryDequeue(ref tickBinary)) {
						return Yield.NoWork.Repeat;
					}
					if( keepReceived) {
						received.Add(tickBinary);
					}
					startTime = Factory.TickCount;
					tickIO.Inject(tickBinary);
					if (debug && count < 5) {
						log.Debug("Received a tick " + tickIO);
						countLog++;
					}
					if( count == 0) {
						log.Notice("First tick received: " + tickIO.ToPosition());
					}
					count++;
					if (count % 1000000 == 0) {
						log.Notice("Read " + count + " ticks");
					}
					if( SyncTicks.Enabled) tickSync.RemoveTick();
					return Yield.DidWork.Repeat;
				} catch (QueueException ex) {
					HandleQueueException(ex);
				}
				return Yield.NoWork.Repeat;
			}
		}
		public bool OnHistorical(SymbolInfo symbol)
		{
			try {
				return tickQueue.TryEnqueue(EventType.StartHistorical, symbol);
			} catch (QueueException) {
				// Queue was already ended.
			}
			return true;
		}

		public bool OnSend(TickBinaryBox o)
		{
			bool result = false;
			try {
				result = tickQueue.TryEnqueue(ref o.TickBinary);
				if( result) {
					tickPool.Free(o);
				}
			} catch (QueueException) {
				// Queue already terminated.
			}
			return result;
		}

		Dictionary<long, int> actualPositions = new Dictionary<long, int>();

		public double GetPosition(SymbolInfo symbol)
		{
			return actualPositions[symbol.BinaryIdentifier];
		}

		public bool OnLogicalFill(SymbolInfo symbol, LogicalFillBinary fill)
		{
			log.Info("Got Logical Fill of " + symbol + " at " + fill.Price + " for " + fill.Position);
			actualPositions[symbol.BinaryIdentifier] = fill.Position;
			if( SyncTicks.Enabled) tickSync.RemovePhysicalFill(fill);
			return true;
		}

		public bool OnStop()
		{
			Dispose();
			return true;
		}

		public bool OnError(ErrorDetail error)
		{
			OnException( new Exception(error.ErrorMessage));
			return true;
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
		            	if( task != null) {
			            	task.Stop();
			            	task.Join();
							tickQueue.Terminate();
		            	}
		            }
		            task = null;
		            // Leave tickQueue set so any extraneous
		            // events will see the queue is already terminated.
//		            tickQueue = null;
	    		}
    		}
	    }
	    
		public bool OnEndHistorical(SymbolInfo symbol)
		{
			try {
				return tickQueue.TryEnqueue(EventType.EndHistorical, symbol);
			} catch (QueueException) {
				// Queue was already ended.
			}
			return true;
		}

		public bool OnRealTime(SymbolInfo symbol)
		{
			try {
				return tickQueue.TryEnqueue(EventType.StartRealTime, symbol);
			} catch (QueueException) {
				// Queue was already ended.
			}
			return true;
		}


		public bool OnEndRealTime(SymbolInfo symbol)
		{
			try {
				return tickQueue.TryEnqueue(EventType.EndRealTime, symbol);
			} catch (QueueException) {
				// Queue was already ended.
			}
			return true;
		}
		

		public bool OnStartBroker(SymbolInfo symbol)
		{
			try {
				return tickQueue.TryEnqueue(EventType.StartBroker, symbol);
			} catch (QueueException) {
				// Queue was already ended.
			}
			return true;
		}


		public bool OnEndBroker(SymbolInfo symbol)
		{
			try {
				return tickQueue.TryEnqueue(EventType.EndBroker, symbol);
			} catch (QueueException) {
				// Queue was already ended.
			}
			return true;
		}
		
		public bool IsRealTime {
			get { return isRealTime; }
		}
		
		volatile SymbolInfo customEventSymbol;
		volatile int customEventType;
		volatile object customEventDetail;
		public bool OnCustomEvent(SymbolInfo symbol, int eventType, object eventDetail) {
			customEventSymbol = symbol;
			customEventType = eventType;
			customEventDetail = eventDetail;
			return true;
		}
		
		public bool OnEvent(SymbolInfo symbol, int eventType, object eventDetail) {
			if( isDisposed) return false;
			bool result = false;
			try {
				switch( (EventType) eventType) {
					case EventType.Tick:
						TickBinaryBox binary = (TickBinaryBox) eventDetail;
						result = OnSend(binary);
						break;
					case EventType.EndHistorical:
						result = OnEndHistorical(symbol);
						break;
					case EventType.StartHistorical:
						result = OnHistorical(symbol);
						break;
					case EventType.StartRealTime:
						result = OnRealTime(symbol);
						break;
					case EventType.EndRealTime:
						result = OnEndRealTime(symbol);
						break;
					case EventType.StartBroker:
						result = OnStartBroker(symbol);
						break;
					case EventType.EndBroker:
						result = OnEndBroker(symbol);
						break;
					case EventType.Error:
						result = OnError((ErrorDetail)eventDetail);
						break;
					case EventType.LogicalFill:
						result = OnLogicalFill(symbol,(LogicalFillBinary)eventDetail);
						break;
					case EventType.Terminate:
						result = OnStop();
			    		break;
					case EventType.Initialize:
					case EventType.Open:
					case EventType.Close:
					case EventType.PositionChange:
			    		throw new ApplicationException("Unexpected EventType: " + eventType);
					default:
			    		result = OnCustomEvent(symbol,eventType,eventDetail);
			    		break;
				}
				return result;
			} catch( QueueException) {
				log.Warn("Already terminated.");
			}
			return false;
		}
		
		public TickIO LastTick {
			get { return lastTick; }
		}
		
		public bool KeepReceived {
			get { return keepReceived; }
			set { keepReceived = value; }
		}
		
		public int PauseSeconds {
			get { return pauseSeconds; }
			set { pauseSeconds = value; }
		}
	}
}
