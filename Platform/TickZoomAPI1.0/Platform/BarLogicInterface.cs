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

        
	public interface BarLogicInterface : IDisposable {
		void InitializeBar( Tick tick, BarData data);
		/// <summary>
		/// Determines if this tick will cause a new bar to get
		/// created. If so, the engine will fire the OnIntervalClose()
		/// events for the symbol and then call ProcessTick() 
		/// to actually add the new bar. The engine always calls
		/// ProcessTick() for every tick. So most of the time it
		/// will simply update the information for the current bar.
		/// </summary>
		/// <param name="tick">The current tick to use in your determination of new bar status.</param>
		/// <returns>true/false whether to end the current bar.</returns>
	    bool IsNewBarNeeded(Tick tick);
	    /// <summary>
	    /// Use the tick to update or create a new bar or bars.
	    /// Make sure you only create new bars after IsNewBarNeeded()
	    /// returns true so that OnIntervalClose() events get fired
	    /// appropriately.
	    /// </summary>
	    /// <param name="tick">The current tick.</param>
	    /// <param name="data">This interface has method to create a bar or update the current bar.</param>
	    void UpdateBar(Tick tick, BarData data);
	    /// <summary>
	    /// This method will determine if the current bar must end.
	    /// Ordinarily bars end and begin at the same boundaries. One
	    /// example of an exception to this are session bars. Sessions
	    /// start and end at specific times. The end of bar events 
	    /// must be called long before the next open in some cases.
	    /// 
	    /// NOTE: if your bars always end and start at the same point
	    /// then simply throw the NotImplementedException() and the
	    /// engine will never call this method again on your type.
	    /// </summary>
	    /// <param name="tick"></param>
	    /// <returns>true/false whether it's time to end the current bar</returns>
	    bool IsEndBarNeeded(Tick tick);
	}
}
