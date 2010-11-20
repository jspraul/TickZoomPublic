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
	    public delegate void UpdateProgressDelegate(string fileName, Int64 BytesRead, Int64 TotalBytes);
	    public delegate Chart CreateChartDelegate();
		private bool enableAlarmSounds = false;
        private List<PortfolioDoc> portfolioDocs = new List<PortfolioDoc>();
   		private ViewModel vm;
   		private Log log;
   		
   		public Form1(ViewModel vm) {
	    	log = Factory.SysLog.GetLogger(typeof(ViewModel));
   			this.vm = vm;
            InitializeComponent();
        }
        
        private void Echo(string msg)
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
        }

        private void StartAlarm()
        {
    		stopAlarmButton.Visible = true;
    		stopAlarmLabel.Visible = true;
			alarmTimer.Enabled = true;
			testTheAlarm.Visible = false;
			PlayAlarmSound();
        }
        
        public void ProcessMessages()
        {
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
            	}
			} catch( Exception ex) {
				log.Error("ERROR: Thread had an exception:",ex);
			}
        }
   
        private void btnOptimize_Click(object sender, EventArgs e)
        {
        	vm.RunCommand(0);
        }

        public void HistoricalButtonClick(object sender, System.EventArgs e)
        {
        	vm.RunCommand(1);
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
        	vm.RunCommand(2);
        }
        
        public void btnStop_Click(object sender, EventArgs e)
        {
        	vm.StopProcess();
            // Set the progress bar back to 0 and the label
            prgExecute.Value = 0;
            lock(progressLocker) {
            	lblProgress.Text = "Execute Stopped";
            }
        }

        void StartTimePickerCloseUp(object sender, EventArgs e)
        {
        	if( startTimePicker.Value >= vm.EndDateTime.DateTime ) {
        		startTimePicker.Value = vm.StartDateTime.DateTime;
			    DialogResult dr = MessageBox.Show("Start date must be less than end date.", "Continue", MessageBoxButtons.OK);
        	} else {
        		vm.StartDateTime = new TimeStamp(startTimePicker.Value);
        		vm.Save();
        	}        	
        }

        void EndTimePickerCloseUp(object sender, System.EventArgs e)
        {
        	if( endTimePicker.Value <= vm.StartDateTime.DateTime) {
        		endTimePicker.Value = vm.EndDateTime.DateTime;
			    DialogResult dr = MessageBox.Show("End date must be greater than start date.", "Continue", MessageBoxButtons.OK);
        	} else {
        		vm.EndDateTime = new TimeStamp(endTimePicker.Value);
        		vm.Save();
        	}
        }
        
		public void Catch() {
			if( vm.TaskException != null) {
				throw new ApplicationException(vm.TaskException.Message,vm.TaskException);
			}
		}
        
        void Terminate() {
            stopMessages = true;
        	CloseCharts();
        	vm.Terminate();
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
            vm.IntervalsUpdate();
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
        
        void ChartRadioClick(object sender, EventArgs e)
        {
        	if( barChartRadio.Checked) {
        		vm.ChartType = ChartType.Bar;		
        	} else {
        		vm.ChartType = ChartType.Time;		
        	}
        }
        
        void BtnGeneticClick(object sender, EventArgs e)
        {
        	vm.RunCommand(3);
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
        	vm.TryAutoUpdate();
        }
        
		public List<PortfolioDoc> PortfolioDocs {
			get { return portfolioDocs; }
		}
        
		public System.Windows.Forms.TextBox LogOutput {
			get { return logOutput; }
		}
        
        void AlarmTimerTick(object sender, EventArgs e)
        {
        	PlayAlarmSound();
        }
        
        bool failedAlarmSound = false;
        void PlayAlarmSound() {
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
