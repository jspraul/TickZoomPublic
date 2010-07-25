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
	public struct Bar {
		public Bar(TimeStamp time, TimeStamp endTime, double open, double high, double low, double close, TimeStamp tickTime) {
			Time = time;
			EndTime = endTime;
			Open = open;
			High = high;
			Low = low;
			Close = close;
			TickTime = tickTime;
		}
		public TimeStamp Time;
		public TimeStamp EndTime;
		public double Open;
		public double High;
		public double Low;
		public double Close;
		public TimeStamp TickTime;
		
		public override string ToString()
		{
			return "Bar " + Time + " - " + EndTime;
		}
	}
	
	public class BarsList : List<Bar> {
		public new Bar this[int position] {
			get { return base[(base.Count-1) - position]; }
		}
	}
	
	[TestFixture]
	public class StrategyWeekTest //: StrategyTest
	{
		StrategySupportMock logic;
		StrategySupportMock weeklogic;
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    	[TestFixtureSetUp]
    	public virtual void Init() {
			log.Notice("Setup StrategySupportTest");
			WeekProcessingSetup();
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
			
		public void WeekProcessingSetup()
		{
			logic = new StrategySupportMock();
			weeklogic = new StrategySupportMock();
			Starter starter = new HistoricalStarter();
			starter.ProjectProperties.Starter.SetSymbols("USD_JPY");
			starter.ProjectProperties.Starter.SymbolInfo[0].SessionStart = new Elapsed(6,20,0);
			starter.ProjectProperties.Starter.SymbolInfo[0].SessionEnd = new Elapsed(15,0,0);
			starter.DataFolder = "TestData";
			starter.Run(weeklogic);
		}
		
		[Test]
		public void MinuteOpen()
		{
			BarsList bars = weeklogic.openMinuteBars;
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,15,59,0);
			expected.EndTime = new TimeStamp(2005,5,31,16,0,0);
			expected.TickTime = new TimeStamp(2005,5,31,15,59,06,877);
			expected.Open = 108.48;
			expected.High = 108.48;
			expected.Low = 108.48;
			expected.Close = 108.48;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void MinuteOpen30()
		{
			BarsList bars = weeklogic.openMinute30Bars;
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,15,50,0);
			expected.EndTime = new TimeStamp(2005,5,31,16,20,0);
			expected.TickTime = new TimeStamp(2005,5,31,15,50,22,129);
			expected.Open = 108.43;
			expected.High = 108.43;
			expected.Low = 108.43;
			expected.Close = 108.43;
			TestBar(bars,expected,bars[0]);
		}
		
		private void TestBar(BarsList bars,Bar expected,Bar bar) {
//			for( int i=0; i<bars.Count;i++) {
//				log.WriteFile(i+": Time="+bars[i].Time+"/"+bars[i].EndTime);
//			}
			Assert.AreEqual(expected.Time,bar.Time,"Time");
			Assert.AreEqual(expected.EndTime,bar.EndTime,"EndTime");
			Assert.AreEqual(expected.TickTime,bar.TickTime,"TickTime");
			Assert.AreEqual(expected.Open,bar.Open,"Open");
			Assert.AreEqual(expected.High,bar.High,"High");
			Assert.AreEqual(expected.Low,bar.Low,"Low");
			Assert.AreEqual(expected.Close,bar.Close,"Close");
		}
		
		[Test]
		public void HourOpen()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,15,20,0);
			expected.EndTime = new TimeStamp(2005,5,31,16,20,0);
			expected.TickTime = new TimeStamp(2005,5,31,15,20,01,991);
			expected.Open = 108.34;
			expected.High = 108.34;
			expected.Low = 108.34;
			expected.Close = 108.34;
			BarsList bars = weeklogic.openHourBars;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void DayOpen()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,0,0,0,0);
			expected.EndTime = new TimeStamp(2005,6,1,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,31,7,0,2,910);
			expected.Open = 108.0;
			expected.High = 108.0;
			expected.Low = 108.0;
			expected.Close = 108.0;
			BarsList bars = weeklogic.openDayBars;
			TestBar(bars,expected,bars[0]);
		}
		
//		[Test]
		public void Session()
		{
			TimeStamp expected = new TimeStamp(2005,5,31,7,0,02,910);
			Assert.AreEqual(expected,weeklogic.closeSessionBars[0].TickTime,"newSessionFlag");
		}
		[Test]
		public void WeekOpen()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,29,0,0,0);
			expected.EndTime = new TimeStamp(2005,6,5,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,30,7,0,15,339);
			BarsList bars = weeklogic.openWeekBars;
			expected.Open = 107.84;
			expected.High = 107.84;
			expected.Low = 107.84;
			expected.Close = 107.84;
			TestBar(bars,expected,bars[0]);
		}
		[Test]
		public void YearOpen()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,1,1,0,0,0);
			expected.EndTime = new TimeStamp(2006,1,1,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			BarsList bars = weeklogic.openYearBars;
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			TestBar(bars,expected,bars[0]);
		}
			
		[Test]
		public void MinuteClose()
		{
			BarsList bars = weeklogic.closeMinuteBars;
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,15,58,0);
			expected.EndTime = new TimeStamp(2005,5,31,15,59,0);
			expected.TickTime = new TimeStamp("2005-05-31 15:58:52.848");
			expected.Open = 108.46;
			expected.High = 108.48;
			expected.Low = 108.46;
			expected.Close = 108.47;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void MinuteClose30()
		{
			BarsList bars = weeklogic.closeMinute30Bars;
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,15,20,0);
			expected.EndTime = new TimeStamp(2005,5,31,15,50,0);
			expected.TickTime = new TimeStamp("2005-05-31 15:49:55.113");
			expected.Open = 108.34;
			expected.High = 108.51;
			expected.Low = 108.32;
			expected.Close = 108.42;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void HourClose()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,14,20,0,0);
			expected.EndTime = new TimeStamp(2005,5,31,15,20,0);
			expected.TickTime = new TimeStamp("2005-05-31 15:19:44.966");
			expected.Open = 108.16;
			expected.High = 108.33;
			expected.Low = 108.15;
			expected.Close = 108.33;
			BarsList bars = weeklogic.closeHourBars;
			TestBar(bars,expected,bars[0]);
		}
		[Test]
		public void DayClose()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,30,0,0,0,0);
			expected.EndTime = new TimeStamp(2005,5,31,0,0,0);
			expected.TickTime = new TimeStamp("2005-05-30 11:59:43.211");
			expected.Open = 107.84;
			expected.High = 107.97;
			expected.Low = 107.81;
			expected.Close = 107.88;
			BarsList bars = weeklogic.closeDayBars;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void SessionOpen()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,31,6,20,0,0);
			expected.EndTime = new TimeStamp(2005,5,31,15,0,0);
			expected.TickTime = new TimeStamp(2005,5,31,7,0,2,910);
			BarsList bars = weeklogic.openSessionBars;
			expected.Open = 108.00;
			expected.High = 108.00;
			expected.Low = 108.00;
			expected.Close = 108.00;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void SessionClose()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,30,6,20,0,0);
			expected.EndTime = new TimeStamp(2005,5,30,15,0,0);
			expected.TickTime = new TimeStamp("2005-05-30 11:59:43.211");
			BarsList bars = weeklogic.closeSessionBars;
			expected.Open = 107.84;
			expected.High = 107.97;
			expected.Low = 107.81;
			expected.Close = 107.88;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void WeekClose()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,22,0,0,0);
			expected.EndTime = new TimeStamp(2005,5,29,0,0,0);
			expected.TickTime = new TimeStamp("2005-05-27 15:59:58.878");
			BarsList bars = weeklogic.closeWeekBars;
			expected.Open = 107.86;
			expected.High = 108.25;
			expected.Low = 107.21;
			expected.Close = 107.97;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void MonthOpen()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,1,0,0,0);
			expected.EndTime = new TimeStamp(2005,6,1,0,0,0);
			expected.TickTime = new TimeStamp(2005,5,2,7,1,34,606);
			BarsList bars = weeklogic.openMonthBars;
			expected.Open = 105.31;
			expected.High = 105.31;
			expected.Low = 105.31;
			expected.Close = 105.31;
			TestBar(bars,expected,bars[0]);
		}
	}
}
#endif