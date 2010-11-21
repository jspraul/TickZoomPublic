namespace TickZoom.GUI
{
	partial class StarterConfigView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                Terminate();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        	this.components = new System.ComponentModel.Container();
        	this.symbolLabel = new System.Windows.Forms.Label();
        	this.symbolList = new System.Windows.Forms.TextBox();
        	this.optimize = new System.Windows.Forms.Button();
        	this.percentProgress = new System.Windows.Forms.ProgressBar();
        	this.stop = new System.Windows.Forms.Button();
        	this.lblProgress = new System.Windows.Forms.Label();
        	this.historical = new System.Windows.Forms.Button();
        	this.startDateTime = new System.Windows.Forms.DateTimePicker();
        	this.endDateTime = new System.Windows.Forms.DateTimePicker();
        	this.startLabel = new System.Windows.Forms.Label();
        	this.endLabel = new System.Windows.Forms.Label();
        	this.realtime = new System.Windows.Forms.Button();
        	this.logOutput = new System.Windows.Forms.TextBox();
        	this.breakAtBarText = new System.Windows.Forms.TextBox();
        	this.label1 = new System.Windows.Forms.Label();
        	this.replaySpeedTextBox = new System.Windows.Forms.TextBox();
        	this.label2 = new System.Windows.Forms.Label();
        	this.intervalEngineTxt = new System.Windows.Forms.Label();
        	this.engineBarUnit = new System.Windows.Forms.ComboBox();
        	this.intervals = new System.Windows.Forms.GroupBox();
        	this.testTheAlarm = new System.Windows.Forms.CheckBox();
        	this.stopAlarmLabel = new System.Windows.Forms.Label();
        	this.label7 = new System.Windows.Forms.Label();
        	this.timeChartRadio = new System.Windows.Forms.RadioButton();
        	this.barChartRadio = new System.Windows.Forms.RadioButton();
        	this.label3 = new System.Windows.Forms.Label();
        	this.label4 = new System.Windows.Forms.Label();
        	this.copyDefaultIntervals = new System.Windows.Forms.CheckBox();
        	this.chartPeriod = new System.Windows.Forms.TextBox();
        	this.enginePeriod = new System.Windows.Forms.TextBox();
        	this.defaultPeriod = new System.Windows.Forms.TextBox();
        	this.useDefaultInterval = new System.Windows.Forms.CheckBox();
        	this.chartBarUnit = new System.Windows.Forms.ComboBox();
        	this.defaultTxt = new System.Windows.Forms.Label();
        	this.defaultBarUnit = new System.Windows.Forms.ComboBox();
        	this.chartBarsTxt = new System.Windows.Forms.Label();
        	this.timeFrameTxt = new System.Windows.Forms.Label();
        	this.periodTxt = new System.Windows.Forms.Label();
        	this.disableCharting = new System.Windows.Forms.CheckBox();
        	this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
        	this.geneticOptimize = new System.Windows.Forms.Button();
        	this.modelLoader = new System.Windows.Forms.ComboBox();
        	this.groupBox1 = new System.Windows.Forms.GroupBox();
        	this.label5 = new System.Windows.Forms.Label();
        	this.stopAlarm = new System.Windows.Forms.Button();
        	this.alarmTimer = new System.Windows.Forms.Timer(this.components);
        	this.intervals.SuspendLayout();
        	this.groupBox1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// symbolLabel
        	// 
        	this.symbolLabel.AutoSize = true;
        	this.symbolLabel.Location = new System.Drawing.Point(13, 16);
        	this.symbolLabel.Name = "symbolLabel";
        	this.symbolLabel.Size = new System.Drawing.Size(41, 13);
        	this.symbolLabel.TabIndex = 0;
        	this.symbolLabel.Text = "Symbol";
        	// 
        	// symbolList
        	// 
        	this.symbolList.Location = new System.Drawing.Point(78, 13);
        	this.symbolList.Name = "symbolList";
        	this.symbolList.Size = new System.Drawing.Size(110, 20);
        	this.symbolList.TabIndex = 1;
        	this.symbolList.Text = "USD/JPY";
        	// 
        	// optimize
        	// 
        	this.optimize.Location = new System.Drawing.Point(110, 118);
        	this.optimize.Name = "optimize";
        	this.optimize.Size = new System.Drawing.Size(78, 23);
        	this.optimize.TabIndex = 19;
        	this.optimize.Text = "Optimize";
        	this.optimize.UseVisualStyleBackColor = true;
        	// 
        	// percentProgress
        	// 
        	this.percentProgress.Location = new System.Drawing.Point(12, 169);
        	this.percentProgress.Maximum = 1000;
        	this.percentProgress.Name = "percentProgress";
        	this.percentProgress.Size = new System.Drawing.Size(475, 23);
        	this.percentProgress.Step = 1;
        	this.percentProgress.TabIndex = 8;
        	// 
        	// stop
        	// 
        	this.stop.Location = new System.Drawing.Point(413, 118);
        	this.stop.Name = "stop";
        	this.stop.Size = new System.Drawing.Size(75, 23);
        	this.stop.TabIndex = 22;
        	this.stop.Text = "Stop";
        	this.stop.UseVisualStyleBackColor = true;
        	// 
        	// lblProgress
        	// 
        	this.lblProgress.AutoSize = true;
        	this.lblProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
        	this.lblProgress.Location = new System.Drawing.Point(12, 149);
        	this.lblProgress.Name = "lblProgress";
        	this.lblProgress.Size = new System.Drawing.Size(72, 13);
        	this.lblProgress.TabIndex = 8;
        	this.lblProgress.Text = "Awaiting Start";
        	// 
        	// historical
        	// 
        	this.historical.Location = new System.Drawing.Point(13, 118);
        	this.historical.Name = "historical";
        	this.historical.Size = new System.Drawing.Size(75, 23);
        	this.historical.TabIndex = 18;
        	this.historical.Text = "Historical";
        	this.historical.UseVisualStyleBackColor = true;
        	// 
        	// startDateTime
        	// 
        	this.startDateTime.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        	this.startDateTime.Location = new System.Drawing.Point(78, 51);
        	this.startDateTime.Name = "startDateTime";
        	this.startDateTime.Size = new System.Drawing.Size(110, 20);
        	this.startDateTime.TabIndex = 2;
        	this.startDateTime.CloseUp += new System.EventHandler(this.StartTimePickerCloseUp);
        	// 
        	// endDateTime
        	// 
        	this.endDateTime.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        	this.endDateTime.Location = new System.Drawing.Point(78, 91);
        	this.endDateTime.Name = "endDateTime";
        	this.endDateTime.Size = new System.Drawing.Size(110, 20);
        	this.endDateTime.TabIndex = 3;
        	this.endDateTime.CloseUp += new System.EventHandler(this.EndTimePickerCloseUp);
        	// 
        	// startLabel
        	// 
        	this.startLabel.Location = new System.Drawing.Point(12, 55);
        	this.startLabel.Name = "startLabel";
        	this.startLabel.Size = new System.Drawing.Size(60, 16);
        	this.startLabel.TabIndex = 14;
        	this.startLabel.Text = "Start Date";
        	// 
        	// endLabel
        	// 
        	this.endLabel.Location = new System.Drawing.Point(12, 95);
        	this.endLabel.Name = "endLabel";
        	this.endLabel.Size = new System.Drawing.Size(60, 16);
        	this.endLabel.TabIndex = 15;
        	this.endLabel.Text = "End Date";
        	// 
        	// realtime
        	// 
        	this.realtime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.realtime.Location = new System.Drawing.Point(318, 118);
        	this.realtime.Name = "realtime";
        	this.realtime.Size = new System.Drawing.Size(74, 23);
        	this.realtime.TabIndex = 1;
        	this.realtime.Text = "Real Time";
        	this.realtime.UseVisualStyleBackColor = true;
        	// 
        	// logOutput
        	// 
        	this.logOutput.Location = new System.Drawing.Point(12, 198);
        	this.logOutput.Multiline = true;
        	this.logOutput.Name = "logOutput";
        	this.logOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        	this.logOutput.Size = new System.Drawing.Size(475, 161);
        	this.logOutput.TabIndex = 17;
        	// 
        	// breakAtBarText
        	// 
        	this.breakAtBarText.Location = new System.Drawing.Point(428, 92);
        	this.breakAtBarText.Name = "breakAtBarText";
        	this.breakAtBarText.Size = new System.Drawing.Size(60, 20);
        	this.breakAtBarText.TabIndex = 6;
        	// 
        	// label1
        	// 
        	this.label1.Location = new System.Drawing.Point(354, 95);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(68, 16);
        	this.label1.TabIndex = 19;
        	this.label1.Text = "Break at bar";
        	// 
        	// replaySpeedTextBox
        	// 
        	this.replaySpeedTextBox.Location = new System.Drawing.Point(280, 92);
        	this.replaySpeedTextBox.Name = "replaySpeedTextBox";
        	this.replaySpeedTextBox.Size = new System.Drawing.Size(58, 20);
        	this.replaySpeedTextBox.TabIndex = 5;
        	// 
        	// label2
        	// 
        	this.label2.Location = new System.Drawing.Point(194, 95);
        	this.label2.Name = "label2";
        	this.label2.Size = new System.Drawing.Size(80, 23);
        	this.label2.TabIndex = 21;
        	this.label2.Text = "Replay Speed";
        	// 
        	// intervalEngineTxt
        	// 
        	this.intervalEngineTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.intervalEngineTxt.Location = new System.Drawing.Point(6, 109);
        	this.intervalEngineTxt.Name = "intervalEngineTxt";
        	this.intervalEngineTxt.Size = new System.Drawing.Size(80, 15);
        	this.intervalEngineTxt.TabIndex = 22;
        	this.intervalEngineTxt.Text = "Engine Bars";
        	// 
        	// engineBarUnit
        	// 
        	this.engineBarUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.engineBarUnit.FormattingEnabled = true;
        	this.engineBarUnit.Location = new System.Drawing.Point(145, 105);
        	this.engineBarUnit.Name = "engineBarUnit";
        	this.engineBarUnit.Size = new System.Drawing.Size(121, 21);
        	this.engineBarUnit.TabIndex = 12;
        	this.engineBarUnit.SelectedIndexChanged += new System.EventHandler(this.IntervalChange);
        	// 
        	// intervals
        	// 
        	this.intervals.Controls.Add(this.testTheAlarm);
        	this.intervals.Controls.Add(this.stopAlarmLabel);
        	this.intervals.Controls.Add(this.label7);
        	this.intervals.Controls.Add(this.timeChartRadio);
        	this.intervals.Controls.Add(this.barChartRadio);
        	this.intervals.Controls.Add(this.label3);
        	this.intervals.Controls.Add(this.label4);
        	this.intervals.Controls.Add(this.copyDefaultIntervals);
        	this.intervals.Controls.Add(this.chartPeriod);
        	this.intervals.Controls.Add(this.enginePeriod);
        	this.intervals.Controls.Add(this.defaultPeriod);
        	this.intervals.Controls.Add(this.useDefaultInterval);
        	this.intervals.Controls.Add(this.chartBarUnit);
        	this.intervals.Controls.Add(this.defaultTxt);
        	this.intervals.Controls.Add(this.defaultBarUnit);
        	this.intervals.Controls.Add(this.chartBarsTxt);
        	this.intervals.Controls.Add(this.timeFrameTxt);
        	this.intervals.Controls.Add(this.periodTxt);
        	this.intervals.Controls.Add(this.intervalEngineTxt);
        	this.intervals.Controls.Add(this.engineBarUnit);
        	this.intervals.Controls.Add(this.disableCharting);
        	this.intervals.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.intervals.Location = new System.Drawing.Point(494, 10);
        	this.intervals.Name = "intervals";
        	this.intervals.Size = new System.Drawing.Size(276, 349);
        	this.intervals.TabIndex = 25;
        	this.intervals.TabStop = false;
        	this.intervals.Text = "Intervals";
        	// 
        	// testTheAlarm
        	// 
        	this.testTheAlarm.Location = new System.Drawing.Point(10, 320);
        	this.testTheAlarm.Name = "testTheAlarm";
        	this.testTheAlarm.Size = new System.Drawing.Size(256, 24);
        	this.testTheAlarm.TabIndex = 24;
        	this.testTheAlarm.Text = "Test the error alarm sound.";
        	this.testTheAlarm.UseVisualStyleBackColor = true;
        	this.testTheAlarm.Click += new System.EventHandler(this.TestTheAlarmClick);
        	// 
        	// stopAlarmLabel
        	// 
        	this.stopAlarmLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.stopAlarmLabel.ForeColor = System.Drawing.Color.Red;
        	this.stopAlarmLabel.Location = new System.Drawing.Point(6, 304);
        	this.stopAlarmLabel.Name = "stopAlarmLabel";
        	this.stopAlarmLabel.Size = new System.Drawing.Size(264, 40);
        	this.stopAlarmLabel.TabIndex = 54;
        	this.stopAlarmLabel.Text = "ERROR: Please check the logs after you stop the alarm.";
        	this.stopAlarmLabel.Visible = false;
        	// 
        	// label7
        	// 
        	this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.label7.Location = new System.Drawing.Point(6, 171);
        	this.label7.Name = "label7";
        	this.label7.Size = new System.Drawing.Size(80, 20);
        	this.label7.TabIndex = 52;
        	this.label7.Text = "Chart Type";
        	// 
        	// timeChartRadio
        	// 
        	this.timeChartRadio.Location = new System.Drawing.Point(168, 167);
        	this.timeChartRadio.Name = "timeChartRadio";
        	this.timeChartRadio.Size = new System.Drawing.Size(104, 24);
        	this.timeChartRadio.TabIndex = 16;
        	this.timeChartRadio.Text = "Time Chart";
        	this.timeChartRadio.UseVisualStyleBackColor = true;
        	this.timeChartRadio.Click += new System.EventHandler(this.ChartRadioClick);
        	// 
        	// barChartRadio
        	// 
        	this.barChartRadio.Checked = true;
        	this.barChartRadio.Location = new System.Drawing.Point(92, 167);
        	this.barChartRadio.Name = "barChartRadio";
        	this.barChartRadio.Size = new System.Drawing.Size(70, 24);
        	this.barChartRadio.TabIndex = 15;
        	this.barChartRadio.TabStop = true;
        	this.barChartRadio.Text = "Bar Chart";
        	this.barChartRadio.UseVisualStyleBackColor = true;
        	this.barChartRadio.Click += new System.EventHandler(this.ChartRadioClick);
        	// 
        	// label3
        	// 
        	this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.label3.Location = new System.Drawing.Point(145, 87);
        	this.label3.Name = "label3";
        	this.label3.Size = new System.Drawing.Size(63, 14);
        	this.label3.TabIndex = 47;
        	this.label3.Text = "Time Frame";
        	// 
        	// label4
        	// 
        	this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.label4.Location = new System.Drawing.Point(92, 88);
        	this.label4.Name = "label4";
        	this.label4.Size = new System.Drawing.Size(46, 14);
        	this.label4.TabIndex = 46;
        	this.label4.Text = "Period";
        	// 
        	// copyDefaultIntervals
        	// 
        	this.copyDefaultIntervals.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.copyDefaultIntervals.Location = new System.Drawing.Point(156, 59);
        	this.copyDefaultIntervals.Name = "copyDefaultIntervals";
        	this.copyDefaultIntervals.Size = new System.Drawing.Size(89, 24);
        	this.copyDefaultIntervals.TabIndex = 10;
        	this.copyDefaultIntervals.Text = "Copy default";
        	this.copyDefaultIntervals.UseVisualStyleBackColor = true;
        	// 
        	// chartPeriod
        	// 
        	this.chartPeriod.Location = new System.Drawing.Point(92, 136);
        	this.chartPeriod.Name = "chartPeriod";
        	this.chartPeriod.Size = new System.Drawing.Size(46, 20);
        	this.chartPeriod.TabIndex = 13;
        	this.chartPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.chartPeriod.Leave += new System.EventHandler(this.IntervalChange);
        	// 
        	// enginePeriod
        	// 
        	this.enginePeriod.Location = new System.Drawing.Point(92, 105);
        	this.enginePeriod.Name = "enginePeriod";
        	this.enginePeriod.Size = new System.Drawing.Size(46, 20);
        	this.enginePeriod.TabIndex = 11;
        	this.enginePeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.enginePeriod.Leave += new System.EventHandler(this.IntervalChange);
        	// 
        	// defaultPeriod
        	// 
        	this.defaultPeriod.Location = new System.Drawing.Point(92, 36);
        	this.defaultPeriod.Name = "defaultPeriod";
        	this.defaultPeriod.Size = new System.Drawing.Size(46, 20);
        	this.defaultPeriod.TabIndex = 7;
        	this.defaultPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.defaultPeriod.Leave += new System.EventHandler(this.IntervalChange);
        	// 
        	// useDefaultInterval
        	// 
        	this.useDefaultInterval.Checked = true;
        	this.useDefaultInterval.CheckState = System.Windows.Forms.CheckState.Checked;
        	this.useDefaultInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.useDefaultInterval.Location = new System.Drawing.Point(27, 59);
        	this.useDefaultInterval.Name = "useDefaultInterval";
        	this.useDefaultInterval.Size = new System.Drawing.Size(123, 24);
        	this.useDefaultInterval.TabIndex = 9;
        	this.useDefaultInterval.Text = "Use this default only";
        	this.useDefaultInterval.UseVisualStyleBackColor = true;
        	// 
        	// chartBarUnit
        	// 
        	this.chartBarUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.chartBarUnit.FormattingEnabled = true;
        	this.chartBarUnit.Location = new System.Drawing.Point(145, 135);
        	this.chartBarUnit.Name = "chartBarUnit";
        	this.chartBarUnit.Size = new System.Drawing.Size(121, 21);
        	this.chartBarUnit.TabIndex = 14;
        	this.chartBarUnit.SelectedIndexChanged += new System.EventHandler(this.IntervalChange);
        	// 
        	// defaultTxt
        	// 
        	this.defaultTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.defaultTxt.Location = new System.Drawing.Point(6, 39);
        	this.defaultTxt.Name = "defaultTxt";
        	this.defaultTxt.Size = new System.Drawing.Size(80, 15);
        	this.defaultTxt.TabIndex = 29;
        	this.defaultTxt.Text = "Default";
        	// 
        	// defaultBarUnit
        	// 
        	this.defaultBarUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.defaultBarUnit.FormattingEnabled = true;
        	this.defaultBarUnit.Location = new System.Drawing.Point(145, 36);
        	this.defaultBarUnit.Name = "defaultBarUnit";
        	this.defaultBarUnit.Size = new System.Drawing.Size(121, 21);
        	this.defaultBarUnit.TabIndex = 8;
        	this.defaultBarUnit.SelectedIndexChanged += new System.EventHandler(this.IntervalChange);
        	// 
        	// chartBarsTxt
        	// 
        	this.chartBarsTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.chartBarsTxt.Location = new System.Drawing.Point(6, 139);
        	this.chartBarsTxt.Name = "chartBarsTxt";
        	this.chartBarsTxt.Size = new System.Drawing.Size(80, 20);
        	this.chartBarsTxt.TabIndex = 28;
        	this.chartBarsTxt.Text = "Chart Bars";
        	// 
        	// timeFrameTxt
        	// 
        	this.timeFrameTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.timeFrameTxt.Location = new System.Drawing.Point(145, 19);
        	this.timeFrameTxt.Name = "timeFrameTxt";
        	this.timeFrameTxt.Size = new System.Drawing.Size(63, 14);
        	this.timeFrameTxt.TabIndex = 26;
        	this.timeFrameTxt.Text = "Time Frame";
        	// 
        	// periodTxt
        	// 
        	this.periodTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.periodTxt.Location = new System.Drawing.Point(92, 20);
        	this.periodTxt.Name = "periodTxt";
        	this.periodTxt.Size = new System.Drawing.Size(46, 14);
        	this.periodTxt.TabIndex = 25;
        	this.periodTxt.Text = "Period";
        	// 
        	// disableCharting
        	// 
        	this.disableCharting.Location = new System.Drawing.Point(10, 188);
        	this.disableCharting.Name = "disableCharting";
        	this.disableCharting.Size = new System.Drawing.Size(256, 31);
        	this.disableCharting.TabIndex = 17;
        	this.disableCharting.Text = "Disable charting to run faster with less memory.";
        	this.disableCharting.UseVisualStyleBackColor = true;
        	// 
        	// geneticOptimize
        	// 
        	this.geneticOptimize.Location = new System.Drawing.Point(214, 118);
        	this.geneticOptimize.Name = "geneticOptimize";
        	this.geneticOptimize.Size = new System.Drawing.Size(81, 23);
        	this.geneticOptimize.TabIndex = 20;
        	this.geneticOptimize.Text = "Genetic";
        	this.geneticOptimize.UseVisualStyleBackColor = true;
        	// 
        	// modelLoader
        	// 
        	this.modelLoader.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.modelLoader.FormattingEnabled = true;
        	this.modelLoader.Location = new System.Drawing.Point(118, 13);
        	this.modelLoader.MaxDropDownItems = 20;
        	this.modelLoader.Name = "modelLoader";
        	this.modelLoader.Size = new System.Drawing.Size(165, 21);
        	this.modelLoader.Sorted = true;
        	this.modelLoader.TabIndex = 4;
        	// 
        	// groupBox1
        	// 
        	this.groupBox1.Controls.Add(this.label5);
        	this.groupBox1.Controls.Add(this.modelLoader);
        	this.groupBox1.Location = new System.Drawing.Point(194, 10);
        	this.groupBox1.Name = "groupBox1";
        	this.groupBox1.Size = new System.Drawing.Size(294, 47);
        	this.groupBox1.TabIndex = 30;
        	this.groupBox1.TabStop = false;
        	this.groupBox1.Text = "Load";
        	// 
        	// label5
        	// 
        	this.label5.Location = new System.Drawing.Point(36, 16);
        	this.label5.Name = "label5";
        	this.label5.Size = new System.Drawing.Size(76, 17);
        	this.label5.TabIndex = 29;
        	this.label5.Text = "Model Loader";
        	// 
        	// stopAlarm
        	// 
        	this.stopAlarm.Location = new System.Drawing.Point(194, 260);
        	this.stopAlarm.Name = "stopAlarm";
        	this.stopAlarm.Size = new System.Drawing.Size(75, 23);
        	this.stopAlarm.TabIndex = 25;
        	this.stopAlarm.Text = "Stop Alarm";
        	this.stopAlarm.UseVisualStyleBackColor = true;
        	this.stopAlarm.Visible = false;
        	this.stopAlarm.Click += new System.EventHandler(this.StopAlarmButtonClick);
        	// 
        	// alarmTimer
        	// 
        	this.alarmTimer.Interval = 2000;
        	this.alarmTimer.Tick += new System.EventHandler(this.AlarmTimerTick);
        	// 
        	// Form1
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(782, 371);
        	this.Controls.Add(this.stopAlarm);
        	this.Controls.Add(this.geneticOptimize);
        	this.Controls.Add(this.label2);
        	this.Controls.Add(this.replaySpeedTextBox);
        	this.Controls.Add(this.label1);
        	this.Controls.Add(this.breakAtBarText);
        	this.Controls.Add(this.logOutput);
        	this.Controls.Add(this.realtime);
        	this.Controls.Add(this.endLabel);
        	this.Controls.Add(this.startLabel);
        	this.Controls.Add(this.endDateTime);
        	this.Controls.Add(this.startDateTime);
        	this.Controls.Add(this.historical);
        	this.Controls.Add(this.lblProgress);
        	this.Controls.Add(this.stop);
        	this.Controls.Add(this.percentProgress);
        	this.Controls.Add(this.optimize);
        	this.Controls.Add(this.symbolList);
        	this.Controls.Add(this.symbolLabel);
        	this.Controls.Add(this.intervals);
        	this.Controls.Add(this.groupBox1);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        	this.Name = "Form1";
        	this.Text = "TickZOOM";
        	this.Load += new System.EventHandler(this.Form1Load);
        	this.Shown += new System.EventHandler(this.Form1Shown);
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
        	this.intervals.ResumeLayout(false);
        	this.intervals.PerformLayout();
        	this.groupBox1.ResumeLayout(false);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.ComboBox defaultBarUnit;
        
		public System.Windows.Forms.ComboBox DefaultBarUnit {
			get { return defaultBarUnit; }
			set { defaultBarUnit = value; }
		}
        private System.Windows.Forms.TextBox defaultPeriod;
        
		public System.Windows.Forms.TextBox DefaultPeriod {
			get { return defaultPeriod; }
			set { defaultPeriod = value; }
		}
        
        private System.Windows.Forms.CheckBox testTheAlarm;
        private System.Windows.Forms.Timer alarmTimer;
        private System.Windows.Forms.Label stopAlarmLabel;
        private System.Windows.Forms.Button stopAlarm;
        
		public System.Windows.Forms.Button StopAlarm {
			get { return stopAlarm; }
		}
        private System.Windows.Forms.CheckBox disableCharting;
        
		public System.Windows.Forms.CheckBox DisableCharting {
			get { return disableCharting; }
		}
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox modelLoader;
        
		public System.Windows.Forms.ComboBox ModelLoader {
			get { return modelLoader; }
		}
        private System.Windows.Forms.Label symbolLabel;
        
		public System.Windows.Forms.Label SymbolLabel {
			get { return symbolLabel; }
		}
        private System.Windows.Forms.TextBox symbolList;
        private System.Windows.Forms.Button geneticOptimize;
        
		public System.Windows.Forms.Button GeneticOptimize {
			get { return geneticOptimize; }
		}
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        
		public System.Windows.Forms.ComboBox EngineBarUnit {
			get { return engineBarUnit; }
		}
        private System.Windows.Forms.RadioButton timeChartRadio;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton barChartRadio;
        
		public System.Windows.Forms.RadioButton BarChartRadio {
			get { return barChartRadio; }
		}
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox copyDefaultIntervals;
        
		public System.Windows.Forms.CheckBox CopyDefaultIntervals {
			get { return copyDefaultIntervals; }
		}
        private System.Windows.Forms.TextBox enginePeriod;
        
		public System.Windows.Forms.TextBox EnginePeriod {
			get { return enginePeriod; }
		}
        private System.Windows.Forms.TextBox chartPeriod;
        
		public System.Windows.Forms.TextBox ChartPeriod {
			get { return chartPeriod; }
		}
        
        private System.Windows.Forms.ComboBox engineBarUnit;
        private System.Windows.Forms.CheckBox useDefaultInterval;
        
		public System.Windows.Forms.CheckBox UseDefaultInterval {
			get { return useDefaultInterval; }
			set { useDefaultInterval = value; }
		}
        private System.Windows.Forms.ComboBox chartBarUnit;
        
		public System.Windows.Forms.ComboBox ChartBarUnit {
			get { return chartBarUnit; }
		}
        private System.Windows.Forms.Label defaultTxt;
        private System.Windows.Forms.Label chartBarsTxt;
        private System.Windows.Forms.Label periodTxt;
        private System.Windows.Forms.Label timeFrameTxt;
        private System.Windows.Forms.Label intervalEngineTxt;
        private System.Windows.Forms.GroupBox intervals;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox replaySpeedTextBox;
        private System.Windows.Forms.TextBox breakAtBarText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox logOutput;
        private System.Windows.Forms.Button realtime;
        
		public System.Windows.Forms.Button Realtime {
			get { return realtime; }
		}
        private System.Windows.Forms.Label endLabel;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.DateTimePicker startDateTime;
        
		public System.Windows.Forms.DateTimePicker StartDateTime {
			get { return startDateTime; }
		}
        private System.Windows.Forms.DateTimePicker endDateTime;
        
		public System.Windows.Forms.DateTimePicker EndDateTime {
			get { return endDateTime; }
		}
        private System.Windows.Forms.Button optimize;
        
		public System.Windows.Forms.Button Optimize {
			get { return optimize; }
		}
        private System.Windows.Forms.Button historical;
        
		public System.Windows.Forms.Button Historical {
			get { return historical; }
		}

        #endregion

        private System.Windows.Forms.ProgressBar percentProgress;
        
		public System.Windows.Forms.ProgressBar PercentProgress {
			get { return percentProgress; }
		}
        private System.Windows.Forms.Button stop;
        
		public System.Windows.Forms.Button Stop {
			get { return stop; }
		}
        private object progressLocker = new object();
        private System.Windows.Forms.Label lblProgress;
        
		public System.Windows.Forms.Label LblProgress {
			get { return lblProgress; }
		}
        

        
        void ChartBarsCheckBoxClick(object sender, System.EventArgs e)
        {
        	UpdateCheckBoxes();
        }
        
		public System.Windows.Forms.TextBox SymbolList {
			get { return symbolList; }
		}
        
    }
}