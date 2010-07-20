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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MBTProvider;
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.MBTrading;
using TickZoom.TickUtil;

namespace TickZoom.Test
{
	[TestFixture]
	public class EquityTimeAndSales
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(EquityTimeAndSales));
		private static readonly bool debug = log.IsDebugEnabled;		
		protected Provider provider;
		protected SymbolInfo symbol;
		protected VerifyFeed verify;
		protected bool inProcessFlag = true;
			
		[TestFixtureSetUp]
		public virtual void Init()
		{
			string appData = Factory.Settings["AppDataFolder"];
			File.Delete( Factory.Log.LogFolder + @"\MBTradingTests.log");
			File.Delete( Factory.Log.LogFolder + @"\MBTradingService.log");
  			symbol = Factory.Symbol.LookupSymbol("CSCO");
		}
		
		[TestFixtureTearDown]
		public void Dispose()
		{
		}
		
		public void CreateProvider() {
			if( inProcessFlag) {
				provider = new MbtInterface();
			} else {
				provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,"MBTradingService.exe");
			}
			verify = new VerifyFeed();
			provider.SendEvent(verify,null,(int)EventType.Connect,null);
		}
		
		[SetUp]
		public void Setup() {
			CreateProvider();
		}
		
		[TearDown]
		public void TearDown() {
			provider.SendEvent(verify,null,(int)EventType.Disconnect,null);
			provider.SendEvent(verify,null,(int)EventType.Terminate,null);
		}
		
		[Test]
		public void DemoConnectionTest() {
			if(debug) log.Debug("===DemoConnectionTest===");
			if(debug) log.Debug("===StartSymbol===");
			provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
		}
		
		[Test]		
		public void DemoStopSymbolTest() {
			if(debug) log.Debug("===DemoConnectionTest===");
			if(debug) log.Debug("===StartSymbol===");
			provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(AssertTick,symbol,35);
  			Assert.GreaterOrEqual(count,2,"tick count");
			if(debug) log.Debug("===StopSymbol===");
			provider.SendEvent(verify,symbol,(int)EventType.StopSymbol,null);
  			count = verify.Verify(AssertTick,symbol,10);
  			Assert.AreEqual(0,count,"tick count");
		}

		[Test]
		public void DemoReConnectionTest() {
			provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
  			long count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
  			provider.SendEvent(verify,null,(int)EventType.Disconnect,null);
  			provider.SendEvent(verify,null,(int)EventType.Terminate,null);
  			CreateProvider();
  			provider.SendEvent(verify,symbol,(int)EventType.StartSymbol,new StartSymbolDetail(TimeStamp.MinValue));
  			count = verify.Verify(AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
		}

		public virtual void AssertTick( TickIO tick, TickIO lastTick, ulong symbol) {
        	Assert.Greater(tick.Price,0);
        	Assert.Greater(tick.Size,0);
    		Assert.IsTrue(tick.Time>=lastTick.Time,"tick.Time > lastTick.Time");
    		Assert.AreEqual(symbol,tick.lSymbol);
		}
	}
}
