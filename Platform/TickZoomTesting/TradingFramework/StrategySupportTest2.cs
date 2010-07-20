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
using TickZoom.Engine;
using TickZoom.TickUtil;

#if OBFUSCATE
#else


namespace TickZoom.Unit.TradingFramework
{
	[TestFixture]
	public class StrategySupportTest2
	{
		private static readonly Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		StrategyCommon strategy;
		TickZoom.TickUtil.TickReader tickReader;
		DataImpl data;
		
		[SetUp]
    	public void Init() {
			SymbolInfo properties = new SymbolPropertiesImpl();
 			data = new DataImpl(properties,10000,1000);
			data.AddInterval( IntervalsInternal.Hour1);
			
    		tickReader = new TickZoom.TickUtil.TickReader();
    		tickReader.Initialize("TestData","USD_JPY");
		}
		
		[TearDown]
    	public void Dispose() {
			TickReader.CloseAll();
    	}
		
		[Test]
		public void Constructor()
		{
			TickWrapper wrapper = new TickWrapper();
			TickBinary tickBinary = new TickBinary();
			tickReader.ReadQueue.Dequeue(ref tickBinary);
			wrapper.SetTick(ref tickBinary);
			data.InitializeTick(wrapper);
			strategy = new StrategyCommon();
			strategy.IntervalDefault = IntervalsInternal.Day1;
			ModelDriverFactory factory = new ModelDriverFactory();
			factory.GetInstance(strategy).EngineInitialize(data);
			Assert.IsNotNull(strategy.Minutes,"Minutes");
			Assert.AreEqual(1,strategy.Minutes.Count,"Checking count");
			tickReader.ReadQueue.Dequeue(ref tickBinary);
			wrapper.SetTick(ref tickBinary);
			Assert.IsNotNull(strategy.Performance);
		}
		
		[Test]
		public void Variables()
		{
			Constructor();
			Assert.AreEqual(1,strategy.Minutes.Count,"Minute Bar Count");
			Assert.AreEqual(1,strategy.Hours.Count,"Hour Bar Count");
			Assert.AreEqual(1,strategy.Days.Count,"Day Bar Count");
			Assert.AreEqual(1,strategy.Sessions.Count,"Session Bar Count");
			Assert.AreEqual(1,strategy.Weeks.Count,"Week Bar Count");
			Assert.AreEqual(1,strategy.Months.Count,"Month Bar Count");
			Assert.AreEqual(1,strategy.Years.Count,"Year Bar Count");
			Assert.AreEqual(1,strategy.Ticks.Count,"Year Bar Count");
		}
		
		[Test]
		public void InitializeTest() 
		{
			StrategyInner inner = new StrategyInner();
			inner.IntervalDefault = IntervalsInternal.Day1;			
			ModelDriverFactory factory = new ModelDriverFactory();
			factory.GetInstance(inner).EngineInitialize(data);
			Assert.IsTrue(inner.initializeCalled,"Initialized called");
		}
		
		[Test]
		public void ProcessTest() 
		{
			StrategyInner inner = new StrategyInner();
			inner.IntervalDefault = IntervalsInternal.Day1;
			inner.OnBeforeInitialize();
			TickImpl tick=new TickImpl();
			TickBinary tickBinary = new TickBinary();
			tickReader.ReadQueue.Dequeue(ref tickBinary);
			tick.init(tickBinary);
			inner.OnProcessTick(tick);
			Assert.IsTrue(inner.processCalled,"Process called");
		}
		
		[Test]
		public void EndTest() 
		{
			StrategyInner inner = new StrategyInner();
			inner.OnEndHistorical();
			Assert.IsTrue(inner.endTestCalled,"EndTest called");
		}
		
		[Test]
		public void SignalTest() 
		{
			TickWrapper wrapper = new TickWrapper();
			TickBinary tickBinary = new TickBinary();
			tickReader.ReadQueue.Dequeue(ref tickBinary);
			wrapper.SetTick(ref tickBinary);
			data.InitializeTick(wrapper);
			StrategyInner inner = new StrategyInner();
			inner.IntervalDefault = IntervalsInternal.Day1;
			ModelDriverFactory factory = new ModelDriverFactory();
			factory.GetInstance(inner).EngineInitialize(data);
			inner.setSignal(1);
			Assert.AreEqual(1,inner.Position.Signal,"Signal");
			inner.setSignal(-1);
			Assert.AreEqual(-1,inner.Position.Signal,"Signal");
			inner.setSignal(0);
			Assert.AreEqual(0,inner.Position.Signal,"Signal");
		}
		[Test]
		public void OtherSignalTest() 
		{
			TickWrapper wrapper = new TickWrapper();
			TickBinary tickBinary = new TickBinary();
			tickReader.ReadQueue.Dequeue(ref tickBinary);
			wrapper.SetTick(ref tickBinary);
			data.InitializeTick(wrapper);
			StrategyInner inner = new StrategyInner();
			inner.Name = "Inner";
			inner.IntervalDefault = IntervalsInternal.Day1;
			ModelDriverFactory factory = new ModelDriverFactory();
			factory.GetInstance(inner).EngineInitialize(data);
			StrategyInner other = new StrategyInner();
			other.Name = "Other";
			other.IntervalDefault = IntervalsInternal.Day1;
			factory.GetInstance(other).EngineInitialize(data);
			inner.Chain.InsertAfter(other.Chain);
			log.Debug(inner.Chain.ToChainString());
			other.setSignal(1);
			StrategySupportInterface strategy = inner.Chain.Next.Next.Next.Next.Next.Model as StrategySupportInterface;
			Assert.AreEqual(1,strategy.Position.Signal,"Signal");
			other.setSignal(-1);
			strategy = inner.Chain.Next.Next.Next.Next.Next.Model as StrategySupportInterface;
			Assert.AreEqual(-1,strategy.Position.Signal,"Signal");
			other.setSignal(0);
			strategy = inner.Chain.Next.Model as StrategySupportInterface;
			Assert.AreEqual(0,strategy.Position.Signal,"Signal");
		}
		

		[Test]
		public void StrategySetException() {
			StrategyInner inner = new StrategyInner();
			StrategyInner other1 = new StrategyInner();
			StrategyInner other2 = new StrategyInner();
			StrategyInner other3 = new StrategyInner();
			inner.AddDependency(other1);
			inner.AddDependency(other2);
			inner.AddDependency(other3);
			inner.Chain.InsertBefore(other1.Chain);
			Assert.IsNull(other1.Previous);
		}
		
		[Test]
		public void StrategyGetException() {
			StrategyInner inner = new StrategyInner();
			StrategyInner other1 = new StrategyInner();
			StrategyInner other2 = new StrategyInner();
			StrategyInner other3 = new StrategyInner();
			inner.AddDependency(other1);
			inner.AddDependency(other2);
			inner.AddDependency(other3);
			StrategySupportInterface aware = inner.Next;
			Assert.IsNull(aware);
		}
		
		[Test]

		public void StrategyGetEmptyException() {
			StrategyInner inner = new StrategyInner();
			StrategySupportInterface aware = inner.Next;
			Assert.IsNull( aware, "Strategy property");
		}
		public class StrategyInner : StrategyCommon
		{
			public bool initializeCalled = false;
			public bool processCalled = false;
			public bool endTestCalled = false;
			public override void OnInitialize() {
				initializeCalled = true;
			}
			public override bool OnProcessTick(Tick tick) {
				processCalled = true;
				return true;
			}
			public override void OnEndHistorical() {
				endTestCalled = true;
			}
			public void setSignal(int sig) {
				Position.Signal = sig;
			}
		}
	}
}
#endif