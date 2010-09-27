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
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

using TickZoom.Api;

namespace TickZoom
{
    public partial class Form1 : Form
    {
        // The progress of the task in percentage
        private static int PercentProgress;
        private SynchronizationContext context;
        // The delegate which we will call from the thread to update the form
        // When to pause
//        bool goPause = false;
	    public delegate void UpdateProgressDelegate(string fileName, Int64 BytesRead, Int64 TotalBytes);
	    public delegate Chart CreateChartDelegate();
   		DateTime startTime = new DateTime(2003,1,1); 
   		DateTime endTime = DateTime.Now;
		Dictionary<int,Progress> progressChildren = new Dictionary<int,Progress>();
		bool enableAlarmSounds = false;
		Log log;
		
        Interval initialInterval;
        Interval intervalDefault;
		Interval intervalEngine;
		Interval intervalChartBar;
		string tickZoomEngine;
        bool isInitialized = false;
		
		public DateTime EndTime {
			get { return endTime; }
			set { endTime = value; }
		}
        Thread thread_ProcessMessages;
   		ConfigFile projectConfig;
   		
   		public Form1() : this("project") {
   			
   		}
   		
        public Form1(string projectName)
        {
        	log = Factory.SysLog.GetLogger(typeof(Form1));
        	string storageFolder = Factory.Settings["AppDataFolder"];
        	string workspace = Path.Combine(storageFolder,"Workspace");
        	string projectFile = Path.Combine(workspace,projectName+".tzproj");
        	projectConfig = new ConfigFile( projectFile, DefaultConfig);
			context = SynchronizationContext.Current;
            if(context == null)
            {
                context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
            }
	        intervalDefault = initialInterval;
			intervalEngine = initialInterval;
			intervalChartBar = initialInterval;
            InitializeComponent();
            defaultCombo.DataSource = Enum.GetValues(typeof(BarUnit));
            engineBarsCombo.DataSource = Enum.GetValues(typeof(BarUnit));
            chartBarsCombo.DataSource = Enum.GetValues(typeof(BarUnit));
            IntervalDefaults();
            LoadIntervals();
            IntervalDefaults();
			storageFolder = Factory.Settings["AppDataFolder"];
			tickZoomEngine = projectConfig.GetValue("TickZoomEngine");
			txtSymbol.Text = projectConfig.GetValue("Symbol");
            Array units = Enum.GetValues(typeof(BarUnit));
            DateTime availableDate = new DateTime(1800,1,1);
            startTimePicker.Value = startTimePicker.MinDate = availableDate;
            startTimePicker.MaxDate = DateTime.Now;
            
            endTimePicker.MinDate = availableDate;
            endTimePicker.Value = endTimePicker.MaxDate = DateTime.Now.AddDays(1).AddSeconds(-1);
            string disableCharting = projectConfig.GetValue("DisableCharting");
            if( !string.IsNullOrEmpty(disableCharting) && "true".Equals(disableCharting.ToLower())) {
            	disableChartsCheckBox.Checked = true;
            } else {
            	disableChartsCheckBox.Checked = false;
            }
            string startTimeStr = projectConfig.GetValue("StartTime");
            string EndTimeStr = projectConfig.GetValue("EndTime");
       		if( startTimeStr != null) {
       			startTime = DateTime.Parse(startTimeStr);
       			startTimePicker.Value = startTime;
       		} else {
       			startTime = availableDate;
       		}
       		if( EndTimeStr != null) {
       			EndTime = DateTime.Parse(EndTimeStr).AddDays(1).AddSeconds(-1);
       			if( EndTime > endTimePicker.MaxDate) {
       				endTimePicker.Value = endTimePicker.MaxDate;
       			} else {
       				endTimePicker.Value = EndTime;
       			}
       		} else {
       			EndTime = DateTime.Now;
       		}
            thread_ProcessMessages = new Thread(new ThreadStart(ProcessMessages));
            thread_ProcessMessages.Name = "ProcessMessages";
            thread_ProcessMessages.Priority = ThreadPriority.Lowest;
            thread_ProcessMessages.IsBackground = true;
            thread_ProcessMessages.Start();
            UpdateCheckBoxes();
            isInitialized = true;
        }
        
        public void HandlePlugins() {
    		modelLoaderBox.Enabled = true;
        	
			Plugins plugins = Plugins.Instance;
			List<ModelLoaderInterface> loaders = plugins.GetLoaders();
			List<string> modelLoaderList = new List<string>();
			for( int i=0; i<loaders.Count; i++) {
				if( loaders[i].IsVisibleInGUI) {
					modelLoaderList.Add(loaders[i].Name);
				}
			}
			modelLoaderBox.Items.AddRange(modelLoaderList.ToArray());
			string value = projectConfig.GetValue("ModelLoader");
			modelLoaderBox.SelectedItem = value;
        }

		private void TryUpdate(BackgroundWorker bw) {
			if( Factory.AutoUpdate(bw)) {
        		projectConfig.SetValue("AutoUpdate","false");
				SaveAutoUpdate();
				log.Notice("AutoUpdate succesful. Restart unnecessary.");
			}
        	CheckForEngineInvoke();
		}
		
   		delegate void SetTextCallback(string msg);
   		
        private void Echo(string msg)
        {
       		context.Send(new SendOrPostCallback(
       		delegate(object state)
       	    {
                logOutput.Text += msg + "\r\n";
			    int maxLines = 30; 
			    var lines = logOutput.Lines;
			    int skipLines = lines.Length - maxLines;
			    if( skipLines > 0) {
	    			var newLines = lines.Skip(skipLines);
	    			logOutput.Lines = newLines.ToArray();
			    }
                logOutput.SelectionStart = logOutput.Text.Length;
                logOutput.ScrollToCaret();
       		}), null);
        }

        private void StartAlarm()
        {
       		context.Send(new SendOrPostCallback((state) =>
       	    {
        		stopAlarmButton.Visible = true;
        		stopAlarmLabel.Visible = true;
				alarmTimer.Enabled = true;
				testTheAlarm.Visible = false;
				PlayAlarmSound();
       		}), null);
        }
        
        private void ProcessMessages()
        {
        	try {
	            while (!stopMessages)
	            {
	            	while(  !stopMessages && log.HasLine) {
		            	try {
	            			var message = log.ReadLine();
	            			if( enableAlarmSounds && message.IsAudioAlarm) {
	            				StartAlarm();
	            			}
	            			Echo(message.MessageObject.ToString());
			   			} catch( CollectionTerminatedException) {
			   				break;
		            	}
	            	}
	            	Thread.Sleep(1);
	            }
			} catch( Exception ex) {
				log.Error("ERROR: Thread had an exception:",ex);
			}
        }
   
        List<PortfolioDoc> portfolioDocs = new List<PortfolioDoc>();
        private void btnOptimize_Click(object sender, EventArgs e)
        {
        	RunCommand(0);
        }

        public void RunCommand(int command) {
        	if( !isEngineLoaded) return;
            if (commandWorker.IsBusy)
            {
                MessageBox.Show("A task is already running. Please either the stop the current task or await for its completion before starting a new one.", "Test in progress", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
	            Save();
	            
            	commandWorker.RunWorkerAsync(command);
            }
        }
        
        public void HistoricalButtonClick(object sender, System.EventArgs e)
        {
        	RunCommand(1);
        }
        
        public void ShowChartInvoke()
        {
        	try {
	        	Invoke(new MethodInvoker(ShowChart));
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        }

        public void ShowChart()
        {
        	try {
        		for( int i=portfolioDocs.Count-1; i>=0; i--) {
        			PortfolioDoc portfolioDoc = portfolioDocs[i];
        			if( portfolioDoc.ChartControl.IsDisposed) {
        				portfolioDocs.RemoveAt(i);
        			} else {
        				if( portfolioDoc.ChartControl.IsValid) {
		        			portfolioDoc.Show();
    	    			}
        			}
        		}
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        }
        
        public Chart CreateChartInvoke()
        {
        	Chart chart = null;
        	try {
        		chart =  (Chart) Invoke(new CreateChartDelegate(CreateChart));
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        	return chart;
        }
        
        public void CheckForEngineInvoke()
        {
        	try {
        		Invoke(new Action(CheckForEngine));
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        }
        
        public Chart CreateChart() {
        	Chart chart = null;
        	try {
        		PortfolioDoc portfolioDoc = new PortfolioDoc();
        		portfolioDocs.Add( portfolioDoc);
        		chart = portfolioDoc.ChartControl;
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        	return chart;
        }
        
        public void FlushChartsInvoke()
        {
        	try {
        		Invoke(new MethodInvoker(FlushCharts));
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        }
        
        public void FlushCharts() {
        	try {
        		for( int i=portfolioDocs.Count-1; i>=0; i--) {
        			if( portfolioDocs[i].Visible == false) {
        				portfolioDocs.RemoveAt(i);
        			}
        		}
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        }
        
        public void CloseCharts() {
        	try {
        		for( int i=portfolioDocs.Count-1; i>=0; i--) {
        			portfolioDocs[i].Close();
        			portfolioDocs.RemoveAt(i);
        		}
        	} catch( Exception ex) {
        		log.Error(ex.Message, ex);
        	}
        }
        
        public void RealTimeButtonClick(object sender, System.EventArgs e)
        {
        	RunCommand(2);
        }
        
        public void btnStop_Click(object sender, EventArgs e)
        {
        	StopProcess();
            // Set the progress bar back to 0 and the label
            prgExecute.Value = 0;
            lock(progressLocker) {
            	lblProgress.Text = "Execute Stopped";
            }
        }

        void StartTimePickerCloseUp(object sender, EventArgs e)
        {
        	if( startTimePicker.Value >= EndTime ) {
        		startTimePicker.Value = startTime;
			    DialogResult dr = MessageBox.Show("Start date must be less than end date.", "Continue", MessageBoxButtons.OK);
        	} else {
        		startTime = startTimePicker.Value;
        		Save();
        	}        	
        }

        void EndTimePickerCloseUp(object sender, System.EventArgs e)
        {
        	if( endTimePicker.Value <= startTime ) {
        		endTimePicker.Value = EndTime;
			    DialogResult dr = MessageBox.Show("End date must be greater than start date.", "Continue", MessageBoxButtons.OK);
        	} else {
        		endTime = endTimePicker.Value;
        		// Move the EndTime till the last second of the day.
//        		endTime = endTime.AddDays(1).AddSeconds(-1);
        		Save();
        	}
        }
        
        public void Save() {
			
			projectConfig.SetValue("StartTime",startTime.ToLongDateString());
			projectConfig.SetValue("EndTime",EndTime.ToLongDateString());
			projectConfig.SetValue("Symbol",txtSymbol.Text);
			projectConfig.SetValue("UseModelLoader",modelLoaderBox.Enabled ? "true" : "false");
			projectConfig.SetValue("ModelLoader",modelLoaderBox.Text);
			projectConfig.SetValue("AutoUpdate",projectConfig.GetValue("AutoUpdate"));
			projectConfig.SetValue("DisableCharting",disableChartsCheckBox.Checked ? "true" : "false" );
			
			SaveIntervals();
			
        }
        
        public void SaveAutoUpdate() {
			
			// Add an entry to appSettings.
			int appStgCnt = ConfigurationManager.AppSettings.Count;
			
			projectConfig.SetValue("AutoUpdate",projectConfig.GetValue("AutoUpdate"));
        }
        
        void ProcessWorkerDoWork(object sender, DoWorkEventArgs e)
        {
        	BackgroundWorker bw = sender as BackgroundWorker;
            int arg = (int)e.Argument;
            progressChildren = new Dictionary<int,Progress>();
            if( log.IsTraceEnabled) log.Trace("Started thread to do work.");
            if( Thread.CurrentThread.Name == null) {
            	Thread.CurrentThread.Name = "ProcessWorker";
            }
            switch( arg) {
            	case 0:
		        	if( !isEngineLoaded) return;
            		Optimize(bw);
            		break;
            	case 1:
		        	if( !isEngineLoaded) return;
            		StartHistorical(bw);
            		break;
            	case 2:
		        	if( !isEngineLoaded) return;
            		RealTime(bw);
            		break;
            	case 3:
		        	if( !isEngineLoaded) return;
            		GeneticOptimize(bw);
            		break;
            	case 4:
            		TryUpdate(bw);
            		break;
            }
        }
        
        public void Optimize(BackgroundWorker bw)
        {
        	Starter starter = Factory.Starter.OptimizeStarter();
        	starter.ProjectProperties.Starter.EndTime = (TimeStamp) endTime;
			SetupStarter(starter,bw);
        }
        
        public void GeneticOptimize(BackgroundWorker bw)
        {
        	Starter starter = Factory.Starter.GeneticStarter();
        	starter.ProjectProperties.Starter.EndTime = (TimeStamp) endTime;
			SetupStarter(starter,bw);
        }
        
        public void StartHistorical(BackgroundWorker bw)
        {
			int breakAtBar = 0;
    		if( breakAtBarText.Text.Length > 0) {
    			breakAtBar = System.Convert.ToInt32(breakAtBarText.Text,10);
    		}
    		int replaySpeed = 0;
    		if( replaySpeedTextBox.Text.Length > 0) {
    			replaySpeed = System.Convert.ToInt32(replaySpeedTextBox.Text,10);
    		}
    		Starter starter = Factory.Starter.HistoricalStarter();
    		starter.ProjectProperties.Engine.TickReplaySpeed = replaySpeed;
			starter.ProjectProperties.Engine.BreakAtBar = breakAtBar;
        	starter.ProjectProperties.Starter.EndTime = (TimeStamp) endTime;
    		SetupStarter(starter,bw);
        }
        private void SetupStarter( Starter starter, BackgroundWorker bw) {
        	SetupStarter( starter, bw, false);
        }
    		
        private void SetupStarter( Starter starter, BackgroundWorker bw, bool isRealTime) {
        	FlushChartsInvoke();
    		starter.ProjectProperties.Starter.StartTime = (TimeStamp) startTime;
    		starter.BackgroundWorker = bw;
    		if( !disableChartsCheckBox.Checked) {
	    		starter.CreateChartCallback = new CreateChartCallback(CreateChartInvoke);
    			starter.ShowChartCallback = new ShowChartCallback(ShowChartInvoke);
    		} else {
    			log.Notice("You have the \"disable charts\" check box enabled.");
    		}
	    	starter.ProjectProperties.Chart.ChartType = chartType;
	    	starter.ProjectProperties.Starter.SetSymbols(txtSymbol.Text);
			starter.ProjectProperties.Starter.IntervalDefault = intervalDefault;
			starter.Address = projectConfig.GetValue("ServiceAddress");
			starter.Port = (ushort) projectConfig.GetValue("ServicePort",typeof(ushort));
			starter.AddProvider( projectConfig.GetValue("ProviderAssembly"));
			if( defaultOnly.Checked) {
				starter.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
			} else {
				starter.ProjectProperties.Chart.IntervalChartBar = intervalChartBar;
			}
			if( intervalChartBar.BarUnit == BarUnit.Default) {
				starter.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
			}
			starter.Run(Plugins.Instance.GetLoader(modelLoaderText));
        }
        
		public void RealTime(BackgroundWorker bw)
		{
        	Starter starter = Factory.Starter.RealTimeStarter();
        	enableAlarmSounds = true;
    		SetupStarter(starter,bw,true);
		}
		
		void ProcessWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
       		context.Post(new SendOrPostCallback(
       		delegate(object state)
       	    {
	        	Progress progress = (Progress) e.UserState;
	        	progressChildren[progress.Id] = progress;
	        	
	        	long final = 0;
	        	long current = 0;
	        	foreach( var kvp in progressChildren) {
					Progress child = kvp.Value;	        		
	        		final += child.Final;
	        		current += child.Current;
	        	}
	        	
	            // Calculate the task progress in percentages
	            if( final > 0) {
	            	PercentProgress = Convert.ToInt32((current * 100) / final);
	            } else {
	            	PercentProgress = 0;
	            }
	            // Make progress on the progress bar
	            prgExecute.Value = Math.Min(100,PercentProgress);
	            // Display the current progress on the form
	            lock(progressLocker) {
	            	lblProgress.Text = progress.Text + ": " + progress.Current + " out of " + progress.Final + " (" + PercentProgress + "%)";
	            }
       		}), null);
        }
        
		private Exception taskException;
		
		public void Catch() {
			if( taskException != null) {
				throw new ApplicationException(taskException.Message,taskException);
			}
		}
        void ProcessWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        	enableAlarmSounds = false;
        	taskException = e.Error;
			if(taskException !=null)
			{ 
				if( taskException.InnerException is TickZoomException) {
					log.Error(taskException.InnerException.Message,taskException.InnerException);
				} else {
					log.Error(taskException.Message,taskException);
				}
			}
        }
        
        void StopProcess() {
        	commandWorker.CancelAsync();
        }

        void Terminate() {
            stopMessages = true;
            while( thread_ProcessMessages.IsAlive) {
            	Application.DoEvents();
            }
        	StopProcess();
        	CloseCharts();
            commandWorker.CancelAsync();
            Factory.Engine.Dispose();
            Factory.Provider.Release();
            Factory.TickUtil.TickReader().CloseAll();
        }
        
        private bool stopMessages = false;
        
        void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
        	Terminate();
        }
        
        
        void Form1Load(object sender, EventArgs e)
        {
        }
        
        void DefaultOnlyClick(object sender, EventArgs e)
        {
        	UpdateCheckBoxes();
        }
        
		private void IntervalsUpdate() {
			intervalDefault = Factory.Engine.DefineInterval((BarUnit)defaultCombo.SelectedValue, Convert.ToInt32(defaultBox.Text));
			intervalEngine = Factory.Engine.DefineInterval((BarUnit)engineBarsCombo.SelectedValue, Convert.ToInt32(engineBarsBox.Text));
			intervalChartBar = Factory.Engine.DefineInterval((BarUnit)chartBarsCombo.SelectedValue, Convert.ToInt32(chartBarsBox.Text));
        }

		private void IntervalDefaults() {
			if( defaultBox.Text.Length == 0) { defaultBox.Text = "1"; }
			if( engineBarsBox.Text.Length == 0) { engineBarsBox.Text = "1"; }
			if( chartBarsBox.Text.Length == 0) { chartBarsBox.Text = "1"; }
			if( defaultCombo.Text == "None" || defaultCombo.Text.Length == 0) { 
				defaultCombo.Text = "Hour";
			}
			if( engineBarsCombo.Text == "None" || engineBarsCombo.Text.Length == 0 ) { 
				engineBarsCombo.Text = "Hour";
			}
			if( chartBarsCombo.Text == "None" || chartBarsCombo.Text.Length == 0 ) { 
				chartBarsCombo.Text = "Hour";
			}
		}
        
        void CopyDefaultIntervals() {
       		engineBarsBox.Text = defaultBox.Text;
       		engineBarsCombo.Text = defaultCombo.Text;
       		chartBarsBox.Text = defaultBox.Text;
       		chartBarsCombo.Text = defaultCombo.Text;
        }
		
		void SaveIntervals() {
        	projectConfig.SetValue("DefaultPeriod",defaultBox.Text);
			projectConfig.SetValue("DefaultInterval",defaultCombo.Text);
			projectConfig.SetValue("EnginePeriod",engineBarsBox.Text);
			projectConfig.SetValue("EngineInterval",engineBarsCombo.Text);
			projectConfig.SetValue("ChartPeriod",chartBarsBox.Text);
			projectConfig.SetValue("ChartInterval",chartBarsCombo.Text);
        }

		void LoadIntervals() {
        	defaultBox.Text = CheckNull(projectConfig.GetValue("DefaultPeriod"));
        	defaultCombo.Text = CheckNull(projectConfig.GetValue("DefaultInterval"));
        	engineBarsBox.Text = CheckNull(projectConfig.GetValue("EnginePeriod"));
        	engineBarsCombo.Text = CheckNull(projectConfig.GetValue("EngineInterval"));
        	chartBarsBox.Text = CheckNull(projectConfig.GetValue("ChartPeriod"));
        	chartBarsCombo.Text = CheckNull(projectConfig.GetValue("ChartInterval"));
        }
		
		string CheckNull(string str) {
			if( str == null) { 
				return "";
			} else {
				return str;
			}
		}
        
        void CopyDebugCheckBoxClick(object sender, EventArgs e)
        {
        	CheckBox cb = (CheckBox) sender;
        	cb.Checked = false;
        	CopyDefaultIntervals();
        }
        
        void IntervalChange(object sender, EventArgs e)
        {
            if( isInitialized) IntervalsUpdate();
        }
        
        void EngineRollingCheckBoxClick(object sender, EventArgs e)
        {
        	UpdateCheckBoxes();
        }
        
        void UpdateCheckBoxes() {
       		engineBarsBox.Enabled = !defaultOnly.Checked;
       		engineBarsCombo.Enabled = !defaultOnly.Checked;
       		chartBarsBox.Enabled = !defaultOnly.Checked;
       		chartBarsCombo.Enabled = !defaultOnly.Checked;
       		
        }
        
        ChartType chartType = ChartType.Bar;
        void ChartRadioClick(object sender, EventArgs e)
        {
        	if( barChartRadio.Checked) {
        		chartType = ChartType.Bar;		
        	} else {
        		chartType = ChartType.Time;		
        	}
        }
        
        void BtnGeneticClick(object sender, EventArgs e)
        {
        	RunCommand(3);
        }
        
        string modelLoaderText = null;
        void ModelLoaderBoxSelectedIndexChanged(object sender, EventArgs e)
        {
        	modelLoaderText = modelLoaderBox.Text;
        }
        
        private bool isEngineLoaded = false;
        
		public bool IsEngineLoaded {
			get { return isEngineLoaded; }
		}
        
        void Form1Shown(object sender, EventArgs e)
        {
        	string autoUpdateFlag = projectConfig.GetValue("AutoUpdate");
   			if( "true".Equals(autoUpdateFlag) ) {
	           	commandWorker.RunWorkerAsync(4);
   		    } else {
				log.Notice("To enable AutoUpdate, set AutoUpdate 'true' in " + projectConfig);
				CheckForEngineInvoke();
   			}
        }
        
        public void CheckForEngine() {
   			try {
	   			TickEngine engine = Factory.Engine.TickEngine;
	   			isEngineLoaded = true;
   			} catch( Exception ex) {
   				string msg = "Engine load failed, see log for further detail: " + ex.Message;
   				log.Info(msg,ex);
   			}
   			if( isEngineLoaded) {
	        	initialInterval = Factory.Engine.DefineInterval(BarUnit.Day,1);
				IntervalsUpdate();
	            HandlePlugins();
        	}
    	}
        
		public List<PortfolioDoc> PortfolioDocs {
			get { return portfolioDocs; }
		}
        
		public System.Windows.Forms.TextBox LogOutput {
			get { return logOutput; }
		}
        
        public static string DefaultConfig = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <appSettings>
    <clear />
    <add key=""StartTime"" value=""Wednesday, January 01, 1800"" />
    <add key=""EndTime"" value=""Thursday, July 23, 2009"" />
    <add key=""AutoUpdate"" value=""true"" />
    <add key=""Symbol"" value=""GBP/USD,EUR/JPY"" />
    <add key=""UseModelLoader"" value=""true"" />
    <add key=""AlarmSound"" value=""..\..\Media\59642_AlternatingToneAlarm.wav"" />
    <add key=""ModelLoader"" value=""Example: Reversal Multi-Symbol"" />
    <add key=""Model"" value=""ExampleReversalStrategy"" />
    <add key=""MaxParallelPasses"" value=""1000"" />
    <add key=""DefaultPeriod"" value=""1"" />
    <add key=""DefaultInterval"" value=""Hour"" />
    <add key=""EnginePeriod"" value=""1"" />
    <add key=""EngineInterval"" value=""Hour"" />
    <add key=""ChartPeriod"" value=""1"" />
    <add key=""ChartInterval"" value=""Hour"" />
    <add key=""ServiceAddress"" value=""InProcess"" />
    <add key=""ServicePort"" value=""6490"" />
    <add key=""ProviderAssembly"" value=""MBTFIXProvider"" />
  </appSettings>
</configuration>";
        
        
        void AlarmTimerTick(object sender, EventArgs e)
        {
        	PlayAlarmSound();
        }
        
        bool failedAlarmSound = false;
        void PlayAlarmSound() {
        	if( !failedAlarmSound) {
        		string alarmFile = projectConfig.GetValue("AlarmSound");
        		if( string.IsNullOrEmpty(alarmFile)) {
			   		alarmFile = @"..\..\Media\59642_AlternatingToneAlarm.wav";
        		}
		    	try { 
				    SoundPlayer simpleSound = new SoundPlayer(alarmFile);
				    simpleSound.Play();
		    	} catch( Exception ex) {
		   			failedAlarmSound = true;
		    		log.Error("Failure playing alarm sound file " + alarmFile + " : " + ex.Message,ex);
		    	}
        	}
        }
        
        void StopAlarmButtonClick(object sender, EventArgs e)
        {
        	alarmTimer.Enabled = false;
        	stopAlarmButton.Visible = false;
        	stopAlarmLabel.Visible = false;
			testTheAlarm.Checked = false;
			testTheAlarm.Visible = true;
        }
        
        void TestTheAlarmClick(object sender, EventArgs e)
        {
        	StartAlarm();
        }
   }
}
