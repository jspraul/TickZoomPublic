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
	public class ExchangeTimeZoneTest {
		protected string symbol = "USD_JPY2";
		StrategySupportMock logic;
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
	    [TestFixtureSetUp]
	    public virtual void Init() {
			log.Notice("Setup StrategySupportTest");
			InitializeTick();
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
			expected.Time = new TimeStamp("2005-05-02 03:01:00.000");
			expected.EndTime = new TimeStamp("2005-05-02 03:02:00.000");
			expected.TickTime = new TimeStamp("2005-05-02 03:01:34.606");
			BarsList bars = logic.openMinuteBars;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void InitializeTickMinute30()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp("2005-05-02 03:00:00.000");
			expected.EndTime = new TimeStamp("2005-05-02 03:30:00.000");
			expected.TickTime = new TimeStamp("2005-05-02 03:01:34.606");
			BarsList bars = logic.openMinute30Bars;
			TestBar(bars,expected,bars[0]);
		}
		
		[Test]
		public void InitializeTickHour()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp("2005-05-02 03:00:00.000");
			expected.EndTime = new TimeStamp("2005-05-02 04:00:00.000");
			expected.TickTime = new TimeStamp("2005-05-02 03:01:34.606");
			BarsList bars = logic.openHourBars;
			TestBar(bars,expected,bars[0]);
		}
		[Test]
		public void InitializeTickDay()
		{
			Bar expected = new Bar();
			expected.Time = new TimeStamp(2005,5,2,0,0,0);
			expected.EndTime = new TimeStamp(2005,5,3,0,0,0);
			expected.TickTime = new TimeStamp("2005-05-02 03:01:34.606");
			BarsList bars = logic.openDayBars;
			TestBar(bars,expected,bars[0]);
		}
		
		private void TestBar(BarsList bars,Bar expected,Bar bar) {
			Assert.AreEqual(expected.Time,bar.Time,"Time");
			Assert.AreEqual(expected.EndTime,bar.EndTime,"EndTime");
			Assert.AreEqual(expected.TickTime,bar.TickTime,"TickTime");
		}
	}
}
#endif
