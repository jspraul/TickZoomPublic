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
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using TickZoom.Api;
using TickZoom.Interceptors;

namespace TickZoom.Common
{
	[Diagram(AttributeExclude=true)]
	public class BreakPoint : StrategyInterceptor, BreakPointInterface {
		private StrategyInterface strategy;
		private static int count;
		private static TimeStamp time;
		private static BreakMode breakMode;
		private static BreakType breakType;
		private static ConstraintType constraintType;
		private static string constraint;
		private static LocationType locationType = LocationType.Platform;
		
		public bool TrySetEngine(Data data) {
			if( breakMode == BreakMode.None ) {
				return false;
			}
			if( locationType != LocationType.Engine) {
				return false;
			}
			if( constraintType == ConstraintType.Symbol) {
				return data.SymbolInfo.Symbol == constraint;
			}
			return true;
		}
		
		[Conditional("DEBUG")]
		public static void TrySetStrategy(StrategyInterface strategy) {
			if( breakMode == BreakMode.None || locationType != LocationType.Platform) {
				return;
			}
			BreakPoint bp;
			switch( constraintType) {
				case ConstraintType.Strategy:
					if( strategy.Name == constraint) {
						bp = new BreakPoint();
						bp.strategy = strategy;
						strategy.AddInterceptor(bp);
					}
					break;
				case ConstraintType.Symbol:
					if( strategy.SymbolDefault == constraint) {
						bp = new BreakPoint();
						bp.strategy = strategy;
						strategy.AddInterceptor(bp);
					}
					break;
				default:
					bp = new BreakPoint();
					bp.strategy = strategy;
					strategy.AddInterceptor(bp);
					break;
			}
		}
	
		public BreakPoint() {
		}
		
		public override void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			if( eventType == EventType.Initialize) {
				if( breakMode == BreakMode.Bar) {
					strategy.AddInterceptor(EventType.Open,this);
				}
				if( breakMode == BreakMode.Tick) {
					strategy.AddInterceptor(EventType.Tick,this);
				}
			}
			switch(breakMode) {
				case BreakMode.Bar:
					if( eventType == EventType.Open) {
						TryBarBreakPoint(strategy.Bars);
					}
					break;
				case BreakMode.Tick:
					if( eventType == EventType.Tick) {
						TryTickBreakPoint((Tick) eventDetail);
					}
					break;
			}
			context.Invoke();
		}
	
		public void TryTickBreakPoint(Tick tick) {
			if( breakMode != BreakMode.Tick) {
				return;
			}
			if( breakType == BreakType.Time) {
				if( tick.Time >= time) {
					breakMode = BreakMode.None;
					BreakOnATick();
				}
			}
			if( breakType == BreakType.Count) {
				if( strategy.Data.Ticks.BarCount == count) {
					breakMode = BreakMode.None;
					BreakOnATick();
				}
			}
		}
			  
		public void TryBarBreakPoint(Bars bars) {
			if( breakMode != BreakMode.Bar) {
				return;
			}
			if( breakType == BreakType.Count) {
				if( bars.BarCount == count) {
					breakMode = BreakMode.None;
					BreakOnABar();
				}
			}
			if( breakType == BreakType.Time) {
				if( bars.Time[0] >= time) {
					breakMode = BreakMode.None;
					BreakOnABar();
				}
			}
		}
		
		private void BreakOnABar() {
			// This break point stops at the very beginning of the
			// bar that you selected for break point. Next, you
			// may add additional break points in your code to examine
			// what's happening in your code for a specific bar.
			System.Diagnostics.Debugger.Break();
		}
		
		private void BreakOnATick() {
			// This break point stops at the first point this tick
			// starts processing. Next, you may add additional break
			// points in your code to examine what's happening in
			// your code for a specific bar.
			System.Diagnostics.Debugger.Break();
		}
		
		public static void SetBarBreakPoint(int value) {
			breakMode = BreakMode.Bar;
			breakType = BreakType.Count;
			count = value;
		}
		
		public static void SetBarBreakPoint(TimeStamp value) {
			breakMode = BreakMode.Bar;
			breakType = BreakType.Time;
			time = value;
		}
		
		public static void SetBarBreakPoint(string value) {
			SetBarBreakPoint(new TimeStamp(value));
		}
		
		public static void SetTickBreakPoint(int value) {
			breakMode = BreakMode.Tick;
			breakType = BreakType.Count;
			count = value;
		}
		
		public static void SetTickBreakPoint(TimeStamp value) {
			breakMode = BreakMode.Tick;
			breakType = BreakType.Time;
			time = value;
		}
		
		public static void SetTickBreakPoint(string value) {
			SetTickBreakPoint( new TimeStamp(value));
		}
		
		public static void SetStrategyConstraint(string strategy) {
			constraintType = ConstraintType.Strategy;
			constraint = strategy;
		}
		
		public static void SetSymbolConstraint(string symbol) {
			constraintType = ConstraintType.Symbol;
			constraint = symbol;
		}
		
		public static void SetEngineConstraint() {
			locationType = LocationType.Engine;
		}
		
		public bool IsSymbolConstraint {
			get { return constraintType == ConstraintType.Symbol; }
		}
		
		public enum LocationType {
			Platform,
			Engine
		}
		
		public enum ConstraintType {
			None,
			Strategy,
			Symbol,
		}
		
		public enum BreakMode {
			None,
			Bar,
			Tick
		}
		
		public enum BreakType {
			None,
			Count,
			Time,
		}
		
	}
}
