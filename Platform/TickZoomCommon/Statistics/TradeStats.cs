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
	public class TradeStats : BaseStats
	{
		BaseStats winners;
		BaseStats losers;
		int lossBoundary = 0;
		TransactionPairs trades;
		
		public TradeStats( TransactionPairs trades) : this(6000,trades) {
			this.trades = trades;
		}
		
		public TradeStats( double startingEquity, TransactionPairs trades) : base(startingEquity,trades)
		{
			this.trades = trades;
			if( trades != null && trades.Count > 0) {
				calculate();
			}
		}
		
		private void calculate() {
			calcWinners();
			calcLosers();
		}
		
		private void calcWinners() {
			TransactionPairsBinary other = new TransactionPairsBinary(trades.TradeData);
			for(int i=0; i<Trades.Count; i++) {
				if( Trades.CalcProfitLoss(i) > lossBoundary) {
					other.Add(Trades.GetBinary(i));
				}
			}
			if( other.Count > 0) {
				winners = new BaseStats(new TransactionPairs(null,Trades.ProfitLossCalculation,other));
			}
		}
		public BaseStats Winners {
			get { return winners; }
		}
		
		private void calcLosers() {
			TransactionPairsBinary other = new TransactionPairsBinary(trades.TradeData);
			for(int i=0; i<Trades.Count; i++) {
				if( Trades.CalcProfitLoss(i) <= lossBoundary) {
					other.Add(Trades.GetBinary(i));
				}
			}
			losers = new BaseStats(new TransactionPairs(null,Trades.ProfitLossCalculation,other));
		}
		public BaseStats Losers {
			get { return losers; }
		}
		
		public double WinRate{
			get {
				if( winners == null ) {
						return 0;
				}
				return (double) winners.Count / Trades.Count;
			}
		}
		
		public double Expectancy {
			get { return (WinRate * winners.Average) +
					((1-WinRate) * losers.Average); }
		}
		
		public double ProfitFactor {
			get { if( losers == null) {
					if( winners == null || winners.Count == 0) {
						return 0;
					} else {
						return 1;
					}
				}
				if( winners == null ) { return 0; }
				return (double) winners.ProfitLoss / -losers.ProfitLoss; }
		}
		
		public int LossBoundary {
			get { return lossBoundary; }
			set { lossBoundary = value; }
		}
		
		public override string ToString() {
			base.ToString();
			winners.Name = Name + " Winners";
			losers.Name = Name + " Losers";
			return base.ToString() + 
				winners.Count.ToString("N0") + " winners. " +
					losers.Count.ToString("N0") + " losers. " +
					WinRate.ToString("P2") + " win ratio. " +
					ProfitFactor.ToString("N2") + " profit factor.\n" +
				Expectancy.ToString("N2") + " expectancy.\n" +
				winners.ToString() +
				losers.ToString();
		}
	}
}
