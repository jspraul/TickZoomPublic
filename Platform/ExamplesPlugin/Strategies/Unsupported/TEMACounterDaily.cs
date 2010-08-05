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
	public class TEMACounterDaily : Strategy
	{
		int slow = 28;
		int fast = 14;
		TEMA slowTema;
		TEMA fastTema;
		
		public TEMACounterDaily()
		{
			// Set defaults here.
		}
		
		public override void OnInitialize() 
		{
			slowTema = new TEMA(Bars.Close,slow);
			slowTema.Drawing.Color = Color.Orange;
			fastTema = new TEMA(Bars.Close,fast);
			AddIndicator(slowTema);
			AddIndicator(fastTema);
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			if( fastTema.Count>0 && slowTema.Count>0) {
				Position.Change(fastTema[0] > slowTema[0] ? -1 : 1);
			} else {
				Orders.Exit.ActiveNow.GoFlat();
			}
			return true;
		}
		
		public override string ToString()
		{
			return fast + "," + slow ;
		}
		
		public int Slow {
			get { return slow; }
			set { slow = value; }
		}
		
		public int Fast {
			get { return fast; }
			set { fast = value; }
		}

	}
}
