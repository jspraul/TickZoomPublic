#region Header

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

#endregion Header

namespace TickZoom
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Media;
    using System.Threading;
    using System.Windows.Forms;

    using TickZoom.Api;
    using TickZoom.GUI.Framework;

    public class ViewModel
    {
        #region Fields

        public Func<Chart> createChart;

        private string alarmFile;
        private int breakAtBar = 0;
        private BarUnit chartBarUnit = BarUnit.Hour;
        private int chartPeriod = 1;
        private ChartType chartType = ChartType.Bar;
        private System.ComponentModel.BackgroundWorker commandWorker;
        private SynchronizationContext context;
        private BarUnit defaultBarUnit = BarUnit.Hour;
        private bool defaultOnly = true;
        private int defaultPeriod = 1;
        private bool disableCharting;
        private bool enableAlarmSounds = false;
        private TimeStamp endDateTime;
        private BarUnit engineBarUnit = BarUnit.Hour;
        private int enginePeriod = 1;
        private bool failedAlarmSound = false;
        private Action flushCharts;
        private Interval initialInterval;
        private Interval intervalChartBar;
        private Interval intervalDefault;
        private Interval intervalEngine;
        private bool isEngineLoaded = false;
        bool isInitialized = false;
        Log log;
        private TimeStamp maxDateTime;
        private TimeStamp minDateTime;
        private string modelLoaderText = null;

        // The progress of the task in percentage
        private int percentProgress;
        Dictionary<int, Progress> progressChildren = new Dictionary<int,Progress>();
        private string progressText;
        ConfigFile projectConfig;
        private int replaySpeed = 0;
        private Action showChart;
        private TimeStamp startDateTime;
        private string symbolList;
        private Exception taskException;
        string tickZoomEngine;

        #endregion Fields

        #region Constructors

        public ViewModel()
            : this("project")
        {
        }

        public ViewModel(string projectName)
        {
            ConfigurationManager.AppSettings.Set("ProviderAddress","InProcess");
            log = Factory.SysLog.GetLogger(typeof(ViewModel));
            string storageFolder = Factory.Settings["AppDataFolder"];
            string workspace = Path.Combine(storageFolder,"Workspace");
            string projectFile = Path.Combine(workspace,projectName+".tzproj");
            projectConfig = new ConfigFile( projectFile, GetDefaultConfig());
            context = SynchronizationContext.Current;
            this.commandWorker = new System.ComponentModel.BackgroundWorker();
            this.commandWorker.WorkerReportsProgress = true;
            this.commandWorker.WorkerSupportsCancellation = true;
            this.commandWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ProcessWorkerDoWork);
            this.commandWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ProcessWorkerRunWorkerCompleted);
            this.commandWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ProcessWorkerProgressChanged);
            if(context == null)
            {
                context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
            }
            intervalDefault = initialInterval;
            intervalEngine = initialInterval;
            intervalChartBar = initialInterval;
               		alarmFile = projectConfig.GetValue("AlarmSound");
            if( string.IsNullOrEmpty(alarmFile)) {
               		alarmFile = @"..\..\Media\59642_AlternatingToneAlarm.wav";
            }
            IntervalDefaults();
            LoadIntervals();
            IntervalDefaults();
            storageFolder = Factory.Settings["AppDataFolder"];
            tickZoomEngine = projectConfig.GetValue("TickZoomEngine");
            Array units = Enum.GetValues(typeof(BarUnit));
            minDateTime = new TimeStamp(1800,1,1);
            startDateTime = minDateTime;
            maxDateTime = TimeStamp.UtcNow;
               	maxDateTime.AddDays(1);
            maxDateTime.AddSeconds(-1);
            endDateTime = maxDateTime;

            var disableChartingString = projectConfig.GetValue("DisableCharting");
            disableCharting = !string.IsNullOrEmpty(disableChartingString) && "true".Equals(disableChartingString.ToLower());

            string startTimeStr = projectConfig.GetValue("StartTime");
            string endTimeStr = projectConfig.GetValue("EndTime");

               		if( startTimeStr != null) {
                try {
               			startDateTime = new TimeStamp(startTimeStr);
                } catch {
                    startDateTime = minDateTime;
                }
               		}

               		if( endTimeStr != null) {
                try {
               			var time = new TimeStamp(endTimeStr);
               			time.AddDays(1);
               				time.AddSeconds(-1);
               			EndDateTime = time;
                } catch {
                    endDateTime = maxDateTime;
                }
               			if( endDateTime > maxDateTime) {
               				endDateTime = maxDateTime;
               			}
               		}

            isInitialized = true;
        }

        #endregion Constructors

        #region Delegates

        public delegate Chart CreateChartDelegate();

        public delegate void UpdateProgressDelegate(string fileName, Int64 BytesRead, Int64 TotalBytes);

        #endregion Delegates

        #region Properties

        public string AlarmFile
        {
            get { return alarmFile; }
            set { alarmFile = value; }
        }

        public bool BarChartEnabled
        {
            get {
                return chartType == ChartType.Bar;
            }
        }

        public ChartType ChartType
        {
            get { return chartType; }
            set { chartType = value; }
        }

        public BackgroundWorker CommandWorker
        {
            get { return commandWorker; }
        }

        public Func<Chart> CreateChart
        {
            get { return createChart; }
            set { createChart = value; }
        }

        public bool EnableAlarmSounds
        {
            get { return enableAlarmSounds; }
        }

        public bool EnableChartBarsChoice
        {
            get {
                return !defaultOnly;
            }
        }

        public bool EnableEngineBarsChoice
        {
            get {
                return !defaultOnly;
            }
        }

        public TimeStamp EndDateTime
        {
            get { return endDateTime; }
            set { endDateTime = value; }
        }

        public Action FlushCharts
        {
            get { return flushCharts; }
            set { flushCharts = value; }
        }

        public bool IsEngineLoaded
        {
            get { return isEngineLoaded; }
        }

        public TimeStamp MaxDateTime
        {
            get { return maxDateTime; }
            set { maxDateTime = value; }
        }

        public TimeStamp MinDateTime
        {
            get { return minDateTime; }
            set { minDateTime = value; }
        }

        public List<string> ModelLoaderList
        {
            get {
                var plugins = Plugins.Instance;
                var loaders = plugins.GetLoaders();
                var modelLoaderList = new List<string>();
                for( int i=0; i<loaders.Count; i++) {
                    if( loaders[i].IsVisibleInGUI) {
                        modelLoaderList.Add(loaders[i].Name);
                    }
                }
                return modelLoaderList;
            }
        }

        public string ModelLoaderSelected
        {
            get {
                return projectConfig.GetValue("ModelLoader");
            }
        }

        public int PercentProgress
        {
            get { return percentProgress; }
            set { percentProgress = value; }
        }

        public Action ShowChart
        {
            get { return showChart; }
            set { showChart = value; }
        }

        public TimeStamp StartDateTime
        {
            get { return startDateTime; }
            set { startDateTime = value; }
        }

        public string SymbolList
        {
            get { return symbolList; }
            set { symbolList = value; }
        }

        public Exception TaskException
        {
            get { return taskException; }
            set { taskException = value; }
        }

        #endregion Properties

        #region Methods

        public void Catch()
        {
            if( taskException != null) {
                throw new ApplicationException(taskException.Message,taskException);
            }
        }

        public void CheckForEngine()
        {
            try {
               			TickEngine engine = Factory.Engine.TickEngine;
               			isEngineLoaded = true;
               			} catch( Exception) {
               				string msg = "Sorry, cannot find an engine compatible with this version.";
               				log.Notice(msg);
               			}
               			if( isEngineLoaded) {
                initialInterval = Factory.Engine.DefineInterval(BarUnit.Day,1);
                IntervalsUpdate();
            }
        }

        public Chart CreateChartInvoke()
        {
            return Execute.OnUIThread<Chart>(CreateChart);
        }

        public void GeneticOptimize(BackgroundWorker bw)
        {
            Starter starter = Factory.Starter.GeneticStarter();
            starter.ProjectProperties.Starter.EndTime = (TimeStamp) endDateTime;
                SetupStarter(starter,bw);
        }

        public void HistoricalButtonClick(object sender, System.EventArgs e)
        {
            RunCommand(1);
        }

        public void IntervalsUpdate()
        {
            intervalDefault = Factory.Engine.DefineInterval(defaultBarUnit, defaultPeriod);
            intervalEngine = Factory.Engine.DefineInterval(engineBarUnit, enginePeriod);
            intervalChartBar = Factory.Engine.DefineInterval(chartBarUnit, chartPeriod);
        }

        public void Optimize(BackgroundWorker bw)
        {
            Starter starter = Factory.Starter.OptimizeStarter();
            starter.ProjectProperties.Starter.EndTime = (TimeStamp) endDateTime;
                SetupStarter(starter,bw);
        }

        public void RealTime(BackgroundWorker bw)
        {
            Starter starter = Factory.Starter.RealTimeStarter();
            enableAlarmSounds = true;
            SetupStarter(starter,bw,true);
        }

        public void RunCommand(int command)
        {
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

        public void Save()
        {
            projectConfig.SetValue("StartTime",startDateTime.ToString());
            projectConfig.SetValue("EndTime",endDateTime.ToString());
            projectConfig.SetValue("Symbol",symbolList);
            projectConfig.SetValue("ModelLoader",ModelLoaderSelected);
            projectConfig.SetValue("AutoUpdate",projectConfig.GetValue("AutoUpdate"));
            projectConfig.SetValue("DisableCharting",disableCharting ? "true" : "false" );

            SaveIntervals();
        }

        public void SaveAutoUpdate()
        {
            projectConfig.SetValue("AutoUpdate","false");

            // Add an entry to appSettings.
            int appStgCnt = ConfigurationManager.AppSettings.Count;

            projectConfig.SetValue("AutoUpdate",projectConfig.GetValue("AutoUpdate"));
        }

        public void SetupForm( Form1 form)
        {
            form.DefaultCombo.DataSource = Enum.GetValues(typeof(BarUnit));
            form.EngineBarsCombo.DataSource = Enum.GetValues(typeof(BarUnit));
            form.ChartBarsCombo.DataSource = Enum.GetValues(typeof(BarUnit));
            form.SymbolList.Text = symbolList;
            form.StartTimePicker.Value = startDateTime.DateTime;
            form.StartTimePicker.MaxDate = maxDateTime.DateTime;

            form.EndTimePicker.MinDate = minDateTime.DateTime;
            form.EndTimePicker.Value = endDateTime.DateTime;

               		form.EngineBarsBox.Enabled = !defaultOnly;
               		form.EngineBarsCombo.Enabled = !defaultOnly;
               		form.ChartBarsBox.Enabled = !defaultOnly;
               		form.ChartBarsCombo.Enabled = !defaultOnly;
            form.ModelLoaderBox.Enabled = true;
            form.ModelLoaderBox.DataSource = ModelLoaderList;
            form.ModelLoaderBox.SelectedItem = ModelLoaderSelected;
            form.LblProgress.Text = progressText;
            form.PrgExecute.Value = percentProgress;
            form.DefaultBox.Text = defaultPeriod.ToString();
            form.EngineBarsBox.Text = enginePeriod.ToString();
            form.ChartBarsBox.Text = chartPeriod.ToString();
            form.DefaultCombo.Text = defaultBarUnit.ToString();
            form.EngineBarsCombo.Text = engineBarUnit.ToString();
            form.ChartBarsCombo.Text = chartBarUnit.ToString();
            form.BarChartRadio.Checked = BarChartEnabled;
        }

        public void ShowChartInvoke()
        {
            try {
                Execute.OnUIThread(ShowChart);
            } catch( Exception ex) {
                log.Error(ex.Message, ex);
            }
        }

        public void StartHistorical(BackgroundWorker bw)
        {
            Starter starter = Factory.Starter.HistoricalStarter();
            starter.ProjectProperties.Engine.TickReplaySpeed = replaySpeed;
            starter.ProjectProperties.Engine.BreakAtBar = breakAtBar;
            starter.ProjectProperties.Starter.EndTime = (TimeStamp) endDateTime;
            SetupStarter(starter,bw);
        }

        public void StopProcess()
        {
            commandWorker.CancelAsync();
        }

        public void Terminate()
        {
            StopProcess();
            Factory.Engine.Dispose();
            Factory.Provider.Release();
            Factory.TickUtil.TickReader().CloseAll();
        }

        public void TryAutoUpdate()
        {
            var autoUpdateFlag = projectConfig.GetValue("AutoUpdate");
               			if( "true".Equals(autoUpdateFlag) ) {
               	commandWorker.RunWorkerAsync(4);
               		    } else {
                log.Notice("To enable AutoUpdate, set AutoUpdate 'true' in " + projectConfig);
                CheckForEngine();
               			}
        }

        void AlarmTimerTick(object sender, EventArgs e)
        {
            PlayAlarmSound();
        }

        string CheckNull(string str)
        {
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

        void CopyDefaultIntervals()
        {
            enginePeriod = defaultPeriod;
               		engineBarUnit = defaultBarUnit;
               		chartPeriod = defaultPeriod;
               		chartBarUnit = defaultBarUnit;
        }

        void IntervalChange(object sender, EventArgs e)
        {
            if( isInitialized) IntervalsUpdate();
        }

        private void IntervalDefaults()
        {
        }

        void LoadIntervals()
        {
            defaultPeriod = int.Parse(CheckNull(projectConfig.GetValue("DefaultPeriod")));
            defaultBarUnit = (BarUnit) BarUnit.Parse(typeof(BarUnit),CheckNull(projectConfig.GetValue("DefaultInterval")));
            enginePeriod = int.Parse(CheckNull(projectConfig.GetValue("EnginePeriod")));
            engineBarUnit = (BarUnit) BarUnit.Parse(typeof(BarUnit),CheckNull(projectConfig.GetValue("EngineInterval")));
            chartPeriod = int.Parse(CheckNull(projectConfig.GetValue("ChartPeriod")));
            chartBarUnit = (BarUnit) BarUnit.Parse(typeof(BarUnit),CheckNull(projectConfig.GetValue("ChartInterval")));
        }

        void PlayAlarmSound()
        {
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
                progressText = progress.Text + ": " + progress.Current + " out of " + progress.Final + " (" + PercentProgress + "%)";
               		}), null);
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

        void SaveIntervals()
        {
            projectConfig.SetValue("DefaultPeriod",defaultPeriod.ToString());
            projectConfig.SetValue("DefaultInterval",defaultBarUnit.ToString());
            projectConfig.SetValue("EnginePeriod",enginePeriod.ToString());
            projectConfig.SetValue("EngineInterval",engineBarUnit.ToString());
            projectConfig.SetValue("ChartPeriod",chartPeriod.ToString());
            projectConfig.SetValue("ChartInterval",chartBarUnit.ToString());
        }

        private void SetupStarter( Starter starter, BackgroundWorker bw)
        {
            SetupStarter( starter, bw, false);
        }

        private void SetupStarter( Starter starter, BackgroundWorker bw, bool isRealTime)
        {
            flushCharts();
            starter.ProjectProperties.Starter.StartTime = (TimeStamp) startDateTime;
            starter.BackgroundWorker = bw;
            if( !disableCharting) {
                starter.CreateChartCallback = CreateChartInvoke;
                starter.ShowChartCallback = ShowChartInvoke;
            } else {
                log.Notice("You have the \"disable charts\" check box enabled.");
            }
            starter.ProjectProperties.Chart.ChartType = chartType;
            starter.ProjectProperties.Starter.SetSymbols(symbolList);
            starter.ProjectProperties.Starter.IntervalDefault = intervalDefault;
            starter.Address = projectConfig.GetValue("ServiceAddress");
            starter.Config = projectConfig.GetValue("ServiceConfig");
            starter.Port = (ushort) projectConfig.GetValue("ServicePort",typeof(ushort));
            starter.AddProvider( projectConfig.GetValue("ProviderAssembly"));
            if( defaultOnly) {
                starter.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
            } else {
                starter.ProjectProperties.Chart.IntervalChartBar = intervalChartBar;
            }
            if( intervalChartBar.BarUnit == BarUnit.Default) {
                starter.ProjectProperties.Chart.IntervalChartBar = intervalDefault;
            }
            log.Info("Running Loader named: " + modelLoaderText);
            starter.Run(Plugins.Instance.GetLoader(modelLoaderText));
        }

        private void TryUpdate(BackgroundWorker bw)
        {
            if( Factory.AutoUpdate(bw)) {
                log.Notice("AutoUpdate succesful. Restart unnecessary.");
            }
            CheckForEngine();
        }

        public static string GetDefaultConfig() {
        	return @"<?xml version=""1.0"" encoding=""utf-8""?>
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
        }

       #endregion Methods
    }
}