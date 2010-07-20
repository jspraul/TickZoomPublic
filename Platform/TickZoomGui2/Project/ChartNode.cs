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
using System.Windows.Forms;

namespace TickZoom.Portfolio
{
	/// <summary>
	/// Description of Portfolio.
	/// </summary>
	public class ChartNode : TreeNode
	{
		public ChartNode() : base()
		{
			Tag = GetChart();
			this.Text = "Chart Settings";
		}
		
		private object GetChart() {
			ChartProperties chart = new ChartProperties();
			PropertyTable properties = new PropertyTable(chart);
//			Starter starter = new DesignStarter();
//			starter.Model = engine;
//			starter.Run();
//			properties.SetAfterInitialize();
//			LoadIndicators(engine);
			return properties;
		}
		
	}
}
