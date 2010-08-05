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
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of RandomStrategy.
	/// </summary>
	public class RandomIntraday : Strategy
	{
		TimeStamp[] randomEntries = null;
		int randomIndex = 0;
		Random random = new Random(393957857);
		int sessionHours = 4;
		
		public RandomIntraday()
		{
			randomEntries = new TimeStamp[20];
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			TimeStamp tickTime = Ticks[0].Time;
			if( Sessions.IsActive &&
			    randomIndex < randomEntries.Length &&
			    tickTime > randomEntries[randomIndex])
			{
				int randSignal = randomEntries[randomIndex].Second % 2;
				Position.Change( randSignal == 1 ? 1 : -1);
				
				randomIndex ++;
			}
			return true;
		}

		public override bool OnIntervalOpen(Interval timeFrame) {
			TimeStamp test = Ticks[0].Time;
			for( int i =0; i < randomEntries.Length; i++) {
				randomEntries[i] = Sessions.Time[0];
				randomEntries[i].AddSeconds(random.Next(0,sessionHours*60*60));
			}
			Array.Sort(randomEntries,0,randomEntries.Length);
			randomIndex = 0;
			return true;
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			Orders.Exit.ActiveNow.GoFlat();
			return true;
		}
	}
}
