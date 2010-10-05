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

#if TESTING
namespace TickZoom.TradingFramework
{

	
	[TestFixture]
	public class StrategyTickTest //: StrategyTest
	{
		protected string symbol = "USD_JPY";
		StrategySupportMock logic;
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
	    	[TestFixtureSetUp]
	    	public virtual void Init() {
			log.Notice("Setup StrategySupportTest");
			InitializeTick();
	    	}
	    	
		[Test]
		public void TestEmptyMethods()
		{
			Strategy logic = new Strategy();
			logic.OnEndHistorical();
	
			if( logic.OnIntervalOpen(Intervals.Define(BarUnit.Minute,1))) {
				Assert.Fail("NotImplementedException expected");
			} 
	
			if( logic.OnIntervalClose(Intervals.Define(BarUnit.Minute,1))) {
				Assert.Fail("NotImplementedException expected");
			}
		}
		
		public void InitializeTick()
		{
			logic = new StrategySupportMock();
			Starter starter = new HistoricalStarter();
			starter.EndCount = 1;
			starter.ProjectProperties.Starter.SetSymbols(symbol);
			Elapsed start = new Elapsed(6,0,0);
			Elapsed end = new Elapsed(15,0,0);
			starter.DataFolder = "Test\\DataCache";
			starter.Run(logic);
		}
		
		[Test]
		public void InitializeTickMinute()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,2,7,1,0);
			expected.EndTime = new TimeStamp(2005,5,2,7,2,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			BarsList bars = logic.openMinuteBars;
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void InitializeTickMinute30()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,2,7,0,0);
			expected.EndTime = new TimeStamp(2005,5,2,7,30,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			BarsList bars = logic.openMinute30Bars;
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void InitializeTickHour()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,2,7,0,0);
			expected.EndTime = new TimeStamp(2005,5,2,8,0,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			BarsList bars = logic.openHourBars;
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			TestBar(bars,expected,bars[0]);
		}
		[Test]
		public void InitializeTickDay()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,2,0,0,0);
			expected.EndTime = new TimeStamp(2005,5,3,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			BarsList bars = logic.openDayBars;
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void InitializeTickSession()
		{
			Assert.AreEqual(0,logic.openSessionBars.Count,"newSessionFlag");
		}
		[Test]
		public void InitializeTickWeek()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,1,0,0,0);
			expected.EndTime = new TimeStamp(2005,5,8,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			BarsList bars = logic.openWeekBars;
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			TestBar(bars,expected,bars[0]);
		}
		[Test]
		public void InitializeTickMonth()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,1,0,0,0);
			expected.EndTime = new TimeStamp(2005,6,1,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			BarsList bars = logic.openMonthBars;
			TestBar(bars,expected,bars[0]);
		}
		[Test]
		public void InitializeTickYear()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,1,1,0,0,0);
			expected.EndTime = new TimeStamp(2006,1,1,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			BarsList bars = logic.openYearBars;
			TestBar(bars,expected,bars[0]);
		}
			
		private void TestBar(BarsList bars,Bar expected,Bar bar) {
	//			for( int i=0; i<bars.Count;i++) {
	//				log.WriteFile(i+": Time="+bars[i].Time+"/"+bars[i].EndTime);
	//			}
			Assert.AreEqual(expected.EndTime,bar.EndTime,"EndTime");
			Assert.AreEqual(expected.Time,bar.Time,"Time");
			Assert.AreEqual(expected.TickTime,bar.TickTime,"TickTime");
			Assert.AreEqual(expected.Open,bar.Open,"Open");
			Assert.AreEqual(expected.High,bar.High,"High");
			Assert.AreEqual(expected.Low,bar.Low,"Low");
			Assert.AreEqual(expected.Close,bar.Close,"Close");
		}
	}
}
#endif
