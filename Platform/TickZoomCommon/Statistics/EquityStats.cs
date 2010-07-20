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
	public class EquityStats 
	{
		TradeStats daily;
		TradeStats weekly;
		TradeStats monthly;
		TradeStats yearly;
		
		public EquityStats(TransactionPairs daily, TransactionPairs weekly, TransactionPairs monthly, TransactionPairs yearly) 
			: this( 6000, daily, weekly, monthly, yearly) {
		}
		
		public EquityStats(double startingEquity, TransactionPairs daily, TransactionPairs weekly, TransactionPairs monthly, TransactionPairs yearly)
		{
			this.daily = new TradeStats( startingEquity, daily);
			this.weekly = new TradeStats( startingEquity, weekly);
			this.monthly = new TradeStats( startingEquity, monthly);
			this.yearly = new TradeStats( startingEquity, yearly);
		}
		
		public TradeStats Daily {
			get { return daily; }
		}
		
		public TradeStats Weekly {
			get { return weekly; }
		}
		
		public TradeStats Monthly {
			get { return monthly; }
		}
		
		public TradeStats Yearly {
			get { return yearly; }
		}
	}
}
