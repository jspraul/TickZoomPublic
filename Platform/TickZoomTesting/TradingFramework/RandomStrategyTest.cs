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
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Starters;

#if TESTING
namespace TickZoom.TradingFramework
{
	[TestFixture]
	public class RandomStrategyTest 
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		RandomTestInner strategy;
		
    	[SetUp]
    	public virtual void Init() {
			log.Notice("Setup RandomStrategyTest");
    	}
    	
		[Test]
		public void Constructor()
		{
			strategy = new RandomTestInner();
			Assert.IsNotNull(strategy,"RandomStrategy constructor");
		}

		public class RandomTestInner : RandomCommon {
			Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			public List<Tick> signalChanges = new List<Tick>();
			public List<double> signalDirection = new List<double>();
			public double prevSignal = 0;
			public override void OnInitialize()
			{
				Position = new TradingTest(this);
				base.OnInitialize();
				signalChanges = new List<Tick>();
				List<int> signalDirection = new List<int>();
			}
			public class TradingTest : PositionCommon {
				new RandomTestInner model;
				public TradingTest( RandomTestInner formula ) : base(formula) {
					this.model = formula;
				}
				public override double Current  {
					get { return base.current; }
				}
				public override void Change(double position, double price, TimeStamp time)
				{
					base.Change(position, price, time);
					if( model.prevSignal != position) {
						model.signalChanges.Add(model.Ticks[0]);
						model.signalDirection.Add(position);
						model.prevSignal = position;
					}
				}
			}
			public void TickConsoleWrite() {
				for( int i = 0; i< signalChanges.Count; i++) {
					Tick tick = signalChanges[i];
					double signal = signalDirection[i];
					log.Notice( i + ": " + tick + " Direction: " + signal);
				}
			}
		}
			
		public void TickProcessing()
		{
			strategy = new RandomTestInner();
			Starter starter = new HistoricalStarter();
    		starter.ProjectProperties.Starter.StartTime = new TimeStamp(2004,1,1,0,0,0);
    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(2007,1,1,0,0,0);
			starter.EndCount = 2049;
			starter.ProjectProperties.Starter.SetSymbols("USD_JPY_YEARS");
			starter.DataFolder = "TestData";
			starter.Run(strategy);
		}
		
		[Test]
		public void LongEntry() {
			TickProcessing();
			TimeStamp expected = new TimeStamp("2004-01-02 09:56:32.682");
			Assert.Greater(strategy.signalDirection.Count,4,"Long entry signal");
			Assert.AreEqual(expected,strategy.signalChanges[2].Time,"Long entry time");
			Assert.AreEqual(1,strategy.signalDirection[2],"Long entry signal");
		}
			
		[Test]
		public void FlatEntry() {
			TickProcessing();
			TimeStamp expected = new TimeStamp("2004-01-02 09:57:49.975");
			Assert.Greater(strategy.signalDirection.Count,5,"Flat entry signal");
			Assert.AreEqual(expected,strategy.signalChanges[3].Time,"Flat entry time");
			Assert.AreEqual(0,strategy.signalDirection[3],"Flat entry signal");
		}
			
		[Test]
		public void ShortEntry() {
			TickProcessing();
			TimeStamp expected = new TimeStamp("2004-01-02 10:40:04.991");
			Assert.Greater(strategy.signalDirection.Count,10,"Short entry signal");
			Assert.AreEqual(expected,strategy.signalChanges[8].Time,"Short entry time");
			Assert.AreEqual(-1,strategy.signalDirection[8],"Short entry signal");
		}
	}
}
#endif