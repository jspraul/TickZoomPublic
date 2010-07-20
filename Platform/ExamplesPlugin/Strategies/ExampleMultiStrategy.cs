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
	public class ExampleMultiStrategy : Portfolio
	{
		IndicatorCommon equity;
		
		public ExampleMultiStrategy() {
		}
		
		public override void OnInitialize()
		{
			Performance.GraphTrades = true;
			
			equity = new IndicatorCommon();
			equity.Drawing.IsVisible = true;
			equity.Drawing.PaneType = PaneType.Secondary;
			equity.Drawing.GraphType = GraphType.FilledLine;
			equity.Drawing.Color = Color.Green;
			equity.Drawing.GroupName = "SimpleEquity";
			equity.Name = "SimpleEquity";
			AddIndicator(equity);
			
		}
		
		public override bool OnIntervalClose()
		{
			// Example log message.
			//Log.WriteLine( "close: " + Ticks[0] + " " + Minutes.Close[0] + " " + Minutes.Time[0]);
			
			if( !Position.IsLong && Bars.Close[0] > Bars.High[1]) {
				Strategies[0].Orders.Enter.ActiveNow.BuyMarket();
				if( Strategies[1].Position.HasPosition) {
					Strategies[1].Orders.Exit.ActiveNow.GoFlat();
				}
			}
			if( !Position.IsShort && Bars.Close[0] < Bars.Low[1]) {
				Strategies[1].Orders.Enter.ActiveNow.SellMarket();
				if( Strategies[0].Position.HasPosition) {
					Strategies[0].Orders.Exit.ActiveNow.GoFlat();
				}
			}
			equity[0] = Performance.Equity.CurrentEquity;
			return true;
		}
	}
}