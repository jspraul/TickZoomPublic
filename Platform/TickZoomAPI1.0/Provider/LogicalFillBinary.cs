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
using System.Text;

using TickZoom.Api;

namespace TickZoom.Api
{
	    
	public struct LogicalFillBinary : LogicalFill
	{
		private int position;
		private double price;
		private TimeStamp time;
		private TimeStamp utcTime;
		private TimeStamp postedTime;
		private int orderId;
		private long orderSerialNumber;
		public LogicalFillBinary(int position, double price, TimeStamp time, TimeStamp utcTime, int orderId, long orderSerialNumber)
		{
			this.position = position;
			this.price = price;
			this.time = time;
			this.utcTime = utcTime;
			this.orderId = orderId;
			this.orderSerialNumber = orderSerialNumber;
			this.postedTime = new TimeStamp(1800,1,1);
//			if( orderId == 0) {
//				System.Diagnostics.Debugger.Break();
//			}
		}

		public int OrderId {
			get { return orderId; }
		}
		
		public long OrderSerialNumber {
			get { return orderSerialNumber; }
		}
		
		public TimeStamp Time {
			get { return time; }
		}

		public TimeStamp UtcTime {
			get { return utcTime; }
		}
		
		public TimeStamp PostedTime {
			get { return postedTime; }
			set { postedTime = value; }
		}
		
		public double Price {
			get { return price; }
		}

		public int Position {
			get { return position; }
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( orderId);
			sb.Append( ",");
			sb.Append( orderSerialNumber);
			sb.Append( ",");
			sb.Append( price);
			sb.Append( ",");
			sb.Append( position);
			sb.Append( ",");
			sb.Append( time);
			sb.Append( ",");
			sb.Append( utcTime);
			sb.Append( ",");
			sb.Append( postedTime);
			sb.Append(",");
			sb.Append(TimeStamp.UtcNow.Internal);
			return sb.ToString();
		}

		public static LogicalFillBinary Parse(string value) {
			string[] fields = value.Split(',');
			int field = 0;
			var orderId = int.Parse(fields[field++]);
			var orderSerialNumber = long.Parse(fields[field++]);
			var price = double.Parse(fields[field++]);
			var position = int.Parse(fields[field++]);
			var time = TimeStamp.Parse(fields[field++]);
			var utcTime = TimeStamp.Parse(fields[field++]);
			var postedTime = TimeStamp.Parse(fields[field++]);
			var fill = new LogicalFillBinary(position,price,time,utcTime,orderId,orderSerialNumber);
			fill.postedTime = postedTime;
			return fill;
		}		
	}
}
