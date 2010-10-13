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
using TickZoom.Examples;
using TickZoom.Transactions;

namespace Loaders
{
	[TestFixture]
	public class ExampleDualStrategyTest : StrategyTest
	{
		#region SetupTest
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		ExampleReversalStrategy exampleReversal;
		ExampleOrderStrategy fourTicksStrategy;
		Portfolio portfolio;
		
		public ExampleDualStrategyTest() {
			Symbols = "Daily4Sim";
			StoreKnownGood = false;
			ShowCharts = false;
		}
		
		[TestFixtureSetUp]
		public override void RunStrategy() {
			base.RunStrategy();
			try {
				Starter starter = CreateStarterCallback();
				
				// Set run properties as in the GUI.
				starter.ProjectProperties.Starter.StartTime = new TimeStamp(1800,1,1);
	    		starter.ProjectProperties.Starter.EndTime = new TimeStamp(1990,1,1);
	    		starter.DataFolder = "Test\\DataCache";
	    		starter.ProjectProperties.Starter.SetSymbols( Symbols);
				starter.ProjectProperties.Starter.IntervalDefault = Intervals.Day1;
				starter.ProjectProperties.Engine.RealtimeOutput = false;
				
				// Set up chart
		    	starter.CreateChartCallback = new CreateChartCallback(HistoricalCreateChart);
	    		starter.ShowChartCallback = HistoricalShowChart;
	
				// Run the loader.
				ModelLoaderCommon loader = new ExampleDualStrategyLoader();
	    		starter.Run(loader);
	
	 			// Get the stategy
	    		portfolio = loader.TopModel as Portfolio;
	    		int cnt = 0;
	    		if( portfolio.Strategies[cnt] is ExampleOrderStrategy) {
	    			fourTicksStrategy = portfolio.Strategies[cnt] as ExampleOrderStrategy;
	    			cnt++;
	    		}
	    		if( portfolio.Strategies.Count > cnt && portfolio.Strategies[cnt] is ExampleReversalStrategy) {
		    		exampleReversal = portfolio.Strategies[cnt] as ExampleReversalStrategy;
	    		}
	    		LoadTransactions();
	    		LoadTrades();
			} catch( Exception ex) {
				log.Error("Setup error.",ex);
				throw;
			}
		}
		#endregion
		
		[Test]
		public void CheckPortfolio() {
			Assert.NotNull(exampleReversal);
			Assert.NotNull(fourTicksStrategy);
			double expected = exampleReversal.Performance.Equity.CurrentEquity;
			expected -= exampleReversal.Performance.Equity.StartingEquity;
			expected += fourTicksStrategy.Performance.Equity.CurrentEquity;
			expected -= fourTicksStrategy.Performance.Equity.StartingEquity;
			double portfolioTotal = portfolio.Performance.Equity.CurrentEquity;
			portfolioTotal -= portfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(expected,2), Math.Round(portfolioTotal,2));
			Assert.AreEqual(Math.Round(-27.07,2), Math.Round(portfolioTotal,2));
		}
		
		[Test]
		public void CheckPortfolioClosedEquity() {
			Assert.NotNull(exampleReversal);
			Assert.NotNull(fourTicksStrategy);
			double expected = exampleReversal.Performance.Equity.ClosedEquity;
			expected -= exampleReversal.Performance.Equity.StartingEquity;
			expected += fourTicksStrategy.Performance.Equity.ClosedEquity;
			expected -= fourTicksStrategy.Performance.Equity.StartingEquity;
			double portfolioTotal = portfolio.Performance.Equity.ClosedEquity;
			portfolioTotal -= portfolio.Performance.Equity.StartingEquity;
			Assert.AreEqual(Math.Round(expected,2), Math.Round(portfolioTotal,2));
			Assert.AreEqual(Math.Round(-26.90,2), Math.Round(portfolioTotal,2));
		}
		
		[Test]
		public void CheckPortfolioOpenEquity() {
			Assert.NotNull(exampleReversal);
			Assert.NotNull(fourTicksStrategy);
			double expected = exampleReversal.Performance.Equity.OpenEquity;
			expected += fourTicksStrategy.Performance.Equity.OpenEquity;
			Assert.AreEqual(Math.Round(expected,2), Math.Round(portfolio.Performance.Equity.OpenEquity,2));
			Assert.AreEqual(Math.Round(-0.17,2), portfolio.Performance.Equity.OpenEquity);
		}
		
		[Test]
		public void CompareBars0() {
			Assert.NotNull(fourTicksStrategy);
			CompareChart(fourTicksStrategy,GetChart(fourTicksStrategy.SymbolDefault));
		}
		
		[Test]
		public void CompareBars1() {
			Assert.NotNull(exampleReversal);
			CompareChart(exampleReversal,GetChart(exampleReversal.SymbolDefault));
		}
		
		[Test]
		public void VerifyPortfolioTrades() {
			VerifyTrades(portfolio);
		}
		
		[Test]
		public void VerifyStrategy1Trades() {
			Assert.NotNull(fourTicksStrategy);
			VerifyTrades(fourTicksStrategy);
		}
	
		[Test]
		public void VerifyStrategy2Trades() {
			Assert.NotNull(exampleReversal);
			VerifyTrades(exampleReversal);
		}
		
		[Test]
		public void VerifyPortfolioTradeCount() {
			VerifyTradeCount(portfolio);
		}
		
		[Test]
		public void VerifyStrategy1TradeCount() {
			Assert.NotNull(fourTicksStrategy);
			VerifyTradeCount(fourTicksStrategy);
		}
		
		[Test]
		public void VerifyStrategy2TradeCount() {
			Assert.NotNull(exampleReversal);
			VerifyTradeCount(exampleReversal);
		}
	}
	
	public class ExampleDualStrategyLoader : ModelLoaderCommon
	{
		public ExampleDualStrategyLoader() {
			/// <summary>
			/// IMPORTANT: You can personalize the name of each model loader.
			/// </summary>
			category = "Example";
			name = "Dual Symbol";
			IsVisibleInGUI = false;
		}
		
		public override void OnInitialize(ProjectProperties properties) {
		}
		
		public override void OnLoad(ProjectProperties properties) {
			var portfolio = new Portfolio();
			var fourTicks = new ExampleOrderStrategy() {
				Name = "FourTicksData"
			};
			fourTicks.SymbolDefault = properties.Starter.SymbolInfo[0].Symbol;
			var reversal = new ExampleReversalStrategy() {
				SymbolDefault = properties.Starter.SymbolInfo[0].Symbol
			};
			AddDependency(portfolio,fourTicks);
			AddDependency(portfolio,reversal);
			portfolio.Performance.GraphTrades = false;
			TopModel = portfolio;
		}
	}
}
