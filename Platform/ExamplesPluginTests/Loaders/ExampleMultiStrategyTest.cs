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
using TickZoom.Examples;
using TickZoom.Starters;
using TickZoom.Transactions;

namespace Loaders
{
	[TestFixture]
	public class ExampleMultiStrategyTest
	{
		#region SetupTest
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		ExampleMultiStrategy strategy;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
			Starter starter = new HistoricalStarter();
			
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,5,28);
    		starter.DataFolder = "Test\\DataCache";
    		starter.ProjectProperties.Starter.SetSymbols("FullTick");
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
			
			// Run the loader.
			ExampleMultiStrategyLoader loader = new ExampleMultiStrategyLoader();
    		starter.Run(loader);

    		// Get the stategy
    		strategy = loader.TopModel as ExampleMultiStrategy;
		}
		#endregion
		
		[Test]
		public void RoundTurn1() {
			VerifyRoundTurn( 1, "1983-04-18 15:59:00.001", 30.56,
			                 "1983-04-19 15:59:00.001", 30.70);
		}
		
//		[Test]
//		public void RoundTurn2() {
//			VerifyRoundTurn( 2, "1983-04-19 15:59:00.001", 30.725,
//			                 "1983-04-27 15:59:00.001",30.755);
//		}
		
		[Test]
		public void CompareEquity() {
			double expectedTotal = strategy.Strategies[0].Performance.Equity.NetProfit;
			expectedTotal += strategy.Strategies[1].Performance.Equity.NetProfit;
			double actualTotal = strategy.Performance.Equity.NetProfit;
			Assert.AreEqual(expectedTotal.Round(),actualTotal.Round(),"net profit totals");
		}
		
		
		#region Verify Pairs
		public void VerifyRoundTurn(int pairNum,
		                       string expectedEntryTime,
		                     double expectedEntryPrice,
		                      string expectedExitTime,
		                     double expectedExitPrice)
		{
    		TransactionPairs transactionPairs = strategy.Performance.ComboTrades;
    		Assert.Greater(transactionPairs.Count,pairNum);
    		TransactionPair pair = transactionPairs[pairNum];
    		TimeStamp expEntryTime = new TimeStamp(expectedEntryTime);
    		Assert.AreEqual( expEntryTime, pair.EntryTime, "Pair " + pairNum + " Entry");
    		Assert.AreEqual( expectedEntryPrice, pair.EntryPrice, "Pair " + pairNum + " Entry");
    		
    		Assert.AreEqual( new TimeStamp(expectedExitTime), pair.ExitTime, "Pair " + pairNum + " Exit");
    		Assert.AreEqual( expectedExitPrice, pair.ExitPrice, "Pair " + pairNum + " Exit");
    		
    		double direction = pair.Direction;
		}
	}
	#endregion
}
