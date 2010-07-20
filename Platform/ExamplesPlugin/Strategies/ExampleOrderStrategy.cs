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
using System.ComponentModel;
using System.Drawing;

using TickZoom.Api;
using TickZoom.Common;

#endregion

namespace TickZoom.Examples
{
	public class ExampleOrderStrategy : Strategy
	{
		double multiplier = 1.0D;
		double minimumTick;
		
		public ExampleOrderStrategy() {
			Performance.GraphTrades = true;
			Performance.Equity.GraphEquity = true;
			ExitStrategy.ControlStrategy = false;
		}
		
		public override void OnInitialize()
		{
			minimumTick = multiplier * Data.SymbolInfo.MinimumTick;
			ExitStrategy.BreakEven = 30 * minimumTick;
			ExitStrategy.StopLoss = 45 * minimumTick;
		}
		
		public override bool OnIntervalClose()
		{
			// Example log message.
			if( IsTrace) Log.Trace( "close: " + Ticks[0] + " " + Bars.Close[0] + " " + Bars.Time[0]);
			
			double close = Bars.Close[0];
			if( Bars.Close[0] > Bars.Open[0]) {
				if( Position.IsFlat) {
					Orders.Enter.NextBar.BuyStop(Bars.Close[0] + 10 * minimumTick);
					Orders.Exit.NextBar.SellStop(Bars.Close[0] - 10 * minimumTick);
				}
				if( Position.IsShort) {
					Orders.Exit.NextBar.BuyLimit(Bars.Close[0] - 3 * minimumTick);
				}
			}
			if( Position.IsLong) {
				Orders.Exit.NextBar.SellStop(Bars.Close[0] - 10 * minimumTick);
			}
			if( Bars.Close[0] < Bars.Open[0]) {
				if( Position.IsFlat) {
					Orders.Enter.NextBar.SellLimit(Bars.Close[0] + 30 * minimumTick);
					ExitStrategy.StopLoss = 45 * minimumTick;
				}
			}
			if( Bars.Close[0] < Bars.Open[0] && Bars.Open[0] < Bars.Close[1]) {
				if( Position.IsFlat) {
					Orders.Enter.NextBar.SellMarket();
					ExitStrategy.StopLoss = 15 * minimumTick;
				}
			}
			return true;
		}
		
		public double Multiplier {
			get { return multiplier; }
			set { multiplier = value; }
		}
	}
}