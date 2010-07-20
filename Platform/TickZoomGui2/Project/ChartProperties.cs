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
	/// Description of ChartProperties.
	/// </summary>
	public class ChartProperties
	{
		Interval intervalChartBar;
		Interval intervalChartDisplay;
		Interval intervalChartUpdate;
		
		public ChartProperties()
		{
			// Avoid exceptions during design mode.
			try {
				intervalChartUpdate = Factory.Engine.DefineInterval(BarUnit.Default,0);
				intervalChartDisplay = Factory.Engine.DefineInterval(BarUnit.Default,0);
				intervalChartBar = Factory.Engine.DefineInterval(BarUnit.Default,0);
			} catch {
				
			}
		}

		bool showPriceGraph = true;
		
		[DefaultValue(true)]
		public bool ShowPriceGraph {
			get { return showPriceGraph; }
			set { showPriceGraph = value; }
		}
		
		[Editor(typeof(IntervalPropertyEditor),typeof(UITypeEditor))]
		[TypeConverter(typeof(IntervalTypeConverter))]
		public Interval IntervalChartDisplay {
			get { return intervalChartDisplay; }
			set { intervalChartDisplay = value; }
		}
		
		[Editor(typeof(IntervalPropertyEditor),typeof(UITypeEditor))]
		[TypeConverter(typeof(IntervalTypeConverter))]
		public Interval IntervalChartBar {
			get { return intervalChartBar; }
			set { intervalChartBar = value; }
		}
		
		[Editor(typeof(IntervalPropertyEditor),typeof(UITypeEditor))]
		[TypeConverter(typeof(IntervalTypeConverter))]
		public Interval IntervalChartUpdate {
			get { return intervalChartUpdate; }
			set { intervalChartUpdate = value; }
		}

		ChartType chartType = ChartType.Bar;
		[DefaultValue(ChartType.Bar)]
		public ChartType ChartType {
			get { return chartType; }
			set { chartType = value; }
		}
		
		public override string ToString()
		{
			return "";
		}
	}
}
