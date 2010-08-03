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
using System.Text;

using TickZoom.Api;

namespace TickZoom.Common
{
	public class StrategyWatcher {
		private double previousPosition = 0;
		private PositionInterface position;
		private StrategyInterface strategy;
		
		public StrategyWatcher(StrategyInterface strategy) {
			this.strategy = strategy;
			this.position = strategy.Result.Position;
		}
		
		public bool IsActive {
			get { return this.strategy.IsActive; }
		}
		
		public bool PositionChanged {
			get { return previousPosition != position.Current; }
		}
		
		public Iterable<LogicalOrder> ActiveOrders {
			get { return strategy.ActiveOrders; }
		}
		
		public bool IsActiveOrdersChanged {
			get { return strategy.IsActiveOrdersChanged; }
			set { strategy.IsActiveOrdersChanged = false; }
		}
		
		public void Refresh() {
			previousPosition = position.Current;
		}
		
		public PositionInterface Position {
			get { return position; }
		}
	}
}
