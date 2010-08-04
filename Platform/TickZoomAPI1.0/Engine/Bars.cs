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

namespace TickZoom.Api
{
	/// <summary>
	/// Description of DataSeries.
	/// </summary>
	public interface Bars
	{
		/// <summary>
		/// This is the count of bars actually in the array. This is useful for 
		/// for loops like for(int i=0; i<Bars.Count; i++)
		/// </summary>
		int Count {
			get;
		}		
		
		/// <summary>
		/// This is the total number of bars including the current bar.
		/// That makes the first bar number equal to 1.
		/// This number matches what you see are the charts.
		/// BarCount can be greater than Count since Bars
		/// have a maximum Capacity limit for performance.	
		/// </summary>
		int BarCount {
			get;
		}		
		
		/// <summary>
		/// The total Capacity of Bars which means the maximum
		/// value of the Count property. But this doesn't limit BarCount
		/// or CurrentBar.
		/// </summary>
		int Capacity {
			get;
		}		
		
		/// <summary>
		/// CurrentBar the bar index which means that it assigns
		/// the first bar a value of zero. This was added for compatibility
		/// with other platform. It simply equals BarCount-1.
		/// </summary>
		int CurrentBar {
			get;
		}
		
		Prices Open {
			get;
		}
		
		Prices High {
			get;
		}
		
		Prices Low {
			get;
		}
		
		Prices Close {
			get;
		}
		
		Prices Sentiment {
			get;
		}
		
		Prices Typical {
			get;
		}
		
		Times Time {
			get;
		}
		
		Times EndTime {
			get;
		}
		
		Numbers Volume {
			get;
		}

		Numbers TickCount {
			get;
		}
		
		bool IsActive {
			get;
		}
		
		Interval Interval {
			get;
		}
	}
	

}
