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
	public class TickSync : SimpleLock {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TickSync));
		private static readonly bool debug = log.IsDebugEnabled;		
		private static readonly bool trace = log.IsTraceEnabled;		
		private int ticks = 0;
		private int positionChanges = 0;
		private int physicalFills = 0;
		private int physicalOrders = 0;
		private SymbolInfo symbol;
		
		internal TickSync( long symbolId) {
			this.symbol = Factory.Symbol.LookupSymbol(symbolId);
			if( trace) log.Trace(symbol + ": created with binary symbol id = " + symbolId);
		}
		public bool Completed {
			get { var value = CompletedInternal;
//					if( trace && value) log.Trace(symbol + ": Completed()");
					return value;
			}
		}		
		private bool CompletedInternal {
			get { var value = ticks == 0 && positionChanges == 0 &&
					physicalOrders == 0 && physicalFills == 0;
					return value;
			}
		}		
		public void Clear() {
			if( !CompletedInternal) {
				System.Diagnostics.Debugger.Break();
				throw new ApplicationException(symbol + ": Tick, position changes, physical orders, and physical fills, must all complete before clearing the tick sync.");
			}
			ForceClear();
		}
		public void ForceClear() {
			if( this.ticks < 0) {
				System.Diagnostics.Debugger.Break();
			}
			var ticks = Interlocked.Exchange(ref this.ticks, 0);
			var orders = Interlocked.Exchange(ref physicalOrders, 0);
			var changes = Interlocked.Exchange(ref positionChanges, 0);
			var fills = Interlocked.Exchange(ref physicalFills, 0);
			Unlock();
			if( trace) log.Trace(symbol + ": Clear("+ticks+","+orders+","+changes+","+fills+")");
		}
		public void AddTick() {
			var value = Interlocked.Increment( ref ticks);
			if( trace) log.Trace(symbol + ": AddTick("+value+")");
		}
		public void RemoveTick() {
			var value = Interlocked.Decrement( ref ticks);
			if( trace) log.Trace(symbol + ": RemoveTick("+value+")");
			if( value < 0) {
				System.Diagnostics.Debugger.Break();
			}
		}
		public void AddPhysicalFill(PhysicalFill fill) {
			var value = Interlocked.Increment( ref physicalFills);
			if( trace) log.Trace(symbol + ": AddPhysicalFill("+value+","+fill+","+fill.Order+")");
		}
		public void RemovePhysicalFill(object fill) {
			var value = Interlocked.Decrement( ref physicalFills);
			if( trace) log.Trace(symbol + ": RemovePhysicalFill("+value+","+fill+")");
			if( value < 0) {
				System.Diagnostics.Debugger.Break();
			}
		}
		public void AddPhysicalOrder(PhysicalOrder order) {
			var value = Interlocked.Increment( ref physicalOrders);
			if( trace) log.Trace(symbol + ": AddPhysicalOrder("+value+","+order+")");
		}
		public void RemovePhysicalOrder(PhysicalOrder order) {
			var value = Interlocked.Decrement( ref physicalOrders);
			if( trace) log.Trace(symbol + ": RemovePhysicalOrder("+value+","+order+")");
			if( value < 0) {
				System.Diagnostics.Debugger.Break();
			}
		}
		public void RemovePhysicalOrder() {
			var value = Interlocked.Decrement( ref physicalOrders);
			if( trace) log.Trace(symbol + ": RemovePhysicalOrder("+value+")");
			if( value < 0) {
				System.Diagnostics.Debugger.Break();
			}
		}
		public void AddPositionChange() {
			var value = Interlocked.Increment( ref positionChanges);
			if( trace) log.Trace(symbol + ": AddPositionChange("+value+")");
		}
		public void RemovePositionChange() {
			var value = Interlocked.Decrement( ref positionChanges);
			if( trace) log.Trace(symbol + ": RemovePositionChange("+value+")");
			if( value < 0) {
				log.Warn(symbol + ": Completed: value below zero: " + positionChanges);
			}
			if( value < 0) {
				System.Diagnostics.Debugger.Break();
			}
		}
		
		public bool SentPhysicalOrders {
			get { return physicalOrders > 0; }
		}
		
		public bool SentPhysicalFills {
			get { return physicalFills > 0; }
		}
	}
}
