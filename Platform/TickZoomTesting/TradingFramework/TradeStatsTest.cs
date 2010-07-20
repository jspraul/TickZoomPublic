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
using TickZoom.Common;
using TickZoom.Statistics;
using TickZoom.Transactions;

#if TESTING

namespace TickZoom.TradingFramework
{
	[TestFixture]
	public class TradeStatsTest : BaseStatsTest
	{
		protected TradeStats tradeStats;
		
		public override void Constructor(TransactionPairs trades)
		{
			tradeStats = new TradeStats(trades);
			tradeStats.Name = "Test Strategy";
			baseStats = tradeStats;
		}
		
		[Test]
		public void Winners() {
			Assert.IsNotNull(tradeStats.Winners,"Winners");
			Assert.AreEqual(43,tradeStats.Winners.Count,"Winners");
		}
		
		[Test]
		public void Losers() {
			Assert.IsNotNull(tradeStats.Losers,"Losers");
			Assert.AreEqual(143,tradeStats.Losers.Count,"Losers");
		}
		
		[Test]
		public void WinRate() {
			Assert.AreEqual(0.2312,Math.Round(tradeStats.WinRate,4),"WinRate");
		}
		
		[Test]
		public void LossBoundary() {
			tradeStats.LossBoundary = 10;
			Assert.AreEqual(10,tradeStats.LossBoundary,"LossBoundary");
		}		
		
		[Test]
		public void Expectancy() {
			Assert.AreEqual(-0.0255,Math.Round(tradeStats.Expectancy,4),"Expectancy");
		}		
		
		public override void ToStringTest() {
//			TickConsole.WriteLine(tradeStats.ToString());
		}
		[Test]
		public void ProfitFactor() {
			Assert.AreEqual(0.37,Math.Round(tradeStats.ProfitFactor,2),"Expectancy");
		}
	}
}
#endif