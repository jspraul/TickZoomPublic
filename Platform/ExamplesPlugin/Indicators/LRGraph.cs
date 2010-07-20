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
using TickZoom.Api;
using System.Collections.Generic;
using System.Drawing;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of EMA.
	/// </summary>
	public class LRGraph : IndicatorCommon
	{
		Channel fastLR;
		int fastLength = 5;
		Channel middleLR;
		IndicatorCommon middle;
		int middleLength = 15;
		Channel slowLR;
		IndicatorCommon slow;
		int slowLength = 45;
		
		public LRGraph() : base()
		{
			Drawing.GraphType = GraphType.Histogram;
			Drawing.PaneType = PaneType.Secondary;
			Drawing.GroupName = "Fast";
		}
		
		public override void OnInitialize() {
			slowLR = new Channel(Bars);
			middleLR = new Channel(Bars);
			fastLR = new Channel(Bars);
			middle = new IndicatorCommon();
			middle.Drawing.GraphType = GraphType.Histogram;
			middle.Drawing.PaneType = PaneType.Secondary;
			middle.Drawing.GroupName = "Middle";
			AddIndicator(middle);
			
			slow = new IndicatorCommon();
			slow.Drawing.GraphType = GraphType.Histogram;
			slow.Drawing.PaneType = PaneType.Secondary;
			slow.Drawing.GroupName = "Slow";
			AddIndicator(slow);
		}

		public override bool OnIntervalClose()
		{
			fastLR.addPoint(Bars.CurrentBar,Formula.Middle(Bars,0));
			if(fastLR.CountPoints >= fastLength) {
				fastLR.Calculate();
				fastLR.UpdateEnds();
				this[0]=fastLR.Slope;
			}
			
			middleLR.addPoint(Bars.CurrentBar,Formula.Middle(Bars,0));
			if(middleLR.CountPoints >= middleLength) {
				middleLR.Calculate();
				middleLR.UpdateEnds();
				middle[0]=middleLR.Slope;
			}
			
			slowLR.addPoint(Bars.CurrentBar,Formula.Middle(Bars,0));
			if(slowLR.CountPoints >= slowLength) {
				slowLR.Calculate();
				slowLR.UpdateEnds();
				slow[0]=slowLR.Slope;
			}
			return true;
		}
		
		public IndicatorCommon Middle {
			get { return middle; }
		}
		
		public IndicatorCommon Slow {
			get { return slow; }
		}
		public int Code {
			get { int fastBit = this[0]>1?1:0;
				  int middleBit = middle[0]>1?1:0;
				  int slowBit = slow[0]>0?1:0;
				  return slowBit * 4 + middleBit * 2 + fastBit;
			} 
		}
	}
}
