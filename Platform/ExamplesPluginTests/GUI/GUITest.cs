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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;
using TickZoom;
using TickZoom.Api;
using TickZoom.TickUtil;

namespace MiscTest
{
	[TestFixture]
	public class GUITest
	{
		private static Log log = Factory.Log.GetLogger(typeof(GUITest));
		private static bool debug = log.IsDebugEnabled;
		private Form1 form;
		[SetUp]
		public void Setup() {
			DeleteFiles();
			Process[] processes = Process.GetProcessesByName("TickZoomProviderMock");
    		foreach( Process proc in processes) {
    			proc.Kill();
    		}
		}
		
		private Form1 CreateForm() {
			Form1 form = new Form1();
			form.Show();
			WaitForEngine(form);
			return form;
		}

		private void WaitComplete(int seconds) {
			WaitComplete(seconds,null);
		}
		
		private void WaitComplete(int seconds, Func<bool> onCompleteCallback) {
			long end = Factory.TickCount + (seconds * 1000);
			long current;
			while( (current = Factory.TickCount) < end) {
				Application.DoEvents();
				form.Catch();
				if( onCompleteCallback != null && onCompleteCallback()) {
					break;
				}
				Thread.Sleep(1);
			}
		}
		
		[Test]
		public void TestStartRun()
		{
			using( form = CreateForm()) {
				form.TxtSymbol.Text = "USD/JPY";
				form.DefaultBox.Text = "1";
				form.DefaultCombo.Text = "Hour";
				for( int i=0; i<10; i++) {
					log.Notice("Processing #" + (i+1));
					form.HistoricalButtonClick(null,null);
					WaitComplete(120, () => { return !form.ProcessWorker.IsBusy && form.PortfolioDocs[i].Visible; } );
					Assert.AreEqual(form.PortfolioDocs.Count,i+1,"Charts");
					Assert.IsTrue(form.PortfolioDocs[i].Visible,"Chart visible failed at " + i);
					Assert.IsFalse(form.ProcessWorker.IsBusy,"ProcessWorker.Busy");
				}
			}
		}
		
		public void WaitForEngine(Form1 form) {
			while( !form.IsEngineLoaded) {
				Thread.Sleep(1);
				Application.DoEvents();
			}
		}
		
		[Test]
		public void TestRealTimeNoHistorical()
		{
			using( form = CreateForm()) {
				form.TxtSymbol.Text = "IBM,GBP/USD";
				form.DefaultBox.Text = "10";
				form.DefaultCombo.Text = "Tick";
				form.RealTimeButtonClick(null,null);
				WaitComplete(30000, () => { return form.PortfolioDocs.Count == 2 &&
				             		form.PortfolioDocs[0].Visible &&
				             		form.PortfolioDocs[1].Visible; } );
				Assert.AreEqual(2,form.PortfolioDocs.Count,"Charts");
				Assert.IsTrue(form.PortfolioDocs[0].Visible &&
				             		form.PortfolioDocs[1].Visible,"Charts Visible");
				form.btnStop_Click(null,null);
				WaitComplete(10000, () => { return !form.ProcessWorker.IsBusy; } );
				Assert.IsFalse(form.ProcessWorker.IsBusy,"ProcessWorker.Busy");
			}
		}
		
		private void DeleteFiles() {
			int count = 0;
			while( true) {
				try {
					string appData = Factory.Settings["AppDataFolder"];
		 			File.Delete( appData + @"\TestServerCache\ESZ9.tck");
		 			File.Delete( appData + @"\TestServerCache\IBM.tck");
		 			File.Delete( appData + @"\TestServerCache\GBPUSD.tck");
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
			using( form = CreateForm()) {
				form.TxtSymbol.Text = "/ESZ9";
				form.DefaultBox.Text = "1";
				form.DefaultCombo.Text = "Minute";
				form.EndTime = DateTime.Now;
				form.RealTimeButtonClick(null,null);
				WaitComplete(20, () => { return form.PortfolioDocs.Count == 1 &&
				             		form.PortfolioDocs[0].Visible; } );
				Assert.AreEqual(1,form.PortfolioDocs.Count,"Charts");
				WaitComplete(20, () => { return false; } );
				form.btnStop_Click(null,null);
				WaitComplete(20, () => { return !form.ProcessWorker.IsBusy; } );
				Assert.IsFalse(form.ProcessWorker.IsBusy,"ProcessWorker.Busy");
				Assert.Greater(form.LogOutput.Lines.Length,2,"number of log lines");
				string appData = Factory.Settings["AppDataFolder"];
				string compareFile1 = appData + @"\MockProviderData\ESZ9.tck";
				string compareFile2 = appData + @"\TestServerCache\ESZ9.tck";
				using ( TickReader reader1 = new TickReader()) {
					reader1.Initialize(compareFile1,form.TxtSymbol.Text);
					TickBinary tick1 = new TickBinary();
					try {
						int count = 0;
						while(true) {
							while(!reader1.ReadQueue.TryDequeue(ref tick1)) { Thread.Sleep(1); }
							TimeStamp ts1 = new TimeStamp(tick1.UtcTime);
							log.Info("Tick# " + count + ". " + ts1);
							count++;
						}
					} catch( QueueException ex) {
						Assert.AreEqual(ex.EntryType,EventType.EndHistorical);
					}
				}
				using ( TickReader reader1 = new TickReader())
				using ( TickReader reader2 = new TickReader()) {
					reader1.Initialize(compareFile1,form.TxtSymbol.Text);
					reader2.Initialize(compareFile2,form.TxtSymbol.Text);
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
