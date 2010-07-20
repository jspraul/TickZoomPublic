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

#define FOREX
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.TickUtil;

#if LEVEL2
namespace TickZoom.Test
{
//	[TestFixture]
//	public class EquityLevel2 : TimeAndSales
//	{
//		[TestFixtureSetUp]
//		public override void Init()
//		{
//			base.Init();
//			symbol = Factory.Symbol.LookupSymbol("MSFT");
//		}	
//		
//		public override void AssertTick( TickIO tick, TickIO lastTick, ulong symbol) {
//        	Assert.IsFalse(tick.IsQuote);
//        	if( tick.IsQuote) {
//	        	Assert.Greater(tick.Bid,0);
//	        	Assert.Greater(tick.BidLevel(0),0);
//	        	Assert.Greater(tick.Ask,0);
//	        	Assert.Greater(tick.AskLevel(0),0);
//        	}
//        	Assert.IsTrue(tick.IsTrade);
//        	if( tick.IsTrade) {
//	        	Assert.Greater(tick.Price,0);
//    	    	Assert.Greater(tick.Size,0);
//        	}
//    		Assert.IsTrue(tick.Time>=lastTick.Time,"tick.Time > lastTick.Time");
//    		Assert.AreEqual(symbol,tick.lSymbol);
//		}
//	}
}
#endif