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
using TickZoom.Interceptors;
using TickZoom.Starters;

#if TESTING
namespace TickZoom.TradingFramework
{
	[TestFixture]
	public class ExitStrategyMiscTest : MarshalByRefObject
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(ExitStrategyMiscTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		ExitStrategyMock exitStrategy;
		
		[Test]
		public void Constructor()
		{
			Strategy logic = new Strategy();
			exitStrategy = new ExitStrategyMock(logic);
			Assert.IsNotNull(exitStrategy,"ExitSupport constructor");
			logic.ExitStrategy = exitStrategy;
//			Assert.AreSame(logic.PositionSize,exitStrategy.Chain.Previous.Model,"Strategy property");
		}
		
		[Test]
		public void Variables()
		{
			Strategy logic = new Strategy();
			ExitStrategy strategy = new ExitStrategy(logic);
			Assert.AreEqual(false,strategy.ControlStrategy,"ControlStrategySignal");
			Assert.AreEqual(0,logic.Position.Current,"Signal");
			Assert.AreEqual(0,strategy.StopLoss,"Stop");
			Assert.AreEqual(0,strategy.TargetProfit,"Target");
		}
		
		[Test]
		public void DataSeriesSetup()
		{
	    	Strategy logic = new Strategy();
			ExitStrategy exit = logic.ExitStrategy;
			
			Starter starter = new HistoricalStarter();
			starter.EndCount = 1;
			starter.ProjectProperties.Starter.SetSymbols("USD_JPY_YEARS");
			starter.ProjectProperties.Starter.IntervalDefault = Intervals.Hour1;
			starter.DataFolder = "TestData";
			starter.Run(logic);
			
			Assert.AreSame(logic.Hours,logic.Hours,"Exit Signal before entry");
			Assert.AreSame(logic.Ticks,logic.Ticks,"Exit Signal before entry");
			Assert.AreEqual(1,logic.Hours.Count,"Number of hour bars ");
			Assert.AreEqual(1,logic.Ticks.Count,"Number of tick bars ");
		}
		
	}
}
#endif
