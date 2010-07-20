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

namespace TickZoom.Common
{
	/// <summary>
	/// Description of Point.
	/// </summary>
	public class ChartPoint : IComparable<ChartPoint>
	{
		private double barX = 0;
		private double priceY = 0;
		public int Bar {
			get { return (int) barX; }
			set { barX = value; }
		}
		public double X {
			get { return barX; }
			set { barX = value; }
		}
		
		public double Price {
			get { return priceY; }
			set { priceY = value; }
		}
		
		public double Y {
			get { return priceY; }
			set { priceY = value; }
		}
		
		public ChartPoint()
		{
		}
		
		public ChartPoint(double barX, double priceY)
		{
			this.barX = barX;
			this.priceY = priceY;
		}
		public int CompareTo( ChartPoint other) {
			return other.barX == barX ? 0 : (other.barX > barX ? 1 : -1);
		}
	}
}
