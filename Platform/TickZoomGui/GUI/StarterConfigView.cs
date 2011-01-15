using TickZoom.Presentation;
using TickZoom.Presentation.Framework;

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

namespace TickZoom.GUI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Threading;
    using System.Windows.Forms;

    using TickZoom.Api;

    public partial class StarterConfigView : Form
    {
        #region Fields

        private bool enableAlarmSounds = false;
        bool failedAlarmSound = false;
        private bool isEngineLoaded = false;
        private Log log;
        private List<PortfolioDoc> portfolioDocs = new List<PortfolioDoc>();
        private bool stopMessages = false;
        private StarterConfig vm;
        private Execute execute;

        #endregion Fields

        #region Constructors

        public StarterConfigView(Execute execute, StarterConfig vm)
        {
            log = Factory.SysLog.GetLogger(typeof(StarterConfig));
            this.vm = vm;
            this.execute = execute;
            InitializeComponent();
            execute.OnUIThreadLoop( ProcessMessages);
        }

        #endregion Constructors

        #region Properties

        public bool IsEngineLoaded
        {
            get { return isEngineLoaded; }
        }

        public System.Windows.Forms.TextBox LogOutput
        {
            get { return logOutput; }
        }

        public List<PortfolioDoc> PortfolioDocs
        {
            get { return portfolioDocs; }
        }

        #endregion Properties

        #region Methods

        public void Catch()
        {
            if( vm.TaskException != null) {
                throw new ApplicationException(vm.TaskException.Message,vm.TaskException);
            }
        }

        public void CloseCharts()
        {
            try {
                for( int i=portfolioDocs.Count-1; i>=0; i--) {
                    portfolioDocs[i].Close();
                    portfolioDocs.RemoveAt(i);
                }
            } catch( Exception ex) {
                log.Error(ex.Message, ex);
            }
        }
        
        public Chart CreateChart() {
        	return execute.OnUIThread<Chart>((Func<Chart>)CreateChartWork);
        }

        private Chart CreateChartWork()
        {
            Chart chart = null;
            try {
                PortfolioDoc portfolioDoc = new PortfolioDoc(execute);
                portfolioDocs.Add( portfolioDoc);
                chart = portfolioDoc.ChartControl;
            } catch( Exception ex) {
                log.Error(ex.Message, ex);
            }
            return chart;
        }
        
        public void FlushCharts() {
        	execute.OnUIThread(FlushChartsWork);
        }

        private void FlushChartsWork()
        {
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

        public bool ProcessMessages()
        {
        	var result = false;
            try {
                if(  !stopMessages && log.HasLine) {
                    try {
                        var message = log.ReadLine();
                        if( enableAlarmSounds && message.IsAudioAlarm) {
                            StartAlarm();
                        }
                        Echo(message.MessageObject.ToString());
               			} catch( CollectionTerminatedException) {
                        stopMessages = true;
                    }
        			result = true;
        		}
            } catch( Exception ex) {
                log.Error("ERROR: Thread had an exception:",ex);
        	}
        	return result;
        }
        
        public void ShowChart() {
        	execute.OnUIThread(ShowChartWork);
        }

        private void ShowChartWork()
        {
            try {
                for( int i=portfolioDocs.Count-1; i>=0; i--) {
                    PortfolioDoc portfolioDoc = portfolioDocs[i];
                    if( portfolioDoc.ChartControl.IsDisposed) {
                        portfolioDocs.RemoveAt(i);
                    } else {
                        if( portfolioDoc.ChartControl.IsValid && this.Visible) {
                            portfolioDoc.Show();
                        }
                    }
                }
            } catch( Exception ex) {
                log.Error(ex.Message, ex);
            }
        }

        void AlarmTimerTick(object sender, EventArgs e)
        {
            PlayAlarmSound();
        }

        void ChartRadioClick(object sender, EventArgs e)
        {
            if( barChartRadio.Checked) {
                vm.ChartType = ChartType.Bar;
            } else {
                vm.ChartType = ChartType.Time;
            }
        }

        string CheckNull(string str)
        {
            if( str == null) {
                return "";
            } else {
                return str;
            }
        }

        void DefaultOnlyClick(object sender, EventArgs e)
        {
            UpdateCheckBoxes();
        }

        private void Echo(string msg)
        {
        	execute.OnUIThread( () => {
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
        	});
        }

        private void EndTimePickerCloseUp(object sender, System.EventArgs e)
        {
            if( endDateTime.Value <= vm.StartDateTime) {
                endDateTime.Value = vm.EndDateTime;
                DialogResult dr = MessageBox.Show("End date must be greater than start date.", "Continue", MessageBoxButtons.OK);
            } else {
                vm.EndDateTime = endDateTime.Value;
                vm.Save();
            }
        }

        void EngineRollingCheckBoxClick(object sender, EventArgs e)
        {
            UpdateCheckBoxes();
        }

        void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            Terminate();
        }

        void Form1Load(object sender, EventArgs e)
        {
        }

        void Form1Shown(object sender, EventArgs e)
        {
            defaultPeriod.DataBindings.Add( "Enabled", vm, "UseDefaultInterval");
            defaultBarUnit.DataBindings.Add( "Enabled", vm, "UseDefaultInterval");
            enginePeriod.DataBindings.Add( "Enabled", vm, "UseOtherIntervals");
            engineBarUnit.DataBindings.Add( "Enabled", vm, "UseOtherIntervals");
            chartPeriod.DataBindings.Add( "Enabled", vm, "UseOtherIntervals");
            chartBarUnit.DataBindings.Add( "Enabled", vm, "UseOtherIntervals");
        }

        void PlayAlarmSound()
        {
            if( !failedAlarmSound) {
                try {
                    SoundPlayer simpleSound = new SoundPlayer(vm.AlarmFile);
                    simpleSound.Play();
                } catch( Exception ex) {
               			failedAlarmSound = true;
                    log.Error("Failure playing alarm sound file " + vm.AlarmFile + " : " + ex.Message,ex);
                }
            }
        }

        private void StartAlarm()
        {
            stopAlarm.Visible = true;
            stopAlarmLabel.Visible = true;
            alarmTimer.Enabled = true;
            testTheAlarm.Visible = false;
            PlayAlarmSound();
        }

        void StartTimePickerCloseUp(object sender, EventArgs e)
        {
            if( startDateTime.Value >= vm.EndDateTime) {
                startDateTime.Value = vm.StartDateTime;
                DialogResult dr = MessageBox.Show("Start date must be less than end date.", "Continue", MessageBoxButtons.OK);
            } else {
                vm.StartDateTime = startDateTime.Value;
                vm.Save();
            }
        }

        void StopAlarmButtonClick(object sender, EventArgs e)
        {
            alarmTimer.Enabled = false;
            stopAlarm.Visible = false;
            stopAlarmLabel.Visible = false;
            testTheAlarm.Checked = false;
            testTheAlarm.Visible = true;
        }

        void Terminate()
        {
            stopMessages = true;
            CloseCharts();
            vm.Dispose();
        }

        void TestTheAlarmClick(object sender, EventArgs e)
        {
            StartAlarm();
        }

        void UpdateCheckBoxes()
        {
            enginePeriod.Enabled = !useDefaultInterval.Checked;
               		engineBarUnit.Enabled = !useDefaultInterval.Checked;
               		chartPeriod.Enabled = !useDefaultInterval.Checked;
               		chartBarUnit.Enabled = !useDefaultInterval.Checked;
        }

        #endregion Methods
    }
}