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

namespace TickZoom.Api
{
		
	public enum YieldStatus {
		None,
		Invoke,
		Return,
		Repeat,
		Pause,
		Terminate,
	}
	
	public delegate Yield YieldMethod();
	public struct Yield {
		public bool IsIdle;
		public YieldMethod Method;
		public YieldStatus Status;
		
		public Yield Invoke(YieldMethod method) {
			Method = method;
			Status = YieldStatus.Invoke;
			return this;
		}
		
		public Yield Return {
			get {
				Status = YieldStatus.Return;
				return this;
			}
		}
		
		public Yield Repeat {
			get {
				Status = YieldStatus.Repeat;
				return this;
			}
		}
		
		public static Yield Terminate {
			get {
				Yield yield = new Yield();
				yield.Status = YieldStatus.Terminate;
				return yield;
			}
		}
		
		public static Yield Pause {
			get {
				Yield yield = new Yield();
				yield.Status = YieldStatus.Pause;
				return yield;
			}
		}
		
		public static Yield DidWork {
			get {
				return new Yield();
			}
		}
		
		public static Yield NoWork {
			get {
				Yield yield = new Yield();
				yield.IsIdle = true;
				return yield;
			}
		}
	}
	
	public interface ParallelStarter
	{
		void Once(object creator, Action<Exception> onException, Action once);
		Task Loop(object creator, Action<Exception> onException, YieldMethod loop);
		ForLoop ForLoop(object creator, int start, int count);
		void While(object creator, Action<Exception> onException, Func<bool> loop);
	}
	
	public interface ForLoop {
		void For(Action<int> loop);
	}
		
	public interface Parallel : ParallelStarter
	{
		void Yield();
		void Sleep(int millis);
		string GetStats();
		Task CurrentTask {
			get;
		}
		Task[] Tasks {
			get;
		}
		long TickCount { get; }
	}
}
