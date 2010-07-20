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
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Transactions;

namespace TickZoom.Statistics
{
	public class StrategyStats
	{
		TradeStats comboTrades;
		
		public StrategyStats(TransactionPairs combo)
		{
			this.comboTrades = new TradeStats( combo);
		}
		
		[Obsolete("Please using ComboTrades.")]
		public TradeStats Combo {
			get { return comboTrades; }
		}
		
		public TradeStats ComboTrades {
			get { return comboTrades; }
		}
		
		[Obsolete("Please use ComboTrades instead.",true)]
		public TradeStats TransactionPairs {
			get { return null; }
		}

#region Obsolete
		[Obsolete("Use the Performance.Equity.CalculateStatistics() to get these statistics.",true)]
		public TradeStats Daily {
			get { throw new NotImplementedException(); }
		}
		
		[Obsolete("Use the Performance.Equity.CalculateStatistics() to get these statistics.",true)]
		public TradeStats Weekly {
			get { throw new NotImplementedException(); }
		}
		
		[Obsolete("Use the Performance.Equity.CalculateStatistics() to get these statistics.",true)]
		public TradeStats Monthly {
			get { throw new NotImplementedException(); }
		}
		
		[Obsolete("Use the Performance.Equity.CalculateStatistics() to get these statistics.",true)]
		public TradeStats Yearly {
			get { throw new NotImplementedException(); }
		}
#endregion

	}

}
