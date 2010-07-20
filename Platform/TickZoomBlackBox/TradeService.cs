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
using System.ServiceProcess;
using System.Threading;

using TickZoom.Api;

//using TickZoom.Provider;


namespace TickZoom.BlackBox
{
	public class TradeService : ServiceBase
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public const string MyServiceName = "TradeService";
       	BackgroundWorker processWorker = new BackgroundWorker();
        
		public TradeService()
		{
			InitializeComponent();
		}
		
		private void InitializeComponent()
		{
			this.ServiceName = MyServiceName;
			log.FileName = @"TradeService.log";
        	this.processWorker.WorkerSupportsCancellation = true;
        	this.processWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ProcessWorkerRunWorkerCompleted);
        	this.processWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ProcessWorkerDoWork);
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
            if( processWorker != null) { processWorker.CancelAsync(); }
		}
		
		/// <summary>
		/// Start this service.
		/// </summary>
		protected override void OnStart(string[] args)
		{
           	processWorker.RunWorkerAsync(2);
		}
		
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
            if( processWorker != null) { processWorker.CancelAsync(); }
		}
		
        void ProcessWorkerDoWork(object sender, DoWorkEventArgs e)
        {
        	BackgroundWorker bw = sender as BackgroundWorker;
            int arg = (int)e.Argument;
        	log.Notice("Started thread to do work.");
            
       		OrderServer(bw);
        }
        
		public void OrderServer(BackgroundWorker bw)
		{
#if REALTIME
        	Starter starter = new RealTimeStarter();
    		starter.ProjectProperties.Starter.StartTime = (TimeStamp) startTime;
    		starter.BackgroundWorker = bw;
// Leave off charting unless you will write chart data to a database or Web Service.
//    		starter.ShowChartCallback = new ShowChartCallback(ShowChartInvoke);
//    		starter.CreateChartCallback = new CreateChartCallback(CreateChartInvoke);
//	    	starter.ProjectProperties.Chart.ChartType = chartType;
	    	starter.ProjectProperties.Starter.Symbols = txtSymbol.Text;
			starter.ProjectProperties.Starter.IntervalDefault = intervalDefault;
			if( defaultOnly.Checked) {
				starter.ProjectProperties.Chart.IntervalChartDisplay = intervalDefault;
				starter.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
				starter.ProjectProperties.Chart.IntervalChartUpdate = intervalDefault;
			} else {
				starter.ProjectProperties.Chart.IntervalChartDisplay = intervalChartDisplay;
				starter.ProjectProperties.Chart.IntervalChartBar = intervalChartBar;
				starter.ProjectProperties.Chart.IntervalChartUpdate = intervalChartUpdate;
			}
			if( intervalChartDisplay.BarUnit == BarUnit.Default) {
				starter.ProjectProperties.Chart.IntervalChartDisplay = intervalDefault;
			}
			if( intervalChartBar.BarUnit == BarUnit.Default) {
				starter.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
			}
			if( intervalChartUpdate.BarUnit == BarUnit.Default) {
				starter.ProjectProperties.Chart.IntervalChartUpdate = intervalDefault;
			}
			starter.Run(Plugins.Instance.GetLoader(modelLoaderText));
#endif
        }
        
        void ProcessWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			if(e.Error !=null)
			{ // analyze error here
				log.Notice(e.Error.ToString());
				Thread.Sleep(5000);
				log.Notice("Restarting Order Service.");
			}
        }
	}
}
