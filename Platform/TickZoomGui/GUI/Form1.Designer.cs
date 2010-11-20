namespace TickZoom
{
    partial class Form1
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
        	this.lblSymbol = new System.Windows.Forms.Label();
        	this.txtSymbol = new System.Windows.Forms.TextBox();
        	this.btnOptimize = new System.Windows.Forms.Button();
        	this.prgExecute = new System.Windows.Forms.ProgressBar();
        	this.btnStop = new System.Windows.Forms.Button();
        	this.lblProgress = new System.Windows.Forms.Label();
        	this.btnRun = new System.Windows.Forms.Button();
        	this.startTimePicker = new System.Windows.Forms.DateTimePicker();
        	this.endTimePicker = new System.Windows.Forms.DateTimePicker();
        	this.startLabel = new System.Windows.Forms.Label();
        	this.endLabel = new System.Windows.Forms.Label();
        	this.liveButton = new System.Windows.Forms.Button();
        	this.logOutput = new System.Windows.Forms.TextBox();
        	this.breakAtBarText = new System.Windows.Forms.TextBox();
        	this.label1 = new System.Windows.Forms.Label();
        	this.replaySpeedTextBox = new System.Windows.Forms.TextBox();
        	this.label2 = new System.Windows.Forms.Label();
        	this.intervalEngineTxt = new System.Windows.Forms.Label();
        	this.engineBarsCombo = new System.Windows.Forms.ComboBox();
        	this.intervals = new System.Windows.Forms.GroupBox();
        	this.testTheAlarm = new System.Windows.Forms.CheckBox();
        	this.stopAlarmLabel = new System.Windows.Forms.Label();
        	this.disableChartsCheckBox = new System.Windows.Forms.CheckBox();
        	this.label7 = new System.Windows.Forms.Label();
        	this.timeChartRadio = new System.Windows.Forms.RadioButton();
        	this.barChartRadio = new System.Windows.Forms.RadioButton();
        	this.label3 = new System.Windows.Forms.Label();
        	this.label4 = new System.Windows.Forms.Label();
        	this.copyDebugCheckBox = new System.Windows.Forms.CheckBox();
        	this.chartBarsBox = new System.Windows.Forms.TextBox();
        	this.engineBarsBox = new System.Windows.Forms.TextBox();
        	this.defaultBox = new System.Windows.Forms.TextBox();
        	this.defaultOnly = new System.Windows.Forms.CheckBox();
        	this.chartBarsCombo = new System.Windows.Forms.ComboBox();
        	this.defaultTxt = new System.Windows.Forms.Label();
        	this.defaultCombo = new System.Windows.Forms.ComboBox();
        	this.chartBarsTxt = new System.Windows.Forms.Label();
        	this.timeFrameTxt = new System.Windows.Forms.Label();
        	this.periodTxt = new System.Windows.Forms.Label();
        	this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
        	this.btnGenetic = new System.Windows.Forms.Button();
        	this.modelLoaderBox = new System.Windows.Forms.ComboBox();
        	this.groupBox1 = new System.Windows.Forms.GroupBox();
        	this.label5 = new System.Windows.Forms.Label();
        	this.stopAlarmButton = new System.Windows.Forms.Button();
        	this.alarmTimer = new System.Windows.Forms.Timer(this.components);
        	this.intervals.SuspendLayout();
        	this.groupBox1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// lblSymbol
        	// 
        	this.lblSymbol.AutoSize = true;
        	this.lblSymbol.Location = new System.Drawing.Point(13, 16);
        	this.lblSymbol.Name = "lblSymbol";
        	this.lblSymbol.Size = new System.Drawing.Size(41, 13);
        	this.lblSymbol.TabIndex = 0;
        	this.lblSymbol.Text = "Symbol";
        	// 
        	// txtSymbol
        	// 
        	this.txtSymbol.Location = new System.Drawing.Point(78, 13);
        	this.txtSymbol.Name = "txtSymbol";
        	this.txtSymbol.Size = new System.Drawing.Size(110, 20);
        	this.txtSymbol.TabIndex = 4;
        	this.txtSymbol.Text = "USD/JPY";
        	// 
        	// btnOptimize
        	// 
        	this.btnOptimize.Location = new System.Drawing.Point(110, 118);
        	this.btnOptimize.Name = "btnOptimize";
        	this.btnOptimize.Size = new System.Drawing.Size(78, 23);
        	this.btnOptimize.TabIndex = 6;
        	this.btnOptimize.Text = "Optimize";
        	this.btnOptimize.UseVisualStyleBackColor = true;
        	this.btnOptimize.Click += new System.EventHandler(this.btnOptimize_Click);
        	// 
        	// prgExecute
        	// 
        	this.prgExecute.Location = new System.Drawing.Point(12, 169);
        	this.prgExecute.Name = "prgExecute";
        	this.prgExecute.Size = new System.Drawing.Size(475, 23);
        	this.prgExecute.TabIndex = 8;
        	// 
        	// btnStop
        	// 
        	this.btnStop.Location = new System.Drawing.Point(413, 118);
        	this.btnStop.Name = "btnStop";
        	this.btnStop.Size = new System.Drawing.Size(75, 23);
        	this.btnStop.TabIndex = 8;
        	this.btnStop.Text = "Stop";
        	this.btnStop.UseVisualStyleBackColor = true;
        	this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
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
        	// btnRun
        	// 
        	this.btnRun.Location = new System.Drawing.Point(13, 118);
        	this.btnRun.Name = "btnRun";
        	this.btnRun.Size = new System.Drawing.Size(75, 23);
        	this.btnRun.TabIndex = 5;
        	this.btnRun.Text = "Start Run";
        	this.btnRun.UseVisualStyleBackColor = true;
        	this.btnRun.Click += new System.EventHandler(this.HistoricalButtonClick);
        	// 
        	// startTimePicker
        	// 
        	this.startTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        	this.startTimePicker.Location = new System.Drawing.Point(78, 51);
        	this.startTimePicker.Name = "startTimePicker";
        	this.startTimePicker.Size = new System.Drawing.Size(110, 20);
        	this.startTimePicker.TabIndex = 1;
        	this.startTimePicker.CloseUp += new System.EventHandler(this.StartTimePickerCloseUp);
        	// 
        	// endTimePicker
        	// 
        	this.endTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        	this.endTimePicker.Location = new System.Drawing.Point(78, 91);
        	this.endTimePicker.Name = "endTimePicker";
        	this.endTimePicker.Size = new System.Drawing.Size(110, 20);
        	this.endTimePicker.TabIndex = 2;
        	this.endTimePicker.CloseUp += new System.EventHandler(this.EndTimePickerCloseUp);
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
        	// liveButton
        	// 
        	this.liveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.liveButton.Location = new System.Drawing.Point(318, 118);
        	this.liveButton.Name = "liveButton";
        	this.liveButton.Size = new System.Drawing.Size(74, 23);
        	this.liveButton.TabIndex = 16;
        	this.liveButton.Text = "Real Time";
        	this.liveButton.UseVisualStyleBackColor = true;
        	this.liveButton.Click += new System.EventHandler(this.RealTimeButtonClick);
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
        	this.breakAtBarText.TabIndex = 18;
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
        	this.replaySpeedTextBox.TabIndex = 20;
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
        	// engineBarsCombo
        	// 
        	this.engineBarsCombo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.engineBarsCombo.FormattingEnabled = true;
        	this.engineBarsCombo.Location = new System.Drawing.Point(145, 105);
        	this.engineBarsCombo.Name = "engineBarsCombo";
        	this.engineBarsCombo.Size = new System.Drawing.Size(121, 21);
        	this.engineBarsCombo.TabIndex = 23;
        	this.engineBarsCombo.SelectedIndexChanged += new System.EventHandler(this.IntervalChange);
        	// 
        	// intervals
        	// 
        	this.intervals.Controls.Add(this.testTheAlarm);
        	this.intervals.Controls.Add(this.stopAlarmLabel);
        	this.intervals.Controls.Add(this.disableChartsCheckBox);
        	this.intervals.Controls.Add(this.label7);
        	this.intervals.Controls.Add(this.timeChartRadio);
        	this.intervals.Controls.Add(this.barChartRadio);
        	this.intervals.Controls.Add(this.label3);
        	this.intervals.Controls.Add(this.label4);
        	this.intervals.Controls.Add(this.copyDebugCheckBox);
        	this.intervals.Controls.Add(this.chartBarsBox);
        	this.intervals.Controls.Add(this.engineBarsBox);
        	this.intervals.Controls.Add(this.defaultBox);
        	this.intervals.Controls.Add(this.defaultOnly);
        	this.intervals.Controls.Add(this.chartBarsCombo);
        	this.intervals.Controls.Add(this.defaultTxt);
        	this.intervals.Controls.Add(this.defaultCombo);
        	this.intervals.Controls.Add(this.chartBarsTxt);
        	this.intervals.Controls.Add(this.timeFrameTxt);
        	this.intervals.Controls.Add(this.periodTxt);
        	this.intervals.Controls.Add(this.intervalEngineTxt);
        	this.intervals.Controls.Add(this.engineBarsCombo);
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
        	this.testTheAlarm.TabIndex = 55;
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
        	// disableChartsCheckBox
        	// 
        	this.disableChartsCheckBox.Location = new System.Drawing.Point(10, 188);
        	this.disableChartsCheckBox.Name = "disableChartsCheckBox";
        	this.disableChartsCheckBox.Size = new System.Drawing.Size(256, 31);
        	this.disableChartsCheckBox.TabIndex = 53;
        	this.disableChartsCheckBox.Text = "Disable charting to run faster with less memory.";
        	this.disableChartsCheckBox.UseVisualStyleBackColor = true;
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
        	this.timeChartRadio.TabIndex = 51;
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
        	this.barChartRadio.TabIndex = 50;
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
        	// copyDebugCheckBox
        	// 
        	this.copyDebugCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.copyDebugCheckBox.Location = new System.Drawing.Point(156, 59);
        	this.copyDebugCheckBox.Name = "copyDebugCheckBox";
        	this.copyDebugCheckBox.Size = new System.Drawing.Size(89, 24);
        	this.copyDebugCheckBox.TabIndex = 45;
        	this.copyDebugCheckBox.Text = "Copy default";
        	this.copyDebugCheckBox.UseVisualStyleBackColor = true;
        	this.copyDebugCheckBox.Click += new System.EventHandler(this.CopyDebugCheckBoxClick);
        	// 
        	// chartBarsBox
        	// 
        	this.chartBarsBox.Location = new System.Drawing.Point(92, 136);
        	this.chartBarsBox.Name = "chartBarsBox";
        	this.chartBarsBox.Size = new System.Drawing.Size(46, 20);
        	this.chartBarsBox.TabIndex = 43;
        	this.chartBarsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.chartBarsBox.Leave += new System.EventHandler(this.IntervalChange);
        	// 
        	// engineBarsBox
        	// 
        	this.engineBarsBox.Location = new System.Drawing.Point(92, 105);
        	this.engineBarsBox.Name = "engineBarsBox";
        	this.engineBarsBox.Size = new System.Drawing.Size(46, 20);
        	this.engineBarsBox.TabIndex = 41;
        	this.engineBarsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.engineBarsBox.Leave += new System.EventHandler(this.IntervalChange);
        	// 
        	// defaultBox
        	// 
        	this.defaultBox.Location = new System.Drawing.Point(92, 36);
        	this.defaultBox.Name = "defaultBox";
        	this.defaultBox.Size = new System.Drawing.Size(46, 20);
        	this.defaultBox.TabIndex = 40;
        	this.defaultBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.defaultBox.Leave += new System.EventHandler(this.IntervalChange);
        	// 
        	// defaultOnly
        	// 
        	this.defaultOnly.Checked = true;
        	this.defaultOnly.CheckState = System.Windows.Forms.CheckState.Checked;
        	this.defaultOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.defaultOnly.Location = new System.Drawing.Point(27, 59);
        	this.defaultOnly.Name = "defaultOnly";
        	this.defaultOnly.Size = new System.Drawing.Size(123, 24);
        	this.defaultOnly.TabIndex = 39;
        	this.defaultOnly.Text = "Use this default only";
        	this.defaultOnly.UseVisualStyleBackColor = true;
        	this.defaultOnly.Click += new System.EventHandler(this.DefaultOnlyClick);
        	// 
        	// chartBarsCombo
        	// 
        	this.chartBarsCombo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.chartBarsCombo.FormattingEnabled = true;
        	this.chartBarsCombo.Location = new System.Drawing.Point(145, 135);
        	this.chartBarsCombo.Name = "chartBarsCombo";
        	this.chartBarsCombo.Size = new System.Drawing.Size(121, 21);
        	this.chartBarsCombo.TabIndex = 34;
        	this.chartBarsCombo.SelectedIndexChanged += new System.EventHandler(this.IntervalChange);
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
        	// defaultCombo
        	// 
        	this.defaultCombo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.defaultCombo.FormattingEnabled = true;
        	this.defaultCombo.Location = new System.Drawing.Point(145, 36);
        	this.defaultCombo.Name = "defaultCombo";
        	this.defaultCombo.Size = new System.Drawing.Size(121, 21);
        	this.defaultCombo.TabIndex = 30;
        	this.defaultCombo.SelectedIndexChanged += new System.EventHandler(this.IntervalChange);
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
        	// btnGenetic
        	// 
        	this.btnGenetic.Location = new System.Drawing.Point(214, 118);
        	this.btnGenetic.Name = "btnGenetic";
        	this.btnGenetic.Size = new System.Drawing.Size(81, 23);
        	this.btnGenetic.TabIndex = 26;
        	this.btnGenetic.Text = "Genetic";
        	this.btnGenetic.UseVisualStyleBackColor = true;
        	this.btnGenetic.Click += new System.EventHandler(this.BtnGeneticClick);
        	// 
        	// modelLoaderBox
        	// 
        	this.modelLoaderBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.modelLoaderBox.FormattingEnabled = true;
        	this.modelLoaderBox.Location = new System.Drawing.Point(118, 13);
        	this.modelLoaderBox.MaxDropDownItems = 20;
        	this.modelLoaderBox.Name = "modelLoaderBox";
        	this.modelLoaderBox.Size = new System.Drawing.Size(165, 21);
        	this.modelLoaderBox.Sorted = true;
        	this.modelLoaderBox.TabIndex = 28;
        	this.modelLoaderBox.SelectedIndexChanged += new System.EventHandler(this.ModelLoaderBoxSelectedIndexChanged);
        	// 
        	// groupBox1
        	// 
        	this.groupBox1.Controls.Add(this.label5);
        	this.groupBox1.Controls.Add(this.modelLoaderBox);
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
        	// stopAlarmButton
        	// 
        	this.stopAlarmButton.Location = new System.Drawing.Point(194, 260);
        	this.stopAlarmButton.Name = "stopAlarmButton";
        	this.stopAlarmButton.Size = new System.Drawing.Size(75, 23);
        	this.stopAlarmButton.TabIndex = 31;
        	this.stopAlarmButton.Text = "Stop Alarm";
        	this.stopAlarmButton.UseVisualStyleBackColor = true;
        	this.stopAlarmButton.Visible = false;
        	this.stopAlarmButton.Click += new System.EventHandler(this.StopAlarmButtonClick);
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
        	this.Controls.Add(this.stopAlarmButton);
        	this.Controls.Add(this.btnGenetic);
        	this.Controls.Add(this.label2);
        	this.Controls.Add(this.replaySpeedTextBox);
        	this.Controls.Add(this.label1);
        	this.Controls.Add(this.breakAtBarText);
        	this.Controls.Add(this.logOutput);
        	this.Controls.Add(this.liveButton);
        	this.Controls.Add(this.endLabel);
        	this.Controls.Add(this.startLabel);
        	this.Controls.Add(this.endTimePicker);
        	this.Controls.Add(this.startTimePicker);
        	this.Controls.Add(this.btnRun);
        	this.Controls.Add(this.lblProgress);
        	this.Controls.Add(this.btnStop);
        	this.Controls.Add(this.prgExecute);
        	this.Controls.Add(this.btnOptimize);
        	this.Controls.Add(this.txtSymbol);
        	this.Controls.Add(this.lblSymbol);
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
        private System.Windows.Forms.CheckBox testTheAlarm;
        private System.Windows.Forms.Timer alarmTimer;
        private System.Windows.Forms.Label stopAlarmLabel;
        private System.Windows.Forms.Button stopAlarmButton;
        private System.Windows.Forms.CheckBox disableChartsCheckBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox modelLoaderBox;
        
		public System.Windows.Forms.ComboBox ModelLoaderBox {
			get { return modelLoaderBox; }
		}
        private System.Windows.Forms.Label lblSymbol;
        private System.Windows.Forms.TextBox txtSymbol;
        private System.Windows.Forms.Button btnGenetic;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        
		public System.Windows.Forms.ComboBox EngineBarsCombo {
			get { return engineBarsCombo; }
		}
        private System.Windows.Forms.RadioButton timeChartRadio;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton barChartRadio;
        
		public System.Windows.Forms.RadioButton BarChartRadio {
			get { return barChartRadio; }
		}
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox copyDebugCheckBox;
        private System.Windows.Forms.TextBox defaultBox;
        private System.Windows.Forms.TextBox engineBarsBox;
        
		public System.Windows.Forms.TextBox EngineBarsBox {
			get { return engineBarsBox; }
		}
        private System.Windows.Forms.TextBox chartBarsBox;
        
		public System.Windows.Forms.TextBox ChartBarsBox {
			get { return chartBarsBox; }
		}
        
        private System.Windows.Forms.ComboBox engineBarsCombo;
        private System.Windows.Forms.CheckBox defaultOnly;
        private System.Windows.Forms.ComboBox chartBarsCombo;
        
		public System.Windows.Forms.ComboBox ChartBarsCombo {
			get { return chartBarsCombo; }
		}
        private System.Windows.Forms.ComboBox defaultCombo;
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
        private System.Windows.Forms.Button liveButton;
        private System.Windows.Forms.Label endLabel;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.DateTimePicker startTimePicker;
        
		public System.Windows.Forms.DateTimePicker StartTimePicker {
			get { return startTimePicker; }
		}
        private System.Windows.Forms.DateTimePicker endTimePicker;
        
		public System.Windows.Forms.DateTimePicker EndTimePicker {
			get { return endTimePicker; }
		}
        private System.Windows.Forms.Button btnOptimize;
        private System.Windows.Forms.Button btnRun;

        #endregion

        private System.Windows.Forms.ProgressBar prgExecute;
        
		public System.Windows.Forms.ProgressBar PrgExecute {
			get { return prgExecute; }
		}
        private System.Windows.Forms.Button btnStop;
        private object progressLocker = new object();
        private System.Windows.Forms.Label lblProgress;
        
		public System.Windows.Forms.Label LblProgress {
			get { return lblProgress; }
		}
        

        
        void ChartBarsCheckBoxClick(object sender, System.EventArgs e)
        {
        	UpdateCheckBoxes();
        }
        
		public System.Windows.Forms.TextBox TxtSymbol {
			get { return txtSymbol; }
		}
        
		public System.Windows.Forms.ComboBox DefaultCombo {
			get { return defaultCombo; }
		}
        
		public System.Windows.Forms.TextBox DefaultBox {
			get { return defaultBox; }
		}
    }
    
}

