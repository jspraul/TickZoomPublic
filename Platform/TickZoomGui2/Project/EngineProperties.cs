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
using System.ComponentModel;
using System.Drawing.Design;

using TickZoom.Api;

namespace TickZoom
{
	/// <summary>
	/// Description of EngineProperties.
	/// </summary>
	public class EngineProperties
	{
		public EngineProperties()
		{
			try {
				intervalDefault = Factory.Engine.DefineInterval(BarUnit.Default,0);
			} catch {
				
			}
		}
		
		bool enableTickFilter = false;
		
		[DefaultValue(false)]
		public bool EnableTickFilter {
			get { return enableTickFilter; }
			set { enableTickFilter = value; }
		}
		
		Interval intervalDefault;
		
		[Editor(typeof(IntervalPropertyEditor),typeof(UITypeEditor))]
		[TypeConverter(typeof(IntervalTypeConverter))]
		public Interval IntervalDefault {
			get { return intervalDefault; }
			set { intervalDefault = value; }
		}
		
		int breakAtBar = 0;
		
		[DefaultValue(0)]
		public int BreakAtBar {
			get { return breakAtBar; }
			set { breakAtBar = value; }
		}
		
		int maxBarsBack = 0;
		
		[DefaultValue(0)]
		public int MaxBarsBack {
			get { return maxBarsBack; }
			set { maxBarsBack = value; }
		}
		
		int maxTicksBack = 0;
		
		[DefaultValue(0)]
		public int MaxTicksBack {
			get { return maxTicksBack; }
			set { maxTicksBack = value; }
		}
		
		int tickReplaySpeed = 0;
		
		[DefaultValue(0)]
		public int TickReplaySpeed {
			get { return tickReplaySpeed; }
			set { tickReplaySpeed = value; }
		}

		int barReplaySpeed = 0;
		
		[DefaultValue(0)]
		public int BarReplaySpeed {
			get { return barReplaySpeed; }
			set { barReplaySpeed = value; }
		}
		
		public override string ToString()
		{
			return "";
		}
		
	}
}
