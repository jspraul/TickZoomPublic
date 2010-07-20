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

#region Namespaces
using System;
using System.Drawing;
using TickZoom.Api;
using TickZoom.Common;
#endregion

namespace TickZoom.Examples
{
	public class ExampleSMAStrategy : Strategy
	{
		IndicatorCommon equity;
		SMA sma;
		int length = 20;
		int factor = 5;
		TimeStamp timeStamp = new TimeStamp(2009,3,25);
		
		public ExampleSMAStrategy() {
		}
		
		public override void OnInitialize()
		{
			factor = 10;
			Performance.GraphTrades = true;
			
			sma = new SMA(Bars.Close,length);
			sma.Drawing.IsVisible = true;
			AddIndicator( sma);
			
			equity = new IndicatorCommon();
			equity.Drawing.IsVisible = true;
			equity.Drawing.PaneType = PaneType.Secondary;
			equity.Drawing.GraphType = GraphType.FilledLine;
			equity.Drawing.Color = Color.Green;
			equity.Drawing.GroupName = "SMAEquity";
			equity.Name = "SMAEquity";
			AddIndicator(equity);
			
		}
		
		public override bool OnIntervalClose()
		{
			if( Bars.Close[0] > sma[0]) {
				Orders.Enter.ActiveNow.BuyMarket();
			}
			if( Bars.Close[0] < sma[0]) {
				Orders.Enter.ActiveNow.SellMarket();
			}
			equity[0] = Performance.Equity.CurrentEquity;
			return true;
		}
		
		public int Length {
			get { return length; }
			set { length = value; }
		}
		
		public int Factor {
			get { return factor; }
			set { factor = value; }
		}
		
		public TimeStamp TimeStamp {
			get { return timeStamp; }
			set { timeStamp = value; }
		}
	}
}