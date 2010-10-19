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
using System.IO;
using System.Text;
using System.Threading;

using NUnit.Framework;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Starters;
using TickZoom.Transactions;
using ZedGraph;

namespace Loaders
{
	public delegate Starter CreateStarterCallback();
	
	public class StrategyTest
	{
		static readonly Log log = Factory.SysLog.GetLogger(typeof(StrategyTest));
		static readonly bool debug = log.IsDebugEnabled;
		private string testFileName;
		string dataFolder = "Test\\DataCache";
		string symbols;
		List<ChartThread> chartThreads = new List<ChartThread>();
		Dictionary<string,List<StatsInfo>> goodStatsMap = new Dictionary<string,List<StatsInfo>>();
		Dictionary<string,List<StatsInfo>> testStatsMap = new Dictionary<string,List<StatsInfo>>();
		Dictionary<string,List<BarInfo>> goodBarDataMap = new Dictionary<string,List<BarInfo>>();
		Dictionary<string,List<BarInfo>> testBarDataMap = new Dictionary<string,List<BarInfo>>();
		Dictionary<string,List<TradeInfo>> goodTradeMap = new Dictionary<string,List<TradeInfo>>();
		Dictionary<string,List<TradeInfo>> testTradeMap = new Dictionary<string,List<TradeInfo>>();
		Dictionary<string,List<TransactionInfo>> goodTransactionMap = new Dictionary<string,List<TransactionInfo>>();
		Dictionary<string,List<TransactionInfo>> testTransactionMap = new Dictionary<string,List<TransactionInfo>>();
		Dictionary<string,List<TransactionInfo>> goodReconciliationMap = new Dictionary<string,List<TransactionInfo>>();
		Dictionary<string,List<TransactionInfo>> testReconciliationMap = new Dictionary<string,List<TransactionInfo>>();
		public bool ShowCharts = false;
		public bool StoreKnownGood = false;
		public CreateStarterCallback createStarterCallback;
		protected bool testFailed = false;		
		
		public StrategyTest() {
 			testFileName = GetType().Name;
			createStarterCallback = CreateStarter;
		}
		
		public void MatchTestResultsOf( Type type) {
			testFileName = type.Name;
		}
		
		[TestFixtureSetUp]
		public virtual void RunStrategy() {
			string filePath = Factory.SysLog.LogFolder + @"\Trades.log";
			File.Delete(filePath);
			filePath = Factory.SysLog.LogFolder + @"\BarData.log";
			File.Delete(filePath);
			filePath = Factory.SysLog.LogFolder + @"\Stats.log";
			File.Delete(filePath);
			filePath = Factory.SysLog.LogFolder + @"\Transactions.log";
			File.Delete(filePath);
		}
		
		[TestFixtureTearDown]
		public virtual void EndStrategy() {
			if( !ShowCharts) {
	   			HistoricalCloseCharts();
			} else {
				while( TryCloseCharts() == false) {
					Thread.Sleep(100);
				}
			}
			int AddLogBindErrors = 0;
//			SocketDefault.LogBindErrors();

//			if( testFailed) {
//				log.Error("Shutting down due to test failure.");
//				Environment.Exit(1);
//			}
		}
		
		public void SetupWarehouseConfig(string providerAssembly, ushort servicePort)
		{
			try { 
				string storageFolder = Factory.Settings["AppDataFolder"];
				var providersPath = Path.Combine(storageFolder,"Providers");
				string configPath = Path.Combine(providersPath,"ProviderCommon");
				string configFile = Path.Combine(configPath,"WarehouseTest.config");
				ConfigFile warehouseConfig = new ConfigFile(configFile);
				warehouseConfig.SetValue("ServerCacheFolder","Test\\ServerCache");
				warehouseConfig.SetValue("ServiceAddress","0.0.0.0");
				warehouseConfig.SetValue("ServicePort",servicePort.ToString());
				warehouseConfig.SetValue("ProviderAssembly",providerAssembly);
	 			// Clear the history files
			} catch( Exception ex) {
				log.Error("Setup error.",ex);
				throw ex;
			}
		}
		
		public class TransactionInfo {
			public string Symbol;
			public LogicalFillBinary Fill;
		}
		
		public class TradeInfo {
			public double ClosedEquity;
			public double ProfitLoss;
			public TransactionPairBinary Trade;
		}
		
		public class BarInfo {
			public TimeStamp Time;
			public double Open;
			public double High;
			public double Low;
			public double Close;
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Time + ", ");
				sb.Append(Open + ", ");
				sb.Append(High + ", ");
				sb.Append(Low + ", ");
				sb.Append(Close);
				return sb.ToString();
			}
		}
		
		public class StatsInfo {
			public TimeStamp Time;
			public double ClosedEquity;
			public double OpenEquity;
			public double CurrentEquity;
		}
		
		private Starter CreateStarter() {
			return new HistoricalStarter();			
		}
		
		public void LoadTrades() {
			string fileDir = @"..\..\Platform\ExamplesPluginTests\Loaders\Trades\";
			string knownGoodPath = fileDir + testFileName + "Trades.log";
			string newPath = Factory.SysLog.LogFolder + @"\Trades.log";
			if( !File.Exists(newPath)) return;
			if( StoreKnownGood) {
				File.Copy(newPath,knownGoodPath,true);
			}
			goodTradeMap.Clear();
			LoadTrades(knownGoodPath,goodTradeMap);
			testTradeMap.Clear();
			LoadTrades(newPath,testTradeMap);
		}
		
		public void LoadTrades(string filePath, Dictionary<string,List<TradeInfo>> tempTrades) {
			if( !File.Exists(filePath)) return;
			using( FileStream fileStream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) {
				StreamReader file = new StreamReader(fileStream);
				string line;
				while( (line = file.ReadLine()) != null) {
					string[] fields = line.Split(',');
					int fieldIndex = 0;
					string strategyName = fields[fieldIndex++];
					TradeInfo testInfo = new TradeInfo();
					
					testInfo.ClosedEquity = double.Parse(fields[fieldIndex++]);
					testInfo.ProfitLoss = double.Parse(fields[fieldIndex++]);
					
					line = string.Join(",",fields,fieldIndex,fields.Length-fieldIndex);
					testInfo.Trade = TransactionPairBinary.Parse(line);
					List<TradeInfo> tradeList;
					if( tempTrades.TryGetValue(strategyName,out tradeList)) {
						tradeList.Add(testInfo);
					} else {
						tradeList = new List<TradeInfo>();
						tradeList.Add(testInfo);
						tempTrades.Add(strategyName,tradeList);
					}
				}
			}
		}
		
		public void LoadTransactions() {
			string fileDir = @"..\..\Platform\ExamplesPluginTests\Loaders\Trades\";
			string knownGoodPath = fileDir + testFileName + "Transactions.log";
			string newPath = Factory.SysLog.LogFolder + @"\Transactions.log";
			if( !File.Exists(newPath)) return;
			if( StoreKnownGood) {
				File.Copy(newPath,knownGoodPath,true);
			}
			goodTransactionMap.Clear();
			LoadTransactions(knownGoodPath,goodTransactionMap);
			testTransactionMap.Clear();
			LoadTransactions(newPath,testTransactionMap);
		}
		
		public void LoadTransactions(string filePath, Dictionary<string,List<TransactionInfo>> tempTransactions) {
			if( !File.Exists(filePath)) return;
			using( FileStream fileStream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) {
				StreamReader file = new StreamReader(fileStream);
				string line;
				while( (line = file.ReadLine()) != null) {
					string[] fields = line.Split(',');
					int fieldIndex = 0;
					string strategyName = fields[fieldIndex++];
					TransactionInfo testInfo = new TransactionInfo();
					
					testInfo.Symbol = fields[fieldIndex++];
					
					line = string.Join(",",fields,fieldIndex,fields.Length-fieldIndex);
					testInfo.Fill = LogicalFillBinary.Parse(line);
					List<TransactionInfo> transactionList;
					if( tempTransactions.TryGetValue(strategyName,out transactionList)) {
						transactionList.Add(testInfo);
					} else {
						transactionList = new List<TransactionInfo>();
						transactionList.Add(testInfo);
						tempTransactions.Add(strategyName,transactionList);
					}
				}
			}
		}
		
		public void LoadReconciliation() {
			string fileDir = @"..\..\Platform\ExamplesPluginTests\Loaders\Trades\";
			string knownGoodPath = fileDir + testFileName + "Transactions.log";
			string newPath = Factory.SysLog.LogFolder + @"\MockProviderTransactions.log";
			if( !File.Exists(newPath)) return;
			if( StoreKnownGood) {
				File.Copy(newPath,knownGoodPath,true);
			}
			goodReconciliationMap.Clear();
			LoadReconciliation(knownGoodPath,goodReconciliationMap);
			testReconciliationMap.Clear();
			LoadReconciliation(newPath,testReconciliationMap);
		}
		
		public void LoadReconciliation(string filePath, Dictionary<string,List<TransactionInfo>> tempReconciliation) {
			if( !File.Exists(filePath)) return;
			using( FileStream fileStream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) {
				StreamReader file = new StreamReader(fileStream);
				string line;
				while( (line = file.ReadLine()) != null) {
					string[] fields = line.Split(',');
					int fieldIndex = 0;
					string strategyName = fields[fieldIndex++];
					TransactionInfo testInfo = new TransactionInfo();
					
					testInfo.Symbol = fields[fieldIndex++];
					
					line = string.Join(",",fields,fieldIndex,fields.Length-fieldIndex);
					testInfo.Fill = LogicalFillBinary.Parse(line);
					List<TransactionInfo> transactionList;
					if( tempReconciliation.TryGetValue(testInfo.Symbol,out transactionList)) {
						transactionList.Add(testInfo);
					} else {
						transactionList = new List<TransactionInfo>();
						transactionList.Add(testInfo);
						tempReconciliation.Add(testInfo.Symbol,transactionList);
					}
				}
			}
		}
		
		public void LoadBarData() {
			string fileDir = @"..\..\Platform\ExamplesPluginTests\Loaders\Trades\";
			string newPath = Factory.SysLog.LogFolder + @"\BarData.log";
			string knownGoodPath = fileDir + testFileName + "BarData.log";
			if( !File.Exists(newPath)) return;
			if( StoreKnownGood) {
				File.Copy(newPath,knownGoodPath,true);
			}
			goodBarDataMap.Clear();
			LoadBarData(knownGoodPath,goodBarDataMap);
			testBarDataMap.Clear();
			LoadBarData(newPath,testBarDataMap);
		}
		
		public void LoadBarData(string filePath, Dictionary<string,List<BarInfo>> tempBarData) {
			using( FileStream fileStream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) {
				StreamReader file = new StreamReader(fileStream);
				string line;
				while( (line = file.ReadLine()) != null) {
					string[] fields = line.Split(',');
					int fieldIndex = 0;
					string strategyName = fields[fieldIndex++];
					BarInfo barInfo = new BarInfo();
					
					barInfo.Time = new TimeStamp(fields[fieldIndex++]);
					barInfo.Open = double.Parse(fields[fieldIndex++]);
					barInfo.High = double.Parse(fields[fieldIndex++]);
					barInfo.Low = double.Parse(fields[fieldIndex++]);
					barInfo.Close = double.Parse(fields[fieldIndex++]);
					
					List<BarInfo> barList;
					if( tempBarData.TryGetValue(strategyName,out barList)) {
						barList.Add(barInfo);
					} else {
						barList = new List<BarInfo>();
						barList.Add(barInfo);
						tempBarData.Add(strategyName,barList);
					}
				}
			}
		}
		
		public void LoadStats() {
			string fileDir = @"..\..\Platform\ExamplesPluginTests\Loaders\Trades\";
			string newPath = Factory.SysLog.LogFolder + @"\Stats.log";
			string knownGoodPath = fileDir + testFileName + "Stats.log";
			if( !File.Exists(newPath)) return;
			if( StoreKnownGood) {
				File.Copy(newPath,knownGoodPath,true);
			}
			testStatsMap.Clear();
			LoadStats(newPath,testStatsMap);
			goodStatsMap.Clear();
			LoadStats(knownGoodPath,goodStatsMap);
		}
		
		public void LoadStats(string filePath, Dictionary<string,List<StatsInfo>> tempStats) {
			using( FileStream fileStream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) {
				StreamReader file = new StreamReader(fileStream);
				string line;
				while( (line = file.ReadLine()) != null) {
					string[] fields = line.Split(',');
					int fieldIndex = 0;
					string strategyName = fields[fieldIndex++];
					
					StatsInfo statsInfo = new StatsInfo();
					statsInfo.Time = new TimeStamp(fields[fieldIndex++]);
					statsInfo.ClosedEquity = double.Parse(fields[fieldIndex++]);
					statsInfo.OpenEquity = double.Parse(fields[fieldIndex++]);
					statsInfo.CurrentEquity = double.Parse(fields[fieldIndex++]);

					List<StatsInfo> statsList;
					if( tempStats.TryGetValue(strategyName,out statsList)) {
						statsList.Add(statsInfo);
					} else {
						statsList = new List<StatsInfo>();
						statsList.Add(statsInfo);
						tempStats.Add(strategyName,statsList);
					}
				}
			}
		}
		
		public void VerifyTradeCount(StrategyInterface strategy) {
			List<TradeInfo> goodTrades = null;
			goodTradeMap.TryGetValue(strategy.Name,out goodTrades);
			List<TradeInfo> testTrades = null;
			testTradeMap.TryGetValue(strategy.Name,out testTrades);
			Assert.IsNotNull(goodTrades, "good trades");
			Assert.IsNotNull(testTrades, "test trades");
			Assert.AreEqual(goodTrades.Count,testTrades.Count,"trade count");
		}
		
		public void VerifyTransactionCount(StrategyInterface strategy) {
			List<TransactionInfo> goodTransactions = null;
			goodTransactionMap.TryGetValue(strategy.Name,out goodTransactions);
			List<TransactionInfo> testTransactions = null;
			testTransactionMap.TryGetValue(strategy.Name,out testTransactions);
			Assert.IsNotNull(goodTransactions, "good trades");
			Assert.IsNotNull(testTransactions, "test trades");
			Assert.AreEqual(goodTransactions.Count,testTransactions.Count,"transaction fill count");
		}
		
		public void VerifyBarDataCount(StrategyInterface strategy) {
			List<BarInfo> goodBarData = goodBarDataMap[strategy.Name];
			List<BarInfo> testBarData = testBarDataMap[strategy.Name];
			Assert.AreEqual(goodBarData.Count,testBarData.Count,"bar data count");
		}
		
		public void VerifyTrades(StrategyInterface strategy) {
			try {
				assertFlag = false;
				List<TradeInfo> goodTrades = null;
				goodTradeMap.TryGetValue(strategy.Name,out goodTrades);
				List<TradeInfo> testTrades = null;
				testTradeMap.TryGetValue(strategy.Name,out testTrades);
				Assert.IsNotNull(goodTrades, "good trades");
				Assert.IsNotNull(testTrades, "test trades");
				for( int i=0; i<testTrades.Count && i<goodTrades.Count; i++) {
					TradeInfo testInfo = testTrades[i];
					TradeInfo goodInfo = goodTrades[i];
					TransactionPairBinary goodTrade = goodInfo.Trade;
					TransactionPairBinary testTrade = testInfo.Trade;
					AssertEqual(goodTrade,testTrade,strategy.Name + " Trade at " + i);
					AssertEqual(goodInfo.ProfitLoss,testInfo.ProfitLoss,"ProfitLoss at " + i);
					AssertEqual(goodInfo.ClosedEquity,testInfo.ClosedEquity,"ClosedEquity at " + i);
				}
				Assert.IsFalse(assertFlag,"Checking for trade errors.");
			} catch {
				testFailed = true;
				throw;
			}
		}
		
		public void VerifyTransactions(StrategyInterface strategy) {
			try {
				assertFlag = false;
				List<TransactionInfo> goodTransactions = null;
				goodTransactionMap.TryGetValue(strategy.Name,out goodTransactions);
				List<TransactionInfo> testTransactions = null;
				testTransactionMap.TryGetValue(strategy.Name,out testTransactions);
				Assert.IsNotNull(goodTransactions, "good trades");
				Assert.IsNotNull(testTransactions, "test trades");
				for( int i=0; i<testTransactions.Count && i<goodTransactions.Count; i++) {
					var testInfo = testTransactions[i];
					var goodInfo = goodTransactions[i];
					var goodFill = goodInfo.Fill;
					var testFill = testInfo.Fill;
					AssertReconcile(goodFill,testFill,"Transaction Fill at " + i);
					AssertEqual(goodInfo.Symbol,testInfo.Symbol,"Transaction symbol at " + i);
				}
				Assert.IsFalse(assertFlag,"Checking for transaction fill errors.");
			} catch { 
				testFailed = true;
				throw;
			}
		}
		
		public void PerformReconciliation() {
			try {
				foreach( var kvp in goodReconciliationMap) {
					var symbol = kvp.Key;
					assertFlag = false;
					List<TransactionInfo> goodTransactions = null;
					goodReconciliationMap.TryGetValue(symbol,out goodTransactions);
					List<TransactionInfo> testTransactions = null;
					testReconciliationMap.TryGetValue(symbol,out testTransactions);
					Assert.IsNotNull(goodTransactions, "front-end trades");
					Assert.IsNotNull(testTransactions, "back-end trades");
					for( int i=0; i<testTransactions.Count && i<goodTransactions.Count; i++) {
						var testInfo = testTransactions[i];
						var goodInfo = goodTransactions[i];
						var goodFill = goodInfo.Fill;
						var testFill = testInfo.Fill;
						AssertReconcile(goodFill,testFill,symbol + " transaction Fill at " + i);
						AssertEqual(goodInfo.Symbol,testInfo.Symbol,symbol + " transaction symbol at " + i);
					}
					Assert.IsFalse(assertFlag,"Checking for transaction fill errors.");
				}
			} catch { 
				testFailed = true;
				throw;
			}
		}
		
		public void VerifyStatsCount(StrategyInterface strategy) {
			List<StatsInfo> goodStats = goodStatsMap[strategy.Name];
			List<StatsInfo> testStats = testStatsMap[strategy.Name];
			Assert.AreEqual(goodStats.Count,testStats.Count,"Stats count");
		}
		
		private bool assertFlag = false;
		
		private void AssertEqual(object a, object b, string message) {
			if( a.GetType() != b.GetType()) {
				throw new ApplicationException("Expected type " + a.GetType() + " but was " + b.GetType() + ": " + message);
			}
			if( !a.Equals(b)) {
				assertFlag = true;
				log.Error("Expected '" + a + "' but was '" + b + "': " + message);
			}
		}
		
		private void AssertReconcile(LogicalFillBinary a, LogicalFillBinary b, string message) {
			if( a.GetType() != b.GetType()) {
				throw new ApplicationException("Expected type " + a.GetType() + " but was " + b.GetType() + ": " + message);
			}
			if( a.Position != b.Position || a.Price != b.Price || a.Time != b.Time) {
				assertFlag = true;
				log.Error("Expected '" + a + "' but was '" + b + "': " + message);
			}
		}
		
		public void VerifyStats(StrategyInterface strategy) {
			try {
				assertFlag = false;
				List<StatsInfo> goodStats = goodStatsMap[strategy.Name];
				List<StatsInfo> testStats = testStatsMap[strategy.Name];
				for( int i=0; i<testStats.Count && i<goodStats.Count; i++) {
					StatsInfo testInfo = testStats[i];
					StatsInfo goodInfo = goodStats[i];
					AssertEqual(goodInfo.Time,testInfo.Time,strategy.Name + " - [" + i + "] Stats time at " + testInfo.Time);
					AssertEqual(goodInfo.ClosedEquity,testInfo.ClosedEquity,strategy.Name + " - [" + i + "] Closed Equity time at " + testInfo.Time);
					AssertEqual(goodInfo.OpenEquity,testInfo.OpenEquity,strategy.Name + " - [" + i + "] Open Equity time at " + testInfo.Time);
					AssertEqual(goodInfo.CurrentEquity,testInfo.CurrentEquity,strategy.Name + " - [" + i + "] Current Equity time at " + testInfo.Time);
				}
				Assert.IsFalse(assertFlag,"Checking for stats errors.");
			} catch { 
				testFailed = true;
				throw;
			}
		}
		
		public void VerifyBarData(StrategyInterface strategy) {
			try {
				assertFlag = false;
				List<BarInfo> goodBarData = goodBarDataMap[strategy.Name];
				List<BarInfo> testBarData = testBarDataMap[strategy.Name];
				Assert.IsNotNull(goodBarData, "good bar data");
				Assert.IsNotNull(testBarData, "test test data");
				int i=0;
				for( ; i<testBarData.Count && i<goodBarData.Count; i++) {
					BarInfo testInfo = testBarData[i];
					BarInfo goodInfo = goodBarData[i];
					AssertEqual(goodInfo.Time,testInfo.Time,"Time at bar " + i );
					AssertEqual(goodInfo.Open,testInfo.Open,"Open at bar " + i + " " + testInfo.Time);
					AssertEqual(goodInfo.High,testInfo.High,"High at bar " + i + " " + testInfo.Time);
					AssertEqual(goodInfo.Low,testInfo.Low,"Low at bar " + i + " " + testInfo.Time);
					AssertEqual(goodInfo.Close,testInfo.Close,"Close at bar " + i + " " + testInfo.Time);
				}
				for( ; i<testBarData.Count; i++) {
					BarInfo testInfo = testBarData[i];
					log.Error("Extra test bar: #"+i+" " + testInfo);
				}
				for( ; i<goodBarData.Count; i++) {
					BarInfo goodInfo = goodBarData[i];
					log.Error("Extra good bar: #"+i+" " + goodInfo);
				}
				Assert.IsFalse(assertFlag,"Checking for bar data errors.");
			} catch { 
				testFailed = true;
				throw;
			}
		}
		
		public void VerifyPair(Strategy strategy, int pairNum,
		                       string expectedEntryTime,
		                     double expectedEntryPrice,
		                      string expectedExitTime,
		                     double expectedExitPrice)
		{
			Assert.Greater(strategy.Performance.ComboTrades.Count, pairNum);
    		TransactionPairs pairs = strategy.Performance.ComboTrades;
    		TransactionPair pair = pairs[pairNum];
    		TimeStamp expEntryTime = new TimeStamp(expectedEntryTime);
    		Assert.AreEqual( expEntryTime, pair.EntryTime, "Pair " + pairNum + " Entry");
    		Assert.AreEqual( expectedEntryPrice, pair.EntryPrice, "Pair " + pairNum + " Entry");
    		
    		Assert.AreEqual( new TimeStamp(expectedExitTime), pair.ExitTime, "Pair " + pairNum + " Exit");
    		Assert.AreEqual( expectedExitPrice, pair.ExitPrice, "Pair " + pairNum + " Exit");
    		
    		double direction = pair.Direction;
		}
   		
		public void HistoricalShowChart()
        {
       		log.Debug("HistoricalShowChart() start.");
	       	try {
				for( int i=chartThreads.Count-1; i>=0; i--) {
					chartThreads[i].PortfolioDoc.ShowInvoke();
					if( !ShowCharts) {
						chartThreads[i].PortfolioDoc.HideInvoke();
					}
				}
        	} catch( Exception ex) {
        		log.Debug(ex.ToString());
        	}
        }
		
		public bool TryCloseCharts()
        {
	       	try {
				for( int i=chartThreads.Count-1; i>=0; i--) {
					if( chartThreads[i].IsAlive) {
						return false;
					}
				}
        	} catch( Exception ex) {
        		log.Debug(ex.ToString());
        	}
			HistoricalCloseCharts();
			return true;
        }
		
		public void HistoricalCloseCharts()
        {
	       	try {
				for( int i=chartThreads.Count-1; i>=0; i--) {
					chartThreads[i].Stop();
					chartThreads.RemoveAt(i);
				}
        	} catch( Exception ex) {
        		log.Debug(ex.ToString());
        	}
       		log.Debug("HistoricalShowChart() finished.");
        }
		
   		public TickZoom.Api.Chart HistoricalCreateChart()
        {
 			int oldCount = chartThreads.Count;
        	try {
 				ChartThread chartThread = new ChartThread();
	        	chartThreads.Add( chartThread);
        	} catch( Exception ex) {
        		log.Notice(ex.ToString());
        	}
 			return chartThreads[oldCount].PortfolioDoc.ChartControl;
        }
   		
   		public int ChartCount {
   			get { return chartThreads.Count; }
		}
   		
   		public ChartControl GetChart( string symbol) {
   			ChartControl chart;
   			for( int i=0; i<chartThreads.Count; i++) {
				chart = GetChart(i);
				if( chart.Symbol.Symbol == symbol) {
					return chart;
				}
   			}
   			return null;
   		}

   		public ChartControl GetChart( int num) {
   			return chartThreads[num].PortfolioDoc.ChartControl;
   		}
		
		public string DataFolder {
			get { return dataFolder; }
			set { dataFolder = value; }
		}
		
		public void VerifyChartBarCount(string symbol, int expectedCount) {
			ChartControl chart = GetChart(symbol);
     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
    		Assert.IsNotNull(pane.CurveList);
    		Assert.Greater(pane.CurveList.Count,0);
    		Assert.AreEqual(symbol,chart.Symbol);
    		Assert.AreEqual(expectedCount,chart.StockPointList.Count,"Stock point list");
    		Assert.AreEqual(expectedCount,pane.CurveList[0].Points.Count,"Chart Curve");
		}
   		
		public static void CompareChart(StrategyInterface strategy, ChartControl chart) {
     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
    		Assert.IsNotNull(pane.CurveList);
    		Assert.Greater(pane.CurveList.Count,0);
    		OHLCBarItem chartBars = (OHLCBarItem) pane.CurveList[0];
			Bars strategyBars = strategy.Bars;
			int firstMisMatch = int.MaxValue;
			int i, j;
    		for( i=0; i<strategyBars.Count; i++) {
				j=chartBars.NPts-i-1;
				if( j < 0 || j >= chartBars.NPts) {
					log.Debug("bar " + i + " is missing");
				} else {
	    			StockPt bar = (StockPt) chartBars[j];
	    			string match = "NOT match";
	    			if( strategyBars.Open[i] == bar.Open &&
	    			   strategyBars.High[i] == bar.High &&
	    			   strategyBars.Low[i] == bar.Low &&
	    			   strategyBars.Close[i] == bar.Close) {
	    			    match = "matches";
	    			} else {
	    				if( firstMisMatch == int.MaxValue) {
	    					firstMisMatch = i;
	    				}
	    			}
	    			log.Debug( "bar: " + i + ", point: " + j + " " + match + " days:"+strategyBars.Open[i]+","+strategyBars.High[i]+","+strategyBars.Low[i]+","+strategyBars.Close[i]+" => "+
	    				              bar.Open+","+bar.High+","+bar.Low+","+bar.Close);
	    			log.Debug( "bar: " + i + ", point: " + j + " " + match + " days:"+strategyBars.Time[i]+" "+
	    			              new TimeStamp(bar.X));
				}
    		}
			if( firstMisMatch != int.MaxValue) {
				i = firstMisMatch;
				j=chartBars.NPts-i-1;
    			StockPt bar = (StockPt) chartBars[j];
    			Assert.AreEqual(strategyBars.Open[i],bar.Open,"Open for bar " + i + ", point " + j);
    			Assert.AreEqual(strategyBars.High[i],bar.High,"High for bar " + i + ", point " + j);
    			Assert.AreEqual(strategyBars.Low[i],bar.Low,"Low for bar " + i + ", point " + j);
    			Assert.AreEqual(strategyBars.Close[i],bar.Close,"Close for bar " + i + ", point " + j);
			}
   		}
			
		public void CompareChartCount(Strategy strategy) {
			ChartControl chart = GetChart(strategy.SymbolDefault);
     		GraphPane pane = chart.DataGraph.MasterPane.PaneList[0];
    		Assert.IsNotNull(pane.CurveList);
    		Assert.Greater(pane.CurveList.Count,0);
    		OHLCBarItem bars = (OHLCBarItem) pane.CurveList[0];
			Bars days = strategy.Days;
			Assert.AreEqual(strategy.SymbolDefault,chart.Symbol);
    		Assert.AreEqual(days.BarCount,chart.StockPointList.Count,"Stock point list");
			Assert.AreEqual(days.BarCount,bars.NPts,"Chart Points");
		}
   		
		public string Symbols {
			get { return symbols; }
			set { symbols = value; }
		}
		
		public CreateStarterCallback CreateStarterCallback {
			get { return createStarterCallback; }
			set { createStarterCallback = value; }
		}
	}
}