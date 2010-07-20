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
using System.Drawing;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of Line.
	/// </summary>
	public class Line 
	{
		int interceptBar;
		
		int bar1;
		int y1;
		int bar2;
		int y2;		
		
		LinearRegression lr = new LinearRegression();
		
		public Line(int interceptBar)
		{
			this.interceptBar = interceptBar;
		}
		
		public int LastBar {
			get { return interceptBar; }
		}
		
		public int GetYFromBar(int bar) {
			return GetY(interceptBar - bar);
		}
		
		public void setPoints(Point p1, Point p2) {
			setPoints( p1.X,p1.Y,p2.X,p2.Y);
		}
		
		public void setPoints(int bar1, int y1, int bar2, int y2) {
			lr.clearPoints();
			this.bar1 = bar1;
			this.y1 = y1;
			this.bar2 = bar2;
			this.y2 = y2;
			lr.addPoint( interceptBar - bar1, y1);
			lr.addPoint( interceptBar - bar2, y2);
		}
		
		public void calculate() {
			lr.calculate();
		}
		
		public int GetY(int x) {
			return (int) (lr.Intercept + x*lr.Slope);
		}
		
		int highest = 0;
		int lowest = 0;
		public void calcMaxDev(Bars bars, int length) {
			// Find max deviation from the line.
			highest = 0;
			lowest = 0;
			for( int bar = 0; bar < length; bar++) {
				double lineY = GetY(bar);
				double highY = bars.High[bar];
				double lowY = bars.Low[bar];
				if( highY - lineY > highest) {
					highest = (int) (highY - lineY);
				}
				if( lineY - lowY > lowest) {
					lowest = (int) (lineY - lowY);
				}
			}
		}
		
		public bool IsDown {
			get { double slope = lr.Slope;
				  return lr.Slope >= 0; }
		}
		
		public bool IsUp {
			get { double slope = lr.Slope;
				  return lr.Slope < 0; }
		}
		
		public void extend( int length) {
			bar1 += length;
			y1 = GetYFromBar(bar1);			
		}
		
		public int Bar1 {
			get { return bar1; }
		}
		
		public int Y1 {
			get { return y1; }
		}
		
		public int Bar2 {
			get { return bar2; }
		}
		
		public int Y2 {
			get { return y2; }
		}
		
		public int Length {
			get{ return interceptBar - bar2; }
		}
		
		public int Highest {
			get { return highest; }
		}
		
		public int Lowest {
			get { return lowest; }
		}
	}
}
