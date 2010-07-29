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
using TickZoom.Starters;

//using mscoree;







#if TESTING
namespace TickZoom.TradingFramework
{

	
	[TestFixture]
	public class ExitStrategyTickTest : MarshalByRefObject
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(ExitStrategyTickTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		ExitStrategyMock exitStrategy;
		
    	[TestFixtureSetUp]
    	public virtual void Init() {
			log.Notice("Setup ExitStrategyTest");
			TickProcessing();
    	}
    	
		public void TickProcessing() {
			Strategy random = new RandomCommon();
			exitStrategy = new ExitStrategyMock(random);
			random.IntervalDefault = Intervals.Day1;
			random.ExitStrategy = exitStrategy;
			Starter starter = new HistoricalStarter();
			starter.EndCount = 2048;
			starter.ProjectProperties.Starter.SetSymbols("USD_JPY_YEARS");
			starter.DataFolder = "TestData";
			starter.Run(random);
			
			Assert.AreEqual(exitStrategy,random.ExitStrategy);
		}
	
		[Test]
		public void LongEntry() {
			TimeStamp expected = new TimeStamp("2004-01-02 09:56:32.682");
			Assert.Greater(exitStrategy.signalChanges.Count,4,"Number of signal Changes");
			Assert.AreEqual(expected,exitStrategy.signalChanges[2],"Long entry time");
			Assert.AreEqual(1,exitStrategy.signalDirection[2],"Long entry signal");
		}
			
		[Test]
		public void FlatEntry() {
			TimeStamp expected = new TimeStamp("2004-01-02 09:57:49.975");
			Assert.Greater(exitStrategy.signalDirection.Count,5,"count of signal direction");
			Assert.AreEqual(0,exitStrategy.signalDirection[3],"Flat entry signal");
			Assert.AreEqual(expected,exitStrategy.signalChanges[3],"Flat entry time");
		}
			
		[Test]
		public void ShortEntry() {
			TimeStamp expected = new TimeStamp(2004,1,2,10,40,04,991);
			Assert.Greater(exitStrategy.signalDirection.Count,10,"count of signal direction");
			Assert.AreEqual(-1,exitStrategy.signalDirection[8],"Short entry signal");
			Assert.AreEqual(expected,exitStrategy.signalChanges[8],"Short entry time");
		}
	}
}
#endif
