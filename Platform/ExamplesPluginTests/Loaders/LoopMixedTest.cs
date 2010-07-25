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

#if LOOPTEST
namespace Loaders
{
	[TestFixture]
	public class LoopMixedTest {
		[Test]
		public void LoopTest() {
			for( int i=0; i<100; i++) {
				ExampleMixedTest test = new ExampleMixedTest();
				test.RunStrategy();
				test.TestSingleSymbolPortfolio();
				test.TestMultiSymbolPortfolio();
				test.CompareTradeCount();
				test.CompareAllPairs();
				test.RoundTurn1();
				test.RoundTurn2();
				test.LastRoundTurn();
				test.CompareBars0();
				test.CompareBars1();
				test.VerifyFullTickTrades();
				test.VerifyFullTickTradeCount();
				test.VerifyFullTickBarDataCount();
				test.VerifyFullTickBarData();
				test.VerifyFullTickStatsCount();
				test.VerifyFullTickStats();
				test.VerifyFourTicksTrades();
				test.VerifyFourTicksTradeCount();
				test.VerifyFourTicksBarData();
				test.VerifyFourTicksBarDataCount();
				test.VerifyFourTickStatsCount();
				test.VerifyFourTickStats();
				test.VerifySingleSymbolTrades();
				test.VerifySingleSymbolTradeCount();
				test.VerifySingleSymbolBarData();
				test.VerifySingleSymbolBarDataCount();
				test.VerifySingleSymbolStatsCount();
				test.VerifySingleSymbolStats();
				test.VerifyMultiSymbolStatsCount();
				test.VerifyMultiSymbolStats();
				test.CloseCharts();
			}
		}
	}
}
#endif
