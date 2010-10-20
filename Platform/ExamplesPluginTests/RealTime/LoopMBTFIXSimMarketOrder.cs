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
using System.Configuration;
using System.IO;

using Loaders;
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Starters;

namespace MockProvider
{
#if LOOPTEST
	[TestFixture] 
	public class LoopMBTFIXSimDualLimitOrder {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(LoopMBTFIXSimDualLimitOrder));
		private bool error = false;
		[Test]
		public void LoopTest() {
			for( int i=0; i<100; i++) {
				error = false;
				log.Warn("---------  LoopTest # " + i + " -----------------------");
				var test = new MBTFIXSimDualLimitOrder();
				Test(test.RunStrategy);
				Test(test.CompareBars);
				Test(test.VerifyStartingEquity);
				Test(test.VerifyClosedEquity);
				Test(test.VerifyCurrentEquity);
				Test(test.VerifyOpenEquity);
				Test(test.VerifyPortfolioBarData);
				Test(test.VerifyPortfolioBarDataCount);
				Test(test.VerifyStrategy1BarData);
				Test(test.VerifyStrategy1BarDataCount);
				Test(test.VerifyStrategy1TradeCount);
				Test(test.VerifyStrategy1Trades);
				Test(test.VerifyStrategy1TransactionCount);
				Test(test.VerifyStrategy1Transactions);
				Test(test.VerifyStrategy2BarData);
				Test(test.VerifyStrategy2BarDataCount);
				Test(test.VerifyStrategy2TradeCount);
				Test(test.VerifyStrategy2Trades);
				Test(test.VerifyStrategy2TransactionCount);
				Test(test.VerifyStrategy2Transactions);
				Test(test.PerformReconciliationTest);
				Test(test.EndStrategy);
				if( error) {
					Assert.Fail("Check log file for failures.");
				}
			}
		}
		private void Test(Action action) {
			try {
				action();
			} catch(Exception ex) {
				error = true;
				log.Error(ex.Message,ex);
			}
		}
	}
#endif
#if LOOPTEST
	[TestFixture] 
	public class LoopMBTFIXSimMarketOrder {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(LoopMBTFIXSimMarketOrder));
		[Test]
		public void LoopTest() {
			for( int i=0; i<100; i++) {
				var error = false;
				log.Warn("---------  LoopTest # " + i + " -----------------------");
				var test = new MBTFIXSimMarketOrder();
				try {
					test.RunStrategy();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.CompareBars();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.VerifyBarData();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.VerifyClosedEquity();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.VerifyCurrentEquity();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.VerifyOpenEquity();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.VerifyStartingEquity();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.VerifyTradeCount();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				try {
					test.VerifyTrades();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
	//				try {
	//					test.PerformReconciliationTest();
	//				} catch(Exception ex) {
	//					error = true;
	//					log.Error(ex.Message,ex);
	//				}
				try {
					test.EndStrategy();
				} catch(Exception ex) {
					error = true;
					log.Error(ex.Message,ex);
				}
				if( error) {
					Assert.Fail("Check log file for failures.");
				}
			}
		}
	}
#endif
}