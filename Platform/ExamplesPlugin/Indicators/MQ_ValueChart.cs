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
using TickZoom.Common;

namespace TickZoom.Examples.Indicators
{
	public class MQ_ValueChart : IndicatorCommon
	{
	               
	        /// <summary>
	        /// Four nested indicators to be displayed as a price bar chart
	        /// </summary>
	        IndicatorCommon vcOpen;
	        IndicatorCommon vcHigh;
	        IndicatorCommon vcLow;
	        IndicatorCommon vcClose;
	               
	        public MQ_ValueChart()
	        {
	        }
	
	        public override void OnInitialize() {
	
	            vcOpen = new IndicatorCommon();
	            vcOpen.Drawing.GroupName = "ValueChart";
	            vcOpen.Drawing.PaneType = PaneType.Secondary;
	            vcOpen.Drawing.IsVisible = true;
	            vcOpen.Drawing.GraphType = GraphType.Histogram;
	            AddIndicator(vcOpen);
	
		        vcHigh = new IndicatorCommon();
	            vcHigh.Drawing.GroupName = "ValueChart";
	            vcHigh.Drawing.PaneType = PaneType.Secondary;
	            vcHigh.Drawing.IsVisible = true;
	            vcHigh.Drawing.GraphType = GraphType.Histogram;
	            AddIndicator(vcHigh);
	
	            vcLow = new IndicatorCommon();
	            vcLow.Drawing.GroupName = "ValueChart";
	            vcLow.Drawing.PaneType = PaneType.Secondary;
	            vcLow.Drawing.IsVisible = true;
	            vcLow.Drawing.GraphType = GraphType.Histogram;
	            AddIndicator(vcLow);
	
		        vcClose = new IndicatorCommon();
	            vcClose.Drawing.GroupName = "ValueChart";
	            vcClose.Drawing.PaneType = PaneType.Secondary;
	            vcClose.Drawing.IsVisible = true;
	            vcClose.Drawing.GraphType = GraphType.Histogram;
	            AddIndicator(vcClose);
	        }
	               
	        public override bool OnIntervalClose()
	        {
	            vcOpen[0] = Bars.Open[0];
	            vcHigh[0] = Bars.High[0];
	            vcLow[0] = Bars.Low[0];
	            vcClose[0] = Bars.Close[0];
	           
	            return true;
	        }
	       
	} 
}
