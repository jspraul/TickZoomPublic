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
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of RandomStrategy.
	/// </summary>
	public class TEMATrendDaily : Strategy
	{
		int fast;
		int slow;
		SMA entrySma;
		TEMA tema;
		
		public TEMATrendDaily() : this(10,20)
		{
		}
		
		public TEMATrendDaily(int fast, int slow)
		{
			this.fast = fast;
			this.slow = slow;
		}
		
		public override void OnInitialize() {
			tema = new TEMA(Bars.Close,fast);
			AddIndicator( tema);
			entrySma = new SMA(Bars.Close,slow);
			AddIndicator( entrySma);
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( timeFrame.Equals(IntervalDefault)) {
				TimeStamp time = Days.Time[0];
				if( entrySma.Count > 0 && tema.Count > 0) {
					if( tema[0] > entrySma[0]) {
						Orders.Exit.ActiveNow.GoFlat();
					} else {
						Orders.Exit.ActiveNow.GoFlat();
					}
				}
			}
			return true;
		}
		
		public override string ToString()
		{
			return tema.Period + "," + entrySma.Period;
		}
	}
}
