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

using System.Drawing;
using TickZoom.Api;

namespace TickZoom.Presentation
{
    public class ChartLine : ChartItem
    {
        private int bar1;
        private int bar2;
        private Color color;
        private double price1;
        private double price2;
        private LineStyle style;

        public ChartLine(Color color, int bar1, double price1, int bar2, double price2, LineStyle style)
        {
            this.color = color;
            this.bar1 = bar1;
            this.price1 = price1;
            this.bar2 = bar2;
            this.price2 = price2;
            this.style = style;
        }
    }
}