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
	public class BaseStats 
	{
		private double beginningBalance = 6000;
		
		TransactionPairs trades;
		
		public BaseStats( TransactionPairs trades) : this(6000,trades) {
			
		}
		
		public BaseStats( double startingEquity, TransactionPairs trades)
		{
			this.beginningBalance = startingEquity;
			this.trades = trades;
			if( trades != null && trades.Count > 0) {
				calculate();
			}
		}
		
		string name = "";
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public TransactionPairs Trades {
			get { return trades; }
			set {
				trades = value;
				calculate();
			}
		}
		
		private void calculate() {
			calcProfitLoss();
			calcVariance();
			calcAverageReturn();
			calcAnnualDivisor();
			calcVarianceOfReturn();
			calcVarianceOfDownside();
			calcMaxDownsideRisk();
		}
		
		public long Count {
			get { return trades.Count; }
		}
		
		private double profitLoss;
		private void calcProfitLoss() {
			profitLoss = 0;
			for(int i=0; i<trades.Count; i++) {
				profitLoss += trades.CalcProfitLoss(i);
			}
		}

		public double ProfitLoss {
			get { return profitLoss; }
		}
		
		public double Average {
			get {
				if( trades.Count == 0) {
					return 0;
				} else {
					return (double) ProfitLoss / trades.Count;
				}
			}
		}

		private void calcVariance() {
			double avg = Average;
			double sum = 0;
			for(int i=0; i<trades.Count; i++) {
				sum += Math.Pow( trades.CalcProfitLoss(i) - avg, 2);
			}
			variance = sum / trades.Count;
		}
		
		public double BeginningBalance {
			get { return beginningBalance; }
			set { beginningBalance = value; }
		}
		
		private void calcAverageReturn() {
			double sum = 0;
			double currentBalance = beginningBalance;
			for(int i=0; i<trades.Count; i++) {
				double pnl = trades.CalcProfitLoss(i);
				double returnRate = ((pnl/10)+currentBalance)/currentBalance - 1;
				sum += returnRate;
				currentBalance += pnl/10;
			}
			averageReturn = sum / trades.Count;
		}
		
		double averageReturn = 0;
		public double AverageReturn {
			get {
				return averageReturn;
			}
		}
		
		private double varianceOfReturn;
		public double VarianceOfReturn {
			get { return varianceOfReturn; }
		}
		
		private void calcVarianceOfReturn() {
			double avg = AverageReturn;
			double sum = 0;
			double currentBalance = beginningBalance;
			for(int i=0; i<trades.Count; i++) {
				double pnl = trades.CalcProfitLoss(i);
				double returnRate = (((pnl/10)+currentBalance)/currentBalance)-1;
				sum += Math.Pow( returnRate - avg, 2);
				currentBalance += pnl/10;
			}
			varianceOfReturn = sum / trades.Count;
		}
		
		double varianceOfDownside;
		public double VarianceOfDownside {
			get { return varianceOfDownside; }
		}
		
		private void calcVarianceOfDownside() {
			double avg = RiskFreeRate;
			double sum = 0;
			double currentBalance = beginningBalance;
			for(int i=0; i<trades.Count; i++) {
				double pnl = trades.CalcProfitLoss(i);
				double returnRate = (((pnl/10)+currentBalance)/currentBalance)-1;
				sum += Math.Pow( returnRate - avg, 2);
				currentBalance += pnl/10;
			}
			varianceOfDownside = sum / trades.Count;
		}
		
		double maxDownsideRisk;
		public double MaxDownsideRisk {
			get { return maxDownsideRisk; }
		}
		
		private void calcMaxDownsideRisk() {
			double avg = RiskFreeRate;
			maxDownsideRisk = 0;
			double currentBalance = beginningBalance;
			if( trades.Count == 0 ) {
				return;
			}
			for(int i=0; i<trades.Count; i++) {
				double pnl = trades.CalcProfitLoss(i);
				double returnRate = (((pnl/10)+currentBalance)/currentBalance)-1;
				maxDownsideRisk = Math.Min( maxDownsideRisk, returnRate);
				currentBalance += pnl/10;
			}
		}
		
		public double SortinoRatio {
			get { return (AnnualReturn - RiskFreeRate) / DownsideRisk; }
		}
		
		private double variance;
		public double Variance {
			get { return variance; }
		}
		
		public double StandardDeviation {
			get { return Math.Sqrt( Variance); }
		}
		
		public double DownsideRisk {
			get { return Math.Sqrt( varianceOfDownside) * Math.Sqrt( trades.Count/annualDivisor); }
		}
		
		public double Volatility {
			get { return Math.Sqrt( varianceOfReturn) * Math.Sqrt( trades.Count/annualDivisor); }
		}
		
		double annualDivisor = 0;
		private void calcAnnualDivisor() {
			Elapsed elapsed = trades[trades.Count-1].ExitTime - trades[0].EntryTime;
			annualDivisor = elapsed.TotalDays/365.0D;
		}
		
		public double AnnualReturn {
			get { return Math.Pow(((ProfitLoss/10)+beginningBalance)/beginningBalance,1/annualDivisor) - 1; }
		}
		
		double riskFreeRate = 0.01;
		public double RiskFreeRate {
			get { return riskFreeRate; }
			set { riskFreeRate = value; }
		}
		
		public double SharpeRatio {
			get { double excessReturn = AnnualReturn - RiskFreeRate;  // 1% is the Risk Free Rate of Return.
				  return excessReturn / Volatility; }
		}
		
		public double ModifiedSharpe {
			get { return Average / StandardDeviation; }
		}
		
		public override string ToString() {
			return "\nStrategy Results for " + Name + "\n\n" +
			Count + " trades. " + ProfitLoss + " total profit.\n" +
			Average.ToString("N2") + " average. " +
				StandardDeviation.ToString("N2") + " std dev. " +
				ModifiedSharpe.ToString("N2") + " mod Sharpe.\n";
		}
	}
}
