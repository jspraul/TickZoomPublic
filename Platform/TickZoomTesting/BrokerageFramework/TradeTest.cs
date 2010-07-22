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
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Statistics;
using TickZoom.Transactions;

namespace TickZoom.BrokerageFramework
{
	[TestFixture]
	public class TradeTest
	{
		TransactionPairBinary pair;
		[Test]
		public void Constructor()
		{
			pair = TransactionPairBinary.Create();
			Assert.IsNotNull(pair,"Trade constructor");
		}
		
		[Test]
		public void Direction()
		{
			Constructor();
			pair.Direction = 1;
			Assert.AreEqual(1,pair.Direction,"Direction");
		}
		
		[Test]
		public void EntryPrice()
		{
			Constructor();
			pair.EntryPrice = 12344;
			Assert.AreEqual(12344,pair.EntryPrice,"EntryPrice");
		}
		
		[Test]
		public void EntryTime()
		{
			Constructor();
			TimeStamp testTime = new TimeStamp(2005,5,2,8,33,34,432);
			pair.EntryTime = testTime;
			Assert.AreEqual(testTime,pair.EntryTime,"EntryTime");
		}
		
		[Test]
		public void ExitPrice()
		{
			Constructor();
			pair.ExitPrice = 125440;
			Assert.AreEqual(125440,pair.ExitPrice,"ExitPrice");
		}
		
		[Test]
		public void ExitTime()
		{
			Constructor();
			TimeStamp testTime = new TimeStamp(2005,5,2,8,43,34,432);
			pair.ExitTime = testTime;
			Assert.AreEqual(testTime,pair.ExitTime,"ExitTime");
		}
		
		[Test]
		[ExpectedException(typeof(ApplicationException))]
		public void ProfitLossException()
		{
			Constructor();
			pair.Direction = 0;
			using( BinaryStore tradeData = Factory.Engine.PageStore("TradeData")) {
				TransactionPairsBinary tradesBinary = new TransactionPairsBinary(tradeData);
				tradesBinary.Add(pair);
				TransactionPairs trades = new TransactionPairs(null,new ProfitLossCallback(),tradesBinary);
				double pnl = trades.CalcProfitLoss(0);
			}
		}
		
		public class TicksTest : Ticks {
			
			
			public int Count {
				get {
					throw new NotImplementedException();
				}
			}
			
			public int BarCount {
				get {
					throw new NotImplementedException();
				}
			}
			
			public Tick this[int position] {
				get {
					throw new NotImplementedException();
				}
			}
		}

		[Test]
		public void ProfitLoss()
		{
			Constructor();
			pair.Direction = 1;
			pair.EntryPrice = 123440;
			pair.ExitPrice = 134500;
			double ProfitLoss = (pair.ExitPrice - pair.EntryPrice) * 1;
			using( BinaryStore tradeData = Factory.Engine.PageStore("TradeData")) {
				TransactionPairsBinary tradesBinary = new TransactionPairsBinary(tradeData);
				tradesBinary.Add(pair);
				TransactionPairs trades = new TransactionPairs(null,new ProfitLossCallback(),tradesBinary);
				Assert.AreEqual(11060,trades.CalcProfitLoss(0),"ProfitLoss");
			}
		}
		
		[Test]
		public void ToStringTest()
		{
			Constructor();
			pair.Direction = 1;
			pair.EntryPrice = 134230;
			pair.EntryTime = new TimeStamp(2005,5,2,8,33,34,432);
			pair.ExitPrice = 145230;
			pair.ExitTime = new TimeStamp(2005,5,2,8,33,34,321);
			string expected = "1,0,134230,2005-05-02 08:33:34.432,0,145230,2005-05-02 08:33:34.321,145230,0";
			string actual = pair.ToString();
			Assert.AreEqual(expected,actual,"ToString");
			
		}

		[Test]
		public void ToStringHeader()
		{
			Constructor();
			string expected = "Direction,EntryBar,EntryPrice,EntryTime,ExitPrice,ExitBar,ExitTime,MaxPrice,MinPrice,ProfitLoss";
			string actual = pair.ToStringHeader();
			Assert.AreEqual(expected,actual,"ProfitLoss");
		}
	}
}
