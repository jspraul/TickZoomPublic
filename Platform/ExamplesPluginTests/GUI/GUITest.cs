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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.GUI;
using TickZoom.Presentation;
using ZedGraph;

namespace Other
{
	public static class StarterConfigTestExtensions {
		public static void WaitComplete(this StarterConfig config, int seconds) {
			config.WaitComplete(seconds,null);
		}
		
		public static void WaitComplete(this StarterConfig config, int seconds, Func<bool> onCompleteCallback) {
			long end = Factory.TickCount + (seconds * 1000);
			while( Factory.TickCount < end) {
				config.Catch();
				if( onCompleteCallback != null && onCompleteCallback()) {
					return;
				}
				Thread.Sleep(1);
			}
		}
		
		public static void Pause(this StarterConfig config, int seconds) {
			long end = Factory.TickCount + (seconds * 1000);
			long current;
			while( (current = Factory.TickCount) < end) {
				Application.DoEvents();
				config.Catch();
				Thread.Sleep(1);
			}
		}		
	}
		
	[TestFixture]
	public class GUITest
	{
		private static Log log = Factory.SysLog.GetLogger(typeof(GUITest));
		private bool debug = log.IsDebugEnabled;
		private ushort servicePort = 6490;
		private Thread guiThread;
		private Execute execute;
		[SetUp]
		public void Setup() {
			DeleteFiles();
			Process[] processes = Process.GetProcessesByName("TickZoomCombinedMock");
    		foreach( Process proc in processes) {
    			proc.Kill();
    		}
		}
		
		private StarterConfig CreateSimulateConfig() {
			var config = new StarterConfig("test");
			config.ServiceConfig = "WarehouseTest.config";
			config.ServicePort = servicePort;
			config.ProviderAssembly = "TickZoomCombinedMock";
			
			WaitForEngine(config);
			return config;
		}

		public void WaitForEngine(StarterConfig config) {
			while( !config.IsEngineLoaded) {
				Thread.Sleep(1);
				Application.DoEvents();
			}
		}
		
		[Test]
		public void TestConfigRealTimeNoHistorical()
		{
			using( var config = CreateSimulateConfig()) {
				config.SymbolList = "IBM,GBP/USD";
				config.DefaultPeriod = 10;
				config.DefaultBarUnit = BarUnit.Tick.ToString();
				config.ModelLoader = "Example: Reversal Multi-Symbol";
				config.StarterName = "TestRealTimeStarter";
				config.Start();
				config.WaitComplete(120, () => { return config.CommandWorker.IsBusy; } );
				config.Stop();
				config.WaitComplete(120, () => { return !config.CommandWorker.IsBusy; } );
				Assert.IsFalse(config.CommandWorker.IsBusy,"ProcessWorker.Busy");
			}
		}
		
		[Test]
		public void TestGUIRealTimeNoHistorical()
		{
			using( var config = CreateSimulateConfig()) {
				try {
		            StarterConfigView form;
		            StartGUI(config, out form);
					config.SymbolList = "IBM,GBP/USD";
					config.DefaultPeriod = 10;
					config.DefaultBarUnit = BarUnit.Tick.ToString();
					config.ModelLoader = "Example: Reversal Multi-Symbol";
					config.StarterName = "TestRealTimeStarter";
					config.Start();
					config.WaitComplete(120, () => { return config.CommandWorker.IsBusy; } );
					config.Stop();
					config.WaitComplete(120, () => { return !config.CommandWorker.IsBusy; } );
					Assert.IsFalse(config.CommandWorker.IsBusy,"ProcessWorker.Busy");
				} finally {
					execute.Exit();
					guiThread.Join();
				}
			}
		}
		
		[Test]
		public void TestGUIRealTimeDemo()
		{
			Assert.Ignore();
			while( true) {
				TestGUIIteration();
			}
		}

		private void StartGUI( StarterConfig config, out StarterConfigView outForm) {
			WaitForEngine(config);
			var isRunning = false;
			StarterConfigView form = null;
			guiThread = new Thread( () => {
			    Thread.CurrentThread.Name = "GUIThread";
			    execute = Execute.Create();
	            form = new StarterConfigView(execute,config);
	            AutoModelBinder.Bind( config, form, execute);
	            Application.Idle += execute.MessageLoop;
	            form.Visible = false;
	            isRunning = true;
	            Application.Run();
			});
			guiThread.Start();
			config.WaitComplete(30, () => { return isRunning; } );
			outForm = form;
		}
		
		public void TestGUIIteration() {
			var appData = Factory.Settings["PriceDataFolder"];
 			File.Delete( appData + @"\ServerCache\IBM.tck");
			using( var config = new StarterConfig()) {
				StarterConfigView form = null;
 				StartGUI(config, out form);
				try {
					config.WaitComplete(2);
					config.SymbolList = "IBM";
					config.DefaultPeriod = 10;
					config.DefaultBarUnit = BarUnit.Second.ToString();
					config.ModelLoader = "Example: Breakout Reversal";
					config.StarterName = "Realtime Operation (Demo or Live)";
					config.Start();
					config.WaitComplete(30, () => { return form.PortfolioDocs.Count > 0; } );
					Assert.Greater(form.PortfolioDocs.Count,0);
					var chart = form.PortfolioDocs[0].ChartControl;
					config.WaitComplete(30, () => { return chart.IsDrawn; } );
		     		var pane = chart.DataGraph.MasterPane.PaneList[0];
		    		Assert.IsNotNull(pane.CurveList);
					config.WaitComplete(30, () => { return pane.CurveList.Count > 0; } );
					Assert.Greater(pane.CurveList.Count,0);
		    		var chartBars = (OHLCBarItem) pane.CurveList[0];
					config.WaitComplete(60, () => { return chartBars.NPts >= 3; } );
		    		Assert.GreaterOrEqual(chartBars.NPts,3);
					config.Stop();
					config.WaitComplete(30, () => { return !config.CommandWorker.IsBusy; } );
					Assert.IsFalse(config.CommandWorker.IsBusy,"ProcessWorker.Busy");
				} finally {
					execute.Exit();
					guiThread.Join();
				}
			}
		}
		
		public void TestRealTimeNoHistorical()
		{
			using( var config = CreateSimulateConfig()) {
				config.SymbolList = "IBM,GBP/USD";
				config.DefaultPeriod = 10;
				config.DefaultBarUnit = BarUnit.Tick.ToString();
				config.ModelLoader = "Example: Reversal Multi-Symbol";
				config.StarterName = "TestRealTimeStarter";
				config.Start();
				config.WaitComplete(10);
				config.Stop();
				config.WaitComplete(120, () => { return !config.CommandWorker.IsBusy; } );
				Assert.IsFalse(config.CommandWorker.IsBusy,"ProcessWorker.Busy");
			}
		}
		
		private void DeleteFiles() {
			int count = 0;
			while( true) {
				try {
					string appData = Factory.Settings["AppDataFolder"];
		 			File.Delete( appData + @"\Test\\ServerCache\ESZ9.tck");
		 			File.Delete( appData + @"\Test\\ServerCache\IBM.tck");
		 			File.Delete( appData + @"\Test\\ServerCache\GBPUSD.tck");
		 			Directory.CreateDirectory(appData + @"\Workspace\");
		 			File.Delete( appData + @"\Workspace\test.config");
					return;
				} catch( Exception) {
					count ++;
					if( count > 100) {
						throw;
					} else {
						Thread.Sleep(1);
					}
				}
			}
		}
		
		[Test]
		public void TestCapturedDataMatchesProvider()
		{
			try {
				using( var config = CreateSimulateConfig()) {
					config.SymbolList = "/ESZ9";
					config.DefaultPeriod = 1;
					config.DefaultBarUnit = BarUnit.Minute.ToString();
					config.EndDateTime = DateTime.UtcNow;
					config.ModelLoader = "Example: Reversal Multi-Symbol";
					config.StarterName = "TestRealTimeStarter";
					config.Start();
					config.WaitComplete(10);
					config.Stop();
					config.WaitComplete(120, () => { return !config.CommandWorker.IsBusy; } );
					Assert.IsFalse(config.CommandWorker.IsBusy,"ProcessWorker.Busy");
					string appData = Factory.Settings["AppDataFolder"];
					string compareFile1 = appData + @"\Test\MockProviderData\ESZ9.tck";
					string compareFile2 = appData + @"\Test\ServerCache\ESZ9.tck";
					using ( TickReader reader1 = Factory.TickUtil.TickReader()) {
						reader1.Initialize(compareFile1,config.SymbolList);
						TickBinary tick1 = new TickBinary();
						try {
							int count = 0;
							while(true) {
								while(!reader1.ReadQueue.TryDequeue(ref tick1)) { Thread.Sleep(1); }
								TimeStamp ts1 = new TimeStamp(tick1.UtcTime);
								count++;
							}
						} catch( QueueException ex) {
							Assert.AreEqual(ex.EntryType,EventType.EndHistorical);
						}
					}
					using ( TickReader reader1 = Factory.TickUtil.TickReader())
					using ( TickReader reader2 = Factory.TickUtil.TickReader()) {
						reader1.Initialize(compareFile1,config.SymbolList);
						reader2.Initialize(compareFile2,config.SymbolList);
						TickBinary tick1 = new TickBinary();
						TickBinary tick2 = new TickBinary();
						bool result = true;
						try {
							int count = 0;
							while(true) {
								while(!reader1.ReadQueue.TryDequeue(ref tick1)) { Thread.Sleep(1); }
								while(!reader2.ReadQueue.TryDequeue(ref tick2)) { Thread.Sleep(1); }
								TimeStamp ts1 = new TimeStamp(tick1.UtcTime);
								TimeStamp ts2 = new TimeStamp(tick2.UtcTime);
								if( !ts1.Equals(ts2)) {
									result = false;
									log.Error("Tick# " + count + " failed. Expected: " + ts1 + ", But was:" + ts2);
								}
								count++;
							}
						} catch( QueueException ex) {
							Assert.AreEqual(ex.EntryType,EventType.EndHistorical);
						}
						Assert.IsTrue(result,"Tick mismatch errors. See log file.");
					}
				}
			} catch( Exception ex) {
				log.Error("Test failed with error: " + ex.Message, ex);
				Environment.Exit(1);
			}
		}
	}
}
