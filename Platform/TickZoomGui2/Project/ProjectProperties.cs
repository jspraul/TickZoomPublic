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
using System.Reflection;

using TickZoom.Api;

namespace TickZoom
{
	/// <summary>
	/// Description of ProjectProperties.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class ProjectProperties 
	{
		ChartProperties chartProperties = new ChartProperties();
		EngineProperties engineProperties = new EngineProperties();
		StarterProperties starterProperties = new StarterProperties();
		
		public ProjectProperties()
		{
		}
		
		public StarterProperties Starter {
			get { return starterProperties; }
			set { starterProperties = value; }
		}
		
		public ChartProperties Chart {
			get { return chartProperties; }
			set { chartProperties = value; }
		}
		
		public EngineProperties Engine {
			get { return engineProperties; }
			set { engineProperties = value; }
		}
	}
}