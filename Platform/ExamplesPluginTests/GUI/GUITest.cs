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
using TickZoom.Presentation;

namespace Other
{
	[TestFixture]
	public class GUITest
	{
		private static Log log = Factory.SysLog.GetLogger(typeof(GUITest));
		private static bool debug = log.IsDebugEnabled;
		private ushort servicePort = 6490;
		private StarterConfig config;
		[SetUp]
		public void Setup() {
			DeleteFiles();
			Process[] processes = Process.GetProcessesByName("TickZoomCombinedMock");
    		foreach( Process proc in processes) {
    			proc.Kill();
    		}
		}
		
		private StarterConfig CreateConfig() {
			string storageFolder = Factory.Settings["AppDataFolder"];
			string workspaceFolder = Path.Combine(storageFolder,"Workspace");
			string projectFile = Path.Combine(workspaceFolder,"test.tzproj");
			ConfigFile projectConfig = new ConfigFile(projectFile,StarterConfig.GetDefaultConfig());
			projectConfig.SetValue("ProviderAssembly","TickZoomCombinedMock");
			projectConfig.SetValue("ServicePort",servicePort.ToString());
			projectConfig.SetValue("ServiceConfig","WarehouseTest.config");
			SetupWarehouseConfig();
			config = new StarterConfig("test");
			
			WaitForEngine(config);
			return config;
		}

		private void WaitComplete(int seconds) {
			WaitComplete(seconds,null);
		}
		
		private void WaitComplete(int seconds, Func<bool> onCompleteCallback) {
			long end = Factory.TickCount + (seconds * 1000);
			while( Factory.TickCount < end) {
				config.Catch();
				if( onCompleteCallback != null && onCompleteCallback()) {
					return;
				}
				Thread.Sleep(1);
			}
//			throw new ApplicationException(seconds + " seconds timeout expired waiting on application to complete.");
		}
		
		private void Pause(int seconds) {
			long end = Factory.TickCount + (seconds * 1000);
			long current;
			while( (current = Factory.TickCount) < end) {
				Application.DoEvents();
				config.Catch();
				Thread.Sleep(1);
			}
		}
#if TESTSTARTRUN
		[Test]
		public void TestStartRun()
		{
			using( config = CreateConfig()) {
				config.SymbolList.Text = "USD/JPY";
				config.DefaultBox.Text = "1";
				config.DefaultCombo.Text = "Hour";
				for( int i=0; i<3; i++) {
					log.Notice("Processing #" + (i+1));
					config.HistoricalButtonClick(null,null);
					WaitComplete(120, () => { return !config.ProcessWorker.IsBusy && config.PortfolioDocs[i].Visible; } );
					Assert.AreEqual(config.PortfolioDocs.Count,i+1,"Charts");
					Assert.IsFalse(config.ProcessWorker.IsBusy,"ProcessWorker.Busy");
					Assert.IsTrue(config.PortfolioDocs[i].Visible,"Chart visible failed at " + i);
				}
			}
		}
#endif
		public void WaitForEngine(StarterConfig config) {
			while( !config.IsEngineLoaded) {
				Thread.Sleep(1);
				Application.DoEvents();
			}
		}
		
		[Test]
		public void TestRealTimeNoHistorical()
		{
			using( config = CreateConfig()) {
				config.SymbolList = "IBM,GBP/USD";
				config.DefaultPeriod = 10;
				config.DefaultBarUnit = BarUnit.Tick;
				config.Starter = "RealTimeStarter";
				config.Start();
				WaitComplete(10);
				config.Stop();
				WaitComplete(120, () => { return !config.CommandWorker.IsBusy; } );
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
		
		public void SetupWarehouseConfig()
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
				warehouseConfig.SetValue("ProviderAssembly","TickZoomCombinedMock");
	 			// Clear the history files
			} catch( Exception ex) {
				log.Error("Setup error.",ex);
				throw ex;
			}
		}
		
		[Test]
		public void TestCapturedDataMatchesProvider()
		{
			using( config = CreateConfig()) {
				config.SymbolList = "/ESZ9";
				config.DefaultPeriod = 1;
				config.DefaultBarUnit = BarUnit.Minute;
				config.EndDateTime = DateTime.UtcNow;
				config.Starter = "RealTimeStarter";
				config.Start();
				WaitComplete(10);
				config.Stop();
				WaitComplete(120, () => { return !config.CommandWorker.IsBusy; } );
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
		}
	}
}
