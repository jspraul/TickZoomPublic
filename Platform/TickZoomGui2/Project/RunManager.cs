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
using System.Configuration;
using System.Threading;
using System.Windows.Forms;

using TickZoom.Api;
using TickZoom.Starters;
using TickZoom.TickUtil;

//using TickZoom.Provider;


//using TickZoom.Engine;

namespace TickZoom
{
    
    public partial class RunManager
    {
    	ProjectDoc projectDoc;
        // The progress of the task in percentage
        private static int PercentProgress;
        // The delegate which we will call from the thread to update the form
        // When to pause
	    public delegate void UpdateProgressDelegate(string fileName, Int64 BytesRead, Int64 TotalBytes);
	    public delegate void ShowChartDelegate();
// 		ProviderProxy proxy = null;
//		ProviderStub2 stub = null;
		private static readonly Log log = Factory.Log.GetLogger(typeof(RunManager));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
        BackgroundWorker processWorker;
   		
        public RunManager(ProjectDoc projectDoc)
        {
            this.projectDoc = projectDoc;
            
        	processWorker = new System.ComponentModel.BackgroundWorker();
        	processWorker.WorkerReportsProgress = true;
        	processWorker.WorkerSupportsCancellation = true;
        	processWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ProcessWorkerDoWork);
        	processWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ProcessWorkerRunWorkerCompleted);
        	processWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ProcessWorkerProgressChanged);
        }
        
//        public void HandleModelList() {
//			Plugins plugins = Plugins.Instance;
//			
//			List<string> modelList = new List<string>();
//			for( int i=0; i<plugins.Models.Count; i++) {
//				Model model = plugins.Models[i];
//				if( model is Strategy) {
//					modelList.Add(model.Name);
//				}
//			}
//			modelBox.Items.AddRange(modelList.ToArray());
//			value = Factory.Settings["Model"];
//			modelBox.SelectedItem = value;
//        }

        private void btnOptimize_Click(object sender, EventArgs e)
        {
 			RunWork(0);
        }

 		public void SimulationMenuItemClick(object sender, System.EventArgs e)
		{
 			RunWork(1);
        }
 		
        void RealTimeButtonClick(object sender, System.EventArgs e)
        {
 			RunWork(2);
        }
        
 		private void RunWork(int task) {
            if (processWorker.IsBusy)
            {
                MessageBox.Show("A task is already running. Please either the stop the current task or await for its completion before starting a new one.", "Test in progress", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
    			Save();
    			projectDoc.ClearChart();
            	processWorker.RunWorkerAsync(task);
            }
 		}
        
        private void UpdateProgressBar(int value, string text) {
        	projectDoc.MainForm.UpdateProgressBar(value,text);
        }
        
        public void Optimize(BackgroundWorker bw)
        {
        	Starter starter = new OptimizeStarter();
//			starter.StartTime = (TimeStamp) startTime;
//    		starter.EndTime = (TimeStamp) endTime;
//    		starter.BackgroundWorker = bw;
//    		starter.ShowChartCallback = new ShowChartCallback(ShowChartInvoke);
//	    	portfolioDoc.ChartControl.ChartType = chartType;
//    		starter.DataFolder = "DataCache";
//    		starter.Symbol = txtSymbol.Text;
//    		if( modelLoaderBox.Enabled) {
//    			starter.ModelLoader = Plugins.Instance.GetLoader(modelLoaderText);
//    		} else {
//    			starter.Model = Plugins.Instance.GetModel(modelText);
//    		}
//    		starter.Run();
        }
        
        public void GeneticOptimize(BackgroundWorker bw)
        {
        	Starter starter = new GeneticStarter();
//			starter.StartTime = (TimeStamp) startTime;
//    		starter.EndTime = (TimeStamp) endTime;
//    		starter.BackgroundWorker = bw;
//    		starter.ShowChartCallback = new ShowChartCallback(ShowChartInvoke);
//    		starter.DataFolder = "DataCache";
//    		starter.Symbol = txtSymbol.Text;
//    		if( modelLoaderBox.Enabled) {
//    			starter.ModelLoader = Plugins.Instance.GetLoader(modelLoaderText);
//    		} else {
//    			starter.Model = Plugins.Instance.GetModel(modelText);
//    		}
//    		starter.Run();
        }
        
        public void StartHistorical(BackgroundWorker bw)
        {
        	projectDoc.PortfolioControl.WriteProject();
    		Starter starter = new HistoricalStarter();
    		string appData = Factory.Settings["AppDataFolder"];
    		starter.ProjectFile = appData + @"\portfolio.tzproj";
    		starter.BackgroundWorker = bw;
    		starter.ShowChartCallback = new ShowChartCallback(projectDoc.DisplayChart);
    		starter.CreateChartCallback = new CreateChartCallback(projectDoc.CreateChart);
    		starter.Run();
        }
        
        private void btnStop_Click(object sender, EventArgs e)
        {
        	processWorker.CancelAsync();
        	Factory.Engine.Dispose();
        	Factory.TickUtil.TickReader().CloseAll();
            // Close the web response and the streams
            // Set the progress bar back to 0 and the label
            UpdateProgressBar( 0, "Execute Stopped");
            // Disable the Pause/Resume button because the task has ended
        }

        public void Save() {
			// Get the configuration file.
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			
			// Add an entry to appSettings.
			int appStgCnt = ConfigurationManager.AppSettings.Count;
			
//			config.AppSettings.Settings.Clear();
			
//			SaveIntervals(config);
			
			// Save the configuration file.
			config.Save(ConfigurationSaveMode.Modified);
			
			// Force a reload of the changed section.
			ConfigurationManager.RefreshSection("appSettings");
        }
        
        private void SaveSetting(Configuration config, string key, string value) {
        	config.AppSettings.Settings.Remove(key);
			config.AppSettings.Settings.Add(key,value);
        }
        
        void ProcessWorkerDoWork(object sender, DoWorkEventArgs e)
        {
        	BackgroundWorker bw = sender as BackgroundWorker;
            int arg = (int)e.Argument;
            if( trace) log.Trace("Started thread to do work.");
            if( Thread.CurrentThread.Name == null) {
            	Thread.CurrentThread.Name = "ProcessWorker";
            }
            switch( arg) {
            	case 0:
            		Optimize(bw);
            		break;
            	case 1:
            		StartHistorical(bw);
            		break;
            	case 2:
            		OrderServer(bw);
            		break;
            	case 3:
            		GeneticOptimize(bw);
            		break;
            }
        }
        
		public void OrderServer(BackgroundWorker bw)
		{
    		Starter starter = new RealTimeStarter();
//    		proxy = new TickZoomProxy();
//    		proxy.BackgroundWorker = bw;
//    		
//			// Setup the chart
//			starter.ShowChartCallback = new ShowChartCallback(ShowChartInvoke);
//			portfolioDoc.ChartControl.ChartType = chartType;
//			starter.Chart = portfolioDoc.ChartControl;
//			starter.DataFeed = proxy;
//			
////    		starter.StartTime = (TimeStamp) new DateTime(2004,1,1);
////    		starter.EndTime = (TimeStamp) new DateTime(2004,2,1);			
//    		starter.BackgroundWorker = bw;
//        	starter.Symbol = txtSymbol.Text;
//        	starter.Broker = proxy;
//    		if( modelLoaderBox.Enabled) {
//    			starter.ModelLoader = Plugins.Instance.GetLoader(modelLoaderText);
//    		} else {
//    			starter.Model = Plugins.Instance.GetModel(modelText);
//    		}
//			starter.IntervalDefault = intervalDefault;
//			if( defaultOnly.Checked) {
//				portfolioDoc.ChartControl.IntervalChartDisplay = intervalDefault;
//				portfolioDoc.ChartControl.IntervalChartBar = intervalDefault;
//				portfolioDoc.ChartControl.IntervalChartUpdate = intervalDefault;
//			} else {
//				portfolioDoc.ChartControl.IntervalChartDisplay = intervalChartDisplay;
//				portfolioDoc.ChartControl.IntervalChartBar = intervalChartBar;
//				portfolioDoc.ChartControl.IntervalChartUpdate = intervalChartUpdate;
//			}
//    		starter.Run();
//    		
//    		proxy.Connect();
//    		
//            while(!bw.CancellationPending) {
//            	Application.DoEvents();
//            	Thread.Sleep(5);
//    		}
		}
		
		void ProcessWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	Progress progress = (Progress) e.UserState;
            // Calculate the task progress in percentages
            PercentProgress = Convert.ToInt32((progress.Current * 100) / progress.Final);
            // Make progress on the progress bar
            UpdateProgressBar( Math.Min(100,PercentProgress), progress.Text + ": " + progress.Current + " out of " + progress.Final + " (" + PercentProgress + "%)" );
        }
        
        void ProcessWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			if(e.Error !=null)
			{ // analyze error here
				if( e.Error is MustUseLoaderException) {
					MessageBox.Show(e.Error.Message);
				} else {
					log.Notice(e.Error.ToString());
					MessageBox.Show(e.Error.ToString());
				}
			}
        }
        
		string CheckNull(string str) {
			if( str == null) { 
				return "";
			} else {
				return str;
			}
		}
        
        public void Close() {
            processWorker.CancelAsync();
            Factory.TickUtil.TickReader().CloseAll();
        }
    }
}
