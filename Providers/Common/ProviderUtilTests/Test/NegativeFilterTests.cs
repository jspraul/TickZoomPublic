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
using System.Diagnostics;
using System.IO;
using System.Threading;

using NUnit.Framework;
using TickZoom.Api;

namespace TickZoom.Test
{

	public abstract class NegativeFilterTests : BaseProviderTests {
		
		[Test]
		[ExpectedException( typeof(Exception), ExpectedMessage="was greater than MaxPositionSize of ", MatchType=MessageMatch.Contains)]
		public void TestFIXPositionFilterBuy() {
	  		int expectedPosition = 0;
	  		int sizeIncrease = 2;
	  		int secondsDelay = 3;
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);				
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			long count = verify.Verify(2,assertTick,symbol,secondsDelay);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			int strategyId = 1;
	  			int actualPosition = 0;
	  			while( true) {
					ClearOrders(0);
					CreateEntry(OrderType.BuyMarket,0.0,(int)sizeIncrease,strategyId++);
		  			provider.SendEvent(verify,symbol,(int)EventType.PositionChange,new PositionChangeDetail(symbol,expectedPosition,orders));
		  			expectedPosition += sizeIncrease;
		  			actualPosition += verify.VerifyPosition(sizeIncrease,symbol,secondsDelay);
		  			Assert.AreEqual(expectedPosition, actualPosition, "Increasing position.");
	  			}
			}
		}		
		
#if !OTHERS
		
		[Test]
		[ExpectedException( typeof(Exception), ExpectedMessage="was greater than MaxPositionSize of ", MatchType=MessageMatch.Contains)]
		public void TestFIXPositionFilterSell() {
	  		var expectedPosition = 0;
	  		var sizeIncrease = 2;
	  		int secondsDelay = 3;
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);				
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			long count = verify.Verify(2,assertTick,symbol,25);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			int strategyId = 1;
	  			var actualPosition = 0;
	  			while( true) {
					ClearOrders(0);
					CreateEntry(OrderType.SellMarket,0.0,(int)sizeIncrease, strategyId++);
		  			provider.SendEvent(verify,symbol,(int)EventType.PositionChange,new PositionChangeDetail(symbol,expectedPosition,orders));
		  			expectedPosition-=sizeIncrease;
		  			actualPosition += verify.VerifyPosition(sizeIncrease,symbol,secondsDelay);
		  			Assert.AreEqual(expectedPosition, actualPosition, "Increasing position.");
	  			}
			}
		}		
		
		[Test]
		[ExpectedException( typeof(Exception), ExpectedMessage="was greater than MaxOrderSize of ", MatchType=MessageMatch.Contains)]
		public void TestFIXPretradeOrderFilterBuy() {
			var expectedPosition = 0;
	  		int secondsDelay = 3;
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);				
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			long count = verify.Verify(2,assertTick,symbol,25);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			expectedPosition = 10;
	  			CreateLogicalEntry(OrderType.BuyMarket,0.0,(int)expectedPosition);
	  			provider.SendEvent(verify,symbol,(int)EventType.PositionChange,new PositionChangeDetail(symbol,0,orders));
	  			var position = verify.VerifyPosition(expectedPosition,symbol,secondsDelay);
	  			Assert.AreEqual(expectedPosition, position, "Increasing position.");
	  			Thread.Sleep(2000);
			}
		}		
	
		[Test]
		[ExpectedException( typeof(Exception), ExpectedMessage="was greater than MaxOrderSize of ", MatchType=MessageMatch.Contains)]
		public void TestFIXPretradeOrderFilterSell() {
			var expectedPosition = 0;
	  		int secondsDelay = 3;
			using( VerifyFeed verify = Factory.Utility.VerifyFeed())
			using( Provider provider = ProviderFactory()) {
				provider.SendEvent(verify,null,(int)EventType.Connect,null);
				provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
				VerifyConnected(verify);				
				ClearOrders(0);
				ClearPosition(provider,verify,secondsDelay);
	  			long count = verify.Verify(2,assertTick,symbol,25);
	  			Assert.GreaterOrEqual(count,2,"tick count");
	  			expectedPosition = -10;
	  			CreateLogicalEntry(OrderType.SellMarket,0.0,(int)Math.Abs(expectedPosition));
	  			provider.SendEvent(verify,symbol,(int)EventType.PositionChange,new PositionChangeDetail(symbol,0,orders));
	  			var position = verify.VerifyPosition(expectedPosition,symbol,secondsDelay);
	  			Assert.AreEqual(expectedPosition, position, "Increasing position.");
	  			Thread.Sleep(2000);
			}
		}		
#endif
	}
}
