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

namespace TickZoom.Interceptors
{
	/// <summary>
	/// Description of Orders.
	/// </summary>
	public class OrderHandlers
	{
		EnterTiming enter;
		ExitTiming exit;
		ReverseTiming reverse;
		ChangeTiming change;
		
		public OrderHandlers(EnterCommon enterNow, EnterCommon enterNextBar,
		                     ExitCommon exitNow, ExitCommon exitNextBar,
		                     ReverseCommon reverseNow, ReverseCommon reverseNextBar,
		                     ChangeCommon changeNow, ChangeCommon changeNextBar)
		{
			this.enter = new EnterTiming(enterNow,enterNextBar);
			this.exit = new ExitTiming(exitNow,exitNextBar);
			this.reverse = new ReverseTiming(reverseNow,reverseNextBar);
			this.change = new ChangeTiming(changeNow,changeNextBar);
		}
		
		public EnterTiming Enter {
			get { return enter; }
		}
		
		public ExitTiming Exit {
			get { return exit; }
		}
		
		public ReverseTiming Reverse {
			get { return reverse; }
		}
		
		public OrderHandlers.ChangeTiming Change {
			get { return change; }
		}
		
		public class EnterTiming {
			EnterCommon activeNow;
			EnterCommon nextBar;
			
			public EnterTiming( EnterCommon now, EnterCommon nextBar) {
				this.activeNow = now;
				this.nextBar = nextBar;
			}
			
			public EnterCommon ActiveNow {
				get { return activeNow; }
			}
			
			public EnterCommon NextBar {
				get { return nextBar; }
			}
		}
		
		public class ReverseTiming {
			ReverseCommon activeNow;
			ReverseCommon nextBar;
			
			public ReverseTiming( ReverseCommon now, ReverseCommon nextBar) {
				this.activeNow = now;
				this.nextBar = nextBar;
			}
			
			public ReverseCommon ActiveNow {
				get { return activeNow; }
			}
			
			public ReverseCommon NextBar {
				get { return nextBar; }
			}
		}
		
		public class ChangeTiming {
			ChangeCommon activeNow;
			ChangeCommon nextBar;
			
			public ChangeTiming( ChangeCommon now, ChangeCommon nextBar) {
				this.activeNow = now;
				this.nextBar = nextBar;
			}
			
			public ChangeCommon ActiveNow {
				get { return activeNow; }
			}
			
			public ChangeCommon NextBar {
				get { return nextBar; }
			}
		}
		
		public class ExitTiming {
			ExitCommon activeNow;
			ExitCommon nextBar;
			
			public ExitTiming( ExitCommon now, ExitCommon nextBar) {
				this.activeNow = now;
				this.nextBar = nextBar;
			}
			
			public ExitCommon ActiveNow {
				get { return activeNow; }
			}
			
			public ExitCommon NextBar {
				get { return nextBar; }
			}
		}
	}
}
