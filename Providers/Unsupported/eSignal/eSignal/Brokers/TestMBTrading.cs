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
using System.Windows.Forms;

using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using TickZoom.Api;
//using TickZoom.Engine;
using TickZoom.TickLib;
using TickZoom.TickUtil;
using TickZoom.MBTrading;

namespace TickZoom.TradingFramework
{
//	[TestFixture]
	public class TestMBTrading
	{
		TickReader tickArray;
//		[TestFixtureSetUp]
		public void Init()
		{
    		tickArray = new TickReader();
//    		tickArray.TimeFrames.Add( Period.Minute10);
//    		tickArray.TimeFrames.Add( Period.Hour4);
    		tickArray.Initialize("UnitTestData","USD_JPY");
		}
		
//		[TestFixtureTearDown]
		public void Dispose()
		{
			tickArray = null;
//			ModelContext.Context.Reset();
		}
		
//		[Test]
		public void RunLiveTest()
		{
    		Starter starter = new RealTimeStarter();
    		starter.Symbol = "USD_JPY";
    		starter.Run();
    		
// 			TODO find a way to connect to engine to test.
//    		Assert.AreEqual(4,engine.Model.Data.Get(Intervals.Hour1).BarCount,"Number of hour bars ");
//			Assert.AreEqual(1000,engine.Model.Data.Ticks.BarCount,"Number of tick bars ");
		}
		
//		[Test]
		public void RunQuoteServerTest()
		{
    		
            try
            {
                MBTInterface.Login(3712, "DEMOXQEI", "1dust2jeep");
                MBTInterface.AddDepth("ES",true);
//                mbt.InstrumentReaders.SaveDepth("USD/JPY");
//                mbt.InstrumentReaders.SaveDepth("USD/CHF");
//
//                mbt.InstrumentReaders.SaveDepth("USD/CAD");
//                mbt.InstrumentReaders.SaveDepth("AUD/USD");
//                mbt.InstrumentReaders.SaveDepth("CHF/JPY");
//                mbt.InstrumentReaders.SaveDepth("USD/NOK");
//                mbt.InstrumentReaders.SaveDepth("EUR/USD");
//
//                mbt.InstrumentReaders.SaveDepth("USD/SEK");
//                mbt.InstrumentReaders.SaveDepth("USD/DKK");
//                mbt.InstrumentReaders.SaveDepth("GBP/USD");
//                mbt.InstrumentReaders.SaveDepth("EUR/CHF");
//                mbt.InstrumentReaders.SaveDepth("EUR/JPY");
//
//                mbt.InstrumentReaders.SaveDepth("GBP/JPY");
//                mbt.InstrumentReaders.SaveDepth("EUR/GBP");
//                mbt.InstrumentReaders.SaveDepth("EUR/NOK");
//                mbt.InstrumentReaders.SaveDepth("EUR/SEK");
//                mbt.InstrumentReaders.SaveDepth("GBP/CHF");
//                
//                mbt.InstrumentReaders.SaveDepth("AUD/JPY");
//                mbt.InstrumentReaders.SaveDepth("CAD/JPY");
//                mbt.InstrumentReaders.SaveDepth("NZD/USD");
//                mbt.InstrumentReaders.SaveDepth("AUD/CHF");
//                mbt.InstrumentReaders.SaveDepth("AUD/CAD");
                
            }
            catch (Exception problem)
            {
                throw new ApplicationException(problem.Message);
            }

            int endTick = Environment.TickCount + 10 * 1000;
            while(Environment.TickCount < endTick ) {
            	Application.DoEvents();
    		}
    		MBTInterface.Logout();
		}
	}
}
