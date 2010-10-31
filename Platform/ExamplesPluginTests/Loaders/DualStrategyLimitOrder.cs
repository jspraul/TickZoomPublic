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
using NUnit.Framework;
using System.Threading;
using TickZoom;
using TickZoom.Api;
using TickZoom.Examples;
using TickZoom.Common;

namespace Loaders
{
	[TestFixture]
	public class DualStrategyLimitOrder : StrategyTest
	{
		Log log = Factory.SysLog.GetLogger(typeof(DualStrategyLimitOrder));
		Portfolio portfolio;
		Strategy strategy1;
		Strategy strategy2;
		public DualStrategyLimitOrder() {
			Symbols = "USD/JPY,EUR/USD";
			ShowCharts = false;
			StoreKnownGood = false;
			StartTime = new TimeStamp(1800,1,1);
			EndTime = new TimeStamp(2009,06,10);
			IntervalDefault = Intervals.Minute1;
		}
			
		[TestFixtureSetUp]
		public override void RunStrategy() {
			CleanupFiles();
			try {
				Starter starter = SetupStarter(AutoTestMode);
				
				// Run the loader.
				TestDualStrategyLoader loader = new TestDualStrategyLoader();
	    		starter.Run(loader);
	
	    		// Get the stategy
	    		portfolio = loader.TopModel as Portfolio;
	    		strategy1 = portfolio.Strategies[0];
	    		strategy2 = portfolio.Strategies[1];
	    		LoadTrades();
	    		LoadTransactions();
	    		LoadBarData();
				LoadReconciliation();
			} catch( Exception ex) {
				log.Error("Setup error.", ex);
				throw;
			}
		}
		
		[Test]
		public void VerifyCurrentEquity() {
			Assert.AreEqual( 6291.70D,Math.Round(portfolio.Performance.Equity.CurrentEquity,2),"current equity");
		}
		[Test]
		public void VerifyOpenEquity() {
			Assert.AreEqual( -497.00D,portfolio.Performance.Equity.OpenEquity,"open equity");
		}
		[Test]
		public void VerifyClosedEquity() {
			Assert.AreEqual( 6788.70,Math.Round(portfolio.Performance.Equity.ClosedEquity,2),"closed equity");
		}
		[Test]
		public void VerifyStartingEquity() {
			Assert.AreEqual( 10000,portfolio.Performance.Equity.StartingEquity,"starting equity");
		}
		
		[Test]
		public void VerifyStrategy1Trades() {
			VerifyTrades(strategy1);
		}
	
		[Test]
		public void VerifyStrategy2Trades() {
			VerifyTrades(strategy2);
		}
		
		[Test]
		public void VerifyStrategy1TradeCount() {
			VerifyTradeCount(strategy1);
		}
		
		[Test]
		public void VerifyStrategy2TradeCount() {
			VerifyTradeCount(strategy2);
		}
		
		[Test]
		public void VerifyStrategy1Transactions() {
			VerifyTransactions(strategy1);
		}
	
		[Test]
		public void VerifyStrategy2Transactions() {
			VerifyTransactions(strategy2);
		}
		
		[Test]
		public void VerifyStrategy1TransactionCount() {
			VerifyTransactionCount(strategy1);
		}
		
		[Test]
		public void VerifyStrategy2TransactionCount() {
			VerifyTransactionCount(strategy2);
		}
		
		[Test]
		public void VerifyPortfolioBarDataCount() {
			VerifyBarDataCount(portfolio);
		}
		
		[Test]
		public void VerifyPortfolioBarData() {
			VerifyBarData(portfolio);
		}
		
		[Test]
		public void VerifyStrategy1BarData() {
			VerifyBarData(strategy1);
		}
		
		[Test]
		public void VerifyStrategy2BarData() {
			VerifyBarData(strategy2);
		}
		
		[Test]
		public void VerifyStrategy1BarDataCount() {
			VerifyBarDataCount(strategy1);
		}
		
		[Test]
		public void VerifyStrategy2BarDataCount() {
			VerifyBarDataCount(strategy2);
		}
		
		[Test]
		public void CompareBars() {
			CompareChart(strategy1,GetChart(strategy1.SymbolDefault));
		}
	}
	

}
