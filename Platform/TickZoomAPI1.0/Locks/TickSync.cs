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
		private int positionChange = 0;
		private int processPhysical = 0;
		private int physicalFills = 0;
		private int physicalOrders = 0;
		private SymbolInfo symbol;
		
		internal TickSync( long symbolId) {
			this.symbol = Factory.Symbol.LookupSymbol(symbolId);
			if( trace) log.Trace(symbol + ": created with binary symbol id = " + symbolId);
		}
		public bool Completed {
			get { var value = CheckCompletedInternal();
					return value;
			}
		}		
		private bool CheckCompletedInternal() {
			return ticks == 0 && positionChange == 0 &&
					physicalOrders == 0 && physicalFills == 0 &&
					processPhysical == 0;
		}		
		private bool CheckOnlyPhysicalFills() {
			return positionChange == 0 && physicalOrders == 0 &&
				physicalFills == 0 && processPhysical > 0;
		}		
		public void Clear() {
			if( !CheckCompletedInternal()) {
				System.Diagnostics.Debugger.Break();
				throw new ApplicationException(symbol + ": Tick, position changes, physical orders, and physical fills, must all complete before clearing the tick sync.");
			}
			ForceClear();
		}
		public void ForceClear() {
			var ticks = Interlocked.Exchange(ref this.ticks, 0);
			var orders = Interlocked.Exchange(ref physicalOrders, 0);
			var process = Interlocked.Exchange(ref processPhysical, 0);
			var changes = Interlocked.Exchange(ref positionChange, 0);
			var fills = Interlocked.Exchange(ref physicalFills, 0);
			Unlock();
			if( trace) log.Trace(symbol + ": Clear() " + this);
		}
		
		public override string ToString()
		{
			return "TickSync("+ticks+","+physicalOrders+","+positionChange+","+processPhysical+","+physicalFills+")";
		}
		
		public void AddTick() {
			var value = Interlocked.Increment( ref ticks);
			if( trace) log.Trace(symbol + ": AddTick("+value+")");
		}
		public void RemoveTick() {
			var value = Interlocked.Decrement( ref ticks);
			if( trace) log.Trace(symbol + ": RemoveTick("+value+")");
//			if( value < 0) {
//				System.Diagnostics.Debugger.Break();
//			}
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
//			if( order.Price.ToLong() == 97.46.ToLong()) {
//				int x = 0;
//			}
			var value = Interlocked.Increment( ref physicalOrders);
			if( trace) log.Trace(symbol + ": AddPhysicalOrder("+value+","+order+")");
		}
		public void RemovePhysicalOrder(object order) {
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
			var value = Interlocked.Increment( ref positionChange);
			if( trace) log.Trace(symbol + ": AddPositionChange("+value+")");
		}
		public void RemovePositionChange() {
			var value = Interlocked.Decrement( ref positionChange);
			if( trace) log.Trace(symbol + ": RemovePositionChange("+value+")");
			if( value < 0) {
				log.Warn(symbol + ": Completed: value below zero: " + positionChange);
			}
			if( value < 0) {
				System.Diagnostics.Debugger.Break();
			}
		}
		
		public void AddProcessPhysicalOrders() {
			var value = Interlocked.Increment( ref processPhysical);
			if( trace) log.Trace(symbol + ": AddProcessPhysicalOrders("+value+")");
		}
		public void RemoveProcessPhysicalOrders() {
			var value = Interlocked.Decrement( ref processPhysical);
			if( trace) log.Trace(symbol + ": RemoveProcessPhysicalOrders("+value+")");
			if( value < 0) {
				log.Warn(symbol + ": Completed: value below zero: " + processPhysical);
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
		
		public bool SentPositionChange {
			get { return positionChange > 0; }
		}
		
		public bool OnlyProcessPhysicalOrders {
			get { return CheckOnlyPhysicalFills(); }
		}
		
		public bool SentProcessPhysicalOrders {
			get { return processPhysical > 0; }
		}
	}
}
