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
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;
using TickZoom.Starters;
using TickZoom.Transactions;

namespace Loaders
{
	[TestFixture]
	public class ExampleScannerTest
	{
		#region SetupTest
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Strategy strategy;
		ExampleScannerStrategy scanner;
			
		[TestFixtureSetUp]
		public void RunStrategy() {
    		Assert.Ignore();
			Starter starter = new HistoricalStarter();
			
			// Set run properties as in the GUI.
			starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
//    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,5,28);
    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1983,5,1);
    		starter.DataFolder = "Test\\DataCache";
    		starter.ProjectProperties.Starter.SetSymbols("FullTick,Daily4Sim");
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
			
			// Run the loader.
			ExampleScannerLoader loader = new ExampleScannerLoader();
    		starter.Run(loader);

    		// Get the stategy
    		scanner = loader.TopModel as ExampleScannerStrategy;
    		strategy = scanner.Markets[0];
		}
		#endregion
		
		[Test]
		public void RoundTurn0() {
			VerifyRoundTurn( 0, "1983-04-06 15:59:00.001", 29.90,
			                 "1983-04-07 15:59:00.001",29.92);
		}
		
		[Test]
		public void RoundTurn1() {
			VerifyRoundTurn( 1, "1983-04-08 15:59:00.001", 30.65,
			                 "1983-04-11 15:59:00.001", 30.40);
		}
		
//		[Test]
//		public void RoundTurn2() {
//			VerifyRoundTurn( 2, "1983-04-18 15:59:00.001", 30.560,
//			                 "1983-04-19 15:59:00.001",30.700);
//		}
		
		[Test]
		public void CheckSymbols() {
			string symbols = "";
			if( scanner.Markets.Count != 2) {
				bool isFirst = true;
				foreach( var market in scanner.Strategies) {
					if( !isFirst) {
						symbols += ", ";
					}
					isFirst = false;
					symbols += market.SymbolDefault;
				}
			}
			Assert.AreEqual(2,scanner.Strategies.Count,"Symbols: " + symbols);
			Assert.AreEqual(scanner.Strategies[0].SymbolDefault,"FullTick");
			Assert.AreEqual(scanner.Strategies[1].SymbolDefault,"Daily4Sim");
		}
		
		#region Verify Pairs
		public void VerifyRoundTurn(int pairNum,
		                       string expectedEntryTime,
		                     double expectedEntryPrice,
		                      string expectedExitTime,
		                     double expectedExitPrice)
		{
    		TransactionPairs pairs = strategy.Performance.ComboTrades;
    		Assert.Greater(pairs.Count,pairNum);
    		TransactionPair pair = pairs[pairNum];
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
