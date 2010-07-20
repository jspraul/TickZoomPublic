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
//
//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using TickProcess;
//using TickProcess;
//
//namespace TickZoom.TradingFramework
//{
//	[TestFixture]
//	public class MoneyManagerRandomStrategy
//	{
//		MoneyManagerSupportInner moneyManager;
//		DataStore data = DataStore.DeserializeNow("TestData","USD_JPY");
//		[Test]
//		public void Constructor()
//		{
//			moneyManager = new MoneyManagerSupportInner();
//			Assert.IsNotNull(moneyManager,"MoneyManagerSupport constructor");
//		}
//		[Test]
//		public void InitializeTick() 
//		{
//			Constructor();
//			RandomStrategy strategy = new RandomStrategy();
//			moneyManager.Strategy = strategy;
//			moneyManager.InitializeTick(data);
//			Assert.AreSame(strategy,moneyManager.Strategy,"ExitStrategy property");
//		}
//
//		[Test]
//		[ExpectedException(typeof(ApplicationException))]
//		public void InitializeTickException() 
//		{
//			Constructor();
//			moneyManager.InitializeTick(data);
//		}
//		
//		public class MoneyManagerSupportInner : MoneyManagerSupport
//		{
//			public List<DateTime> enterTrades = new List<DateTime>();
//			public List<DateTime> exitTrades = new List<DateTime>();
//			protected override void EnterTrade() {
//				enterTrades.Add(ExitStrategy.Strategy.Ticks[0].Time);
//			}
//			protected override void ExitTrade() {
//				exitTrades.Add(ExitStrategy.Strategy.Ticks[0].Time);
//			}
//		}
//		
//		[Test]
////		[ExpectedException(typeof(NullReferenceException))]
//		public void StopTesting()
//		{
//			Constructor();
//			InitializeTick();
//			int i = 0;
//			Tick tick;
//			for( i = 1; i < 100; i++) {
//				tick = new Tick(data,i);
////				TickConsole.WriteLine( i + ": " + tick );
//				moneyManager.AddTick(tick);
//				moneyManager.ProcessTick();
//				Assert.AreEqual(false,moneyManager.enterTradeFlag,"enter trade called");
//				Assert.AreEqual(false,moneyManager.exitTradeFlag,"exit trade called");
//				moneyManager.reset();
//			}
//			moneyManager.Strategy.Strategy.Signal = -1;
//			moneyManager.ProcessTick();
//			Assert.AreEqual(true,moneyManager.enterTradeFlag,"enter trade called");
//			Assert.AreEqual(false,moneyManager.exitTradeFlag,"exit trade called");
//			moneyManager.reset();
//			
//			moneyManager.Strategy.Strategy.Signal = 1;
//			moneyManager.ProcessTick();
//			Assert.AreEqual(true,moneyManager.enterTradeFlag,"enter trade called");
//			Assert.AreEqual(true,moneyManager.exitTradeFlag,"exit trade called");
//			moneyManager.reset();
//			
//			moneyManager.Strategy.Stop = 30;  // 3 pips an 0/10ths
//			for( ; i < 360; i++) {
//				tick = new Tick(data,i);
////				TickConsole.WriteLine( i + ": " + tick );
//				moneyManager.AddTick(tick);
//				moneyManager.ProcessTick();
//				Assert.AreEqual(false,moneyManager.enterTradeFlag,"enter trade called");
//				Assert.AreEqual(false,moneyManager.exitTradeFlag,"exit trade called");
//				moneyManager.reset();
//			}
//			tick = new Tick(data,i);
////			TickConsole.WriteLine( i + ": " + tick );
//			moneyManager.AddTick(tick);
//			moneyManager.ProcessTick();
//			Assert.AreEqual(false,moneyManager.enterTradeFlag,"enter trade called");
//			Assert.AreEqual(true,moneyManager.exitTradeFlag,"exit trade called");
//			moneyManager.reset();
//		}		
//	}
//}
