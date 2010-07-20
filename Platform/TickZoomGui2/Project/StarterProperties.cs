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
	/// Description of StarterProperties.
	/// </summary>
	public class StarterProperties 
	{
		string symbol = "USD/JPY";
		TimeStamp startTime;
		TimeStamp endTime;
		
		public StarterProperties()
		{
			startTime = new TimeStamp(2000,1,1);
			endTime = new TimeStamp(DateTime.Now);
		}
		
		public string Symbol {
			get { return symbol; }
			set { symbol = value; }
		}
		
		[Editor(typeof(TimestampPropertyEditor),typeof(UITypeEditor))]
		[TypeConverter(typeof(TimestampTypeConverter))]
		public TimeStamp StartTime {
			get { return startTime; }
			set { startTime = value; }
		}
		
		[Editor(typeof(TimestampPropertyEditor),typeof(UITypeEditor))]
		[TypeConverter(typeof(TimestampTypeConverter))]
		public TimeStamp EndTime {
			get { return endTime; }
			set { endTime = value; }
		}
		
		Interval intervalDefault;
		[Editor(typeof(IntervalPropertyEditor),typeof(UITypeEditor))]
		[TypeConverter(typeof(IntervalTypeConverter))]
		public Interval IntervalDefault {
			get { return intervalDefault; }
			set { intervalDefault = value; }
		}
		
		public override string ToString()
		{
			return "";
		}
	}
}