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
using System.Diagnostics;
using System.IO;
using System.Threading;

using NUnit.Framework;
using TickZoom.Api;

namespace TickZoom.Test
{
	public abstract class ProviderTests : BaseProviderTests
	{
		
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TimeAndSales));
		private static readonly bool debug = log.IsDebugEnabled;		
		private bool isTestSeperate = true;	
		
#if CERTIFICATION
		[Test]
		public void ZAutoReconnection() {
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				if(debug) log.Debug("===ZAutoReconnection===");
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);
				if(debug) log.Debug("===ClearOrders===");
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			TickIO lastTick = verify.LastTick;
	  			double bid = lastTick.IsTrade ? lastTick.Price : lastTick.Bid;
	  			double ask = lastTick.IsTrade ? lastTick.Price : lastTick.Ask;
	  			Assert.GreaterOrEqual(bid,0,"Bid");
	  			Assert.GreaterOrEqual(ask,0,"Ask");
				if(debug) log.Debug("===Create a Buy and Sell Limit ===");
				CreateLogicalEntry(OrderType.BuyLimit,bid-280*symbol.MinimumTick,1);
				CreateLogicalEntry(OrderType.SellLimit,ask+340*symbol.MinimumTick,1);
				SendOrders(provider,verify,secondsDelay);
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
			}
		}		
#endif
			
		[Test]
		public void TestSpecificLogicalOrder() {
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				var secondsDelay = 3;
				VerifyConnected(verify);
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
				
				var expectedTicks = 2;
//	  			var count = verify.Verify(expectedTicks,assertTick,symbol,secondsDelay);
//	  			Assert.GreaterOrEqual(count,expectedTicks,"tick count");
	  			
				CreateLogicalEntry(OrderType.BuyLimit,503.72,2);
				
	  			var count = verify.Verify(expectedTicks,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,expectedTicks,"tick count");

	  			SendOrders(provider,verify,secondsDelay);
				
	  			count = verify.Verify(expectedTicks,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,expectedTicks,"tick count");
			}
		}

#if !OTHERS

		[Test]
		public void TestLogicalLimitOrders() {
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);
				int secondsDelay = 5;
				
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			TickIO lastTick = verify.LastTick;
	  			double bid = lastTick.IsTrade ? lastTick.Price : lastTick.Bid;
	  			double ask = lastTick.IsTrade ? lastTick.Price : lastTick.Ask;
	  			
				LogicalOrder enterBuyLimit = CreateLogicalEntry(OrderType.BuyLimit,bid-280*symbol.MinimumTick,2);
				LogicalOrder enterSellLimit = CreateLogicalEntry(OrderType.SellLimit,ask+340*symbol.MinimumTick,2);
				LogicalOrder exitSellLimit = CreateLogicalExit(OrderType.SellLimit,ask+380*symbol.MinimumTick);
				CreateLogicalExit(OrderType.SellLimit,ask+400*symbol.MinimumTick);
				CreateLogicalExit(OrderType.BuyLimit,bid-150*symbol.MinimumTick);
				LogicalOrder exitBuyStop = CreateLogicalExit(OrderType.BuyStop,ask+540*symbol.MinimumTick);
				SendOrders(provider,verify,secondsDelay);
	  			var count = verify.Verify(2,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,2,"tick count");

				ClearOrders(0);
				enterBuyLimit.Price = bid-260*symbol.MinimumTick;
				enterSellLimit.Price = ask+280*symbol.MinimumTick;
				orders.AddLast(enterBuyLimit);
				orders.AddLast(enterSellLimit);
				orders.AddLast(exitSellLimit);
				SendOrders(provider,verify,secondsDelay);
	  			count = verify.Verify(2,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			count = verify.Wait(symbol,1,secondsDelay);
			}
		}
		
		[Test]
		public void DemoReConnectionTest() {
			int secondsDelay = 8;
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = CreateProvider(true)) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				if(debug) log.Debug("===VerifyState===");
				VerifyConnected(verify);
		  		long count = verify.Verify(2,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,2,"tick count");
		  		provider.SendEvent(verify,null,(int)EventType.Disconnect,null);	
		  		provider.SendEvent(verify,null,(int)EventType.Terminate,null);		
			}
//			Thread.Sleep(2000);
			log.Info("Starting to reconnect---------\n");
			
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = CreateProvider(true)) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
	  			provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				if(debug) log.Debug("===VerifyState===");
				VerifyConnected(verify);
	  			long count = verify.Verify(2,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,2,"tick count");
		  		provider.SendEvent(verify,null,(int)EventType.Disconnect,null);	
		  		provider.SendEvent(verify,null,(int)EventType.Terminate,null);		
			}
		}

		[Test]		
		public void DemoConnectionTest() {
			using( var verify = Factory.Utility.VerifyFeed())
			using( var provider = CreateProvider(true)) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				if(debug) log.Debug("===DemoConnectionTest===");
				if(debug) log.Debug("===StartSymbol===");
				var secondsDelay = 3;
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);
				if(debug) log.Debug("===VerifyState===");
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
				if(debug) log.Debug("===VerifyFeed===");
		  		provider.SendEvent(verify,null,(int)EventType.Disconnect,null);	
		  		provider.SendEvent(verify,null,(int)EventType.Terminate,null);		
			}
		}
		
		[Test]	
		public void DemoStopSymbolTest() {
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = CreateProvider(true)) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				log.Info("===DemoStopSymbolTest===");
				if(debug) log.Debug("===StartSymbol===");
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				if(debug) log.Debug("===VerifyState===");
				VerifyConnected(verify);
				if(debug) log.Debug("===VerifyFeed===");
		  		long count = verify.Verify(2,assertTick,symbol,5);
		  		Assert.GreaterOrEqual(count,2,"tick count");
				if(debug) log.Debug("===StopSymbol===");
		  		provider.SendEvent(verify,symbol,(int)EventType.StopSymbol,null);
		  		
		  		// Wait for it to switch out of real time or historical mode.
		  		var expectedBrokerState = BrokerState.Disconnected;
		  		var expectedSymbolState = ReceiverState.Ready;
		  		var actualState = verify.VerifyState(expectedBrokerState, expectedSymbolState,symbol,25000);
		  		Assert.IsTrue(actualState,"after receiving a StopSymbol event, if your provider plugin was sending ticks then it must return either respond with an EndHistorical or EndRealTime event. If it has already sent one of those prior to the StopSymbol, then no reponse is required.");
		  		
		  		// Clean out and ignore any extra ticks.
		  		count = verify.Verify(1000,assertTick,symbol,5);
		  		Assert.Less(count,1000,"your provider plugin must not send any more ticks after receiving a StopSymbol event.");
		  		
		  		// Make sure we don't get any more ticks.
		  		count = verify.Verify(0,assertTick,symbol,5);
		  		Assert.AreEqual(0,count,"your provider plugin must not send any more ticks after receiving a StopSymbol event.");
		  		
		  		provider.SendEvent(verify,null,(int)EventType.Disconnect,null);	
		  		provider.SendEvent(verify,null,(int)EventType.Terminate,null);		
			}
		}
	
		[Test]
		public void TestMarketOrder() {
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				int secondsDelay = 5;
				if(debug) log.Debug("===TestMarketOrder===");
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			double desiredPosition = 2 * LotSize;
	  			log.Notice("Sending 1");
	  			CreateEntry(provider,verify,OrderType.BuyMarket,desiredPosition,0);
	  			double actualPosition = verify.VerifyPosition(desiredPosition,symbol,secondsDelay);
	  			Assert.AreEqual(desiredPosition,actualPosition,"position");
	
	  			desiredPosition = 0;
	  			log.Warn("Sending 2");
	  			CreateExit(provider,verify,OrderType.SellMarket,desiredPosition,actualPosition);
	  			actualPosition = verify.VerifyPosition(desiredPosition,symbol,secondsDelay);
	  			Assert.AreEqual(desiredPosition,actualPosition,"position");
	
	  			desiredPosition = 2 * LotSize;
	  			log.Warn("Sending 3");
	  			CreateEntry(provider,verify,OrderType.BuyMarket,desiredPosition,actualPosition);
	  			actualPosition = verify.VerifyPosition(desiredPosition,symbol,secondsDelay);
	  			Assert.AreEqual(desiredPosition,actualPosition,"position");
	
	  			desiredPosition = 2 * LotSize;
	  			log.Warn("Sending 4");
	  			CreateEntry(provider,verify,OrderType.BuyMarket,desiredPosition,actualPosition);
	  			actualPosition = verify.VerifyPosition(desiredPosition,symbol,secondsDelay);
	  			Assert.AreEqual(desiredPosition,actualPosition,"position");
	  			
	  			desiredPosition = 0;
	  			log.Warn("Sending 5");
	  			CreateExit(provider,verify,OrderType.SellMarket,desiredPosition,actualPosition);
	  			actualPosition = verify.VerifyPosition(desiredPosition,symbol,secondsDelay);
	  			Assert.AreEqual(desiredPosition,actualPosition,"position");
			}
		}		
		
		[Test]
		public void TestLogicalStopOrders() {
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);
				var secondsDelay = 5;
				
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
//				var expectedTicks = 1;
//	  			var count = verify.Wait(symbol,expectedTicks,secondsDelay);
//	  			Assert.GreaterOrEqual(count,expectedTicks,"at least one tick");
	  			TickIO lastTick = verify.LastTick;
	  			double bid = lastTick.IsTrade ? lastTick.Price : lastTick.Bid;
	  			double ask = lastTick.IsTrade ? lastTick.Price : lastTick.Ask;
	  			
				LogicalOrder enterBuyStop = CreateLogicalEntry(OrderType.BuyStop,bid+420*symbol.MinimumTick,2);
				LogicalOrder enterSellStop = CreateLogicalEntry(OrderType.SellStop,bid-400*symbol.MinimumTick,2);
				CreateLogicalExit(OrderType.SellStop,bid-180*symbol.MinimumTick);
				LogicalOrder exitBuyStop = CreateLogicalExit(OrderType.BuyStop,ask+540*symbol.MinimumTick);
				SendOrders(provider,verify,secondsDelay);
	  			var count = verify.Verify(2,assertTick,symbol,5);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			
				ClearOrders(0);
				enterSellStop.Price = bid-360*symbol.MinimumTick;
				enterBuyStop.Price = ask+380*symbol.MinimumTick;
				orders.AddLast(enterBuyStop);
				orders.AddLast(enterSellStop);
				orders.AddLast(exitBuyStop);
				SendOrders(provider,verify,secondsDelay);
	  			count = verify.Verify(2,assertTick,symbol,5);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
			}
		}

		[Test]
		public void TestSeperateProcess() {
			if( !IsTestSeperate) return;
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = CreateProvider(false)) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				if(debug) log.Debug("===VerifyState===");
				VerifyConnected(verify);
				if(debug) log.Debug("===VerifyFeed===");
	  			long count = verify.Verify(2,assertTick,symbol,25);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			Process[] processes = Process.GetProcessesByName(AssemblyName);
	  			Assert.AreEqual(1,processes.Length,"Number of provider service processes.");
	  			
		  		provider.SendEvent(verify,null,(int)EventType.Disconnect,null);	
		  		provider.SendEvent(verify,null,(int)EventType.Terminate,null);		
			}
		}
		
		[Test]
		public virtual void TestPositionSyncAndStopExits() {
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);
				int secondsDelay = 3;
				
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			TickIO lastTick = verify.LastTick;
	  			double bid = lastTick.IsTrade ? lastTick.Price : lastTick.Bid;
	  			double ask = lastTick.IsTrade ? lastTick.Price : lastTick.Ask;
	  			
				CreateLogicalExit(OrderType.SellStop,bid-180*symbol.MinimumTick);
				LogicalOrder exitBuyStop = CreateLogicalExit(OrderType.BuyStop,ask+540*symbol.MinimumTick);
				SendOrders( provider, verify, secondsDelay);
	  			var count = verify.Verify(2,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
				var expectedTicks = 1;
	  			count = verify.Wait(symbol,expectedTicks,secondsDelay);
			}
		}
		
#endif		
		public static Log Log {
			get { return log; }
		}
		
		public bool IsTestSeperate {
			get { return isTestSeperate; }
			set { isTestSeperate = value; }
		}
	}
}
