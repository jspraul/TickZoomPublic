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
using System.Text;
using TickZoom.Api;

namespace TickZoom.MBTFIX
{
	public class FIXTBuffer {
		private StringBuilder sb = new StringBuilder();
		public const char EndField = (char) 1;
		public const string EndFieldStr = "";
		public void Append(int key, string value) {
			sb.Append(key);
			sb.Append("=");
			sb.Append(value);
			sb.Append(EndField); 
		}
		public void Append(int key, int value) {
			sb.Append(key);
			sb.Append("=");
			sb.Append(value);
			sb.Append(EndField); 
		}
		public void Append(int key, double value) {
			sb.Append(key);
			sb.Append("=");
			sb.Append(value);
			sb.Append(EndField); 
		}
		public void Append(int key, TimeStamp time) {
			sb.Append(key);
			sb.Append("=");
			sb.Append(time.Year);
			sb.AppendFormat("{0:00}",time.Month);
			sb.AppendFormat("{0:00}-",time.Day);
			sb.AppendFormat("{0:00}:",time.Hour);
			sb.AppendFormat("{0:00}:",time.Minute);
			sb.AppendFormat("{0:00}",time.Second);
			sb.Append(EndField); 
		}
		public void Insert(string header) {
			sb.Insert(0,header);
		}
		public void Clear() {
			sb.Length = 0;
		}
		public int Length {
			get { return sb.Length; }
		}
		public override string ToString()
		{
			return sb.ToString();
		}
	}
}
