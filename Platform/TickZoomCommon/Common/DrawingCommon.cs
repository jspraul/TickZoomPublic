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
using System.Drawing;

using TickZoom.Api;

namespace TickZoom.Common
{
	[Diagram(AttributeExclude=true)]
	public class DrawingCommon : DrawingInterface {
		Color color;
		ModelInterface model;
		string groupName;
		bool alreadyDrawn = false;
		GraphType graphType;
		PaneType paneType = PaneType.Primary;
		double scaleMax = Double.NaN;
		double scaleMin = Double.NaN;
		int colorIndex = 0;
		bool isVisible = false;
		
		public DrawingCommon(ModelInterface model)
		{
			this.model = model;
		}
		
		public Color Color {
			get { return color; }
			set { color = value; }
		}
		
		public string GroupName {
			get { return groupName; }
			set { groupName = value; }
		}
		
		public bool AlreadyDrawn {
			get { return alreadyDrawn; }
			set { alreadyDrawn = value; }
		}
		
		public int ColorIndex {
			get { return colorIndex; }
			set { colorIndex = value; }
		}
		
		public double ScaleMax {
			get { return scaleMax; }
			set { scaleMax = value; }
		}
		
		public double ScaleMin {
			get { return scaleMin; }
			set { scaleMin = value; }
		}
		
		public GraphType GraphType {
			get { return graphType; }
			set { graphType = value; }
		}
		
		public PaneType PaneType {
			get { return paneType; }
			set { paneType = value; }
		}
		
		public bool IsVisible {
			get { return isVisible; }
			set { isVisible = value; }
		}
	}
}
