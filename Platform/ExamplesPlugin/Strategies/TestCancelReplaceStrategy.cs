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

	public class TestCancelReplaceStrategy : Strategy
	{
		double multiplier = 1.0D;
		double minimumTick;
		int tradeSize;
		
		public TestCancelReplaceStrategy() {
			Performance.GraphTrades = true;
			Performance.Equity.GraphEquity = true;
			ExitStrategy.ControlStrategy = false;
		}
		
		public override void OnInitialize()
		{
			tradeSize = Data.SymbolInfo.Level2LotSize;
			minimumTick = multiplier * Data.SymbolInfo.MinimumTick;
		}
		
		public override bool OnIntervalClose()
		{
			double close = Bars.Close[0];
			if( Position.IsFlat) {
				Orders.Enter.NextBar.BuyLimit(close - 10 * minimumTick, tradeSize);
				Orders.Exit.ActiveNow.GoFlat();
			}
			return true;
		}
		
		public double Multiplier {
			get { return multiplier; }
			set { multiplier = value; }
		}
	}
}
