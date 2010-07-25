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

#if REFACTORED

namespace TickZoom.Indicators
{
	[TestFixture]
	public class SMATest
	{
		SMA sma;
		Data bars;
		Ticks ticks;
		[TestFixtureSetUp]
		public void Constructor()
		{
			SymbolInfo properties = new SymbolPropertiesImpl();
 			DataImpl data = new DataImpl(properties,10000,1000);
			sma = new SMA(data.Get(IntervalsInternal.Day1).Close,14);
			sma.IntervalDefault = IntervalsInternal.Day1;
			sms.OnConfigure();
			sma.OnInitialize();
			Assert.IsNotNull(sma, "sma constructor");
			ticks = new TickSeries(10000);
			bars = data.GetInternal(IntervalsInternal.Day1);
			Assert.AreEqual(0,sma.Count, "sma list");
		}
		[TestFixtureTearDown]
		public void TestFixtureTearDown() {
			ticks.Release();
		}
		[Test]
		public void Values()
		{
			TickImpl tick = new TickImpl();
			TimeStamp timeStamp = new TimeStamp();
			for( int i = 0; i < data.Length; i++) {
				timeStamp = TimeStamp.UtcNow;
				tick.init(timeStamp,data[i],data[i]);
				TickWrapper wrapper = new TickWrapper();
				wrapper.SetTick( tick);
				bars.AddBar( ref wrapper,tick.Time);
				sma.OnBeforeIntervalOpen();
				sma.OnIntervalClose();
				Assert.AreEqual(Math.Round(result[i]),Math.Round(sma[0]),"current result at " + i + " with value: " + sma[0]);
				if( i > 1) Assert.AreEqual(Math.Round(result[i-1]),Math.Round(sma[1]),"result 1 back at " + i);
				if( i > 2) Assert.AreEqual(Math.Round(result[i-2]),Math.Round(sma[2]),"result 2 back at " + i);
			}
		}
		
		private double[] data = new double[] {
			10000,
			10100,
			10040,
			10200,
			10760,
			11190,
			11300,
			12030,
			12360,
			12150,
			12440,
			12910,
			13270,
			12550,
			11890,
			12350,
			11930,
			11900,
			11370,
			10820,
			10720,
			11570,
			12520,
			13290,
			13590,
			13850,
			13500,
			13810,
			14430,
			13800,
			14140,
			13850,
			13210,
			13480,
			14140,
			14250,
			13600,
			13160,
			12940,
			13670,
			13770,
			13150,
			12990,
			12360,
			12580,
			13220,
			12220,
			11800,
			12230,
			11580,
			10680,
			9940,
			10300,
			11030,
			11790,
			11890
		};

		private double[] result = new double[] {
			10000,
			10050,
			10046.667,
			10085,
			10220,
			10381.667,
			10512.857,
			10702.5,
			10886.667,
			11013,
			11142.727,
			11290,
			11442.308,
			11521.429,
			11656.429,
			11817.142,
			11952.143,
			12073.571,
			12117.143,
			12090.714,
			12049.286,
			12016.429,
			12027.857,
			12109.286,
			12191.429,
			12258.571,
			12275,
			12365,
			12546.429,
			12650,
			12807.857,
			12947.143,
			13078.571,
			13268.571,
			13512.857,
			13704.286,
			13781.429,
			13772.143,
			13725.714,
			13712.857,
			13732.143,
			13685,
			13582.143,
			13479.286,
			13367.857,
			13322.857,
			13252.143,
			13132.143,
			12995.714,
			12805,
			12596.429,
			12366.429,
			12177.857,
			11989.286,
			11847.857,
			11757.857,
		};
		
	}
}
#endif