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
using TickZoom.MBTrading;

namespace MBTest
{
    public class App : System.Windows.Forms.Form
	{
    	private static readonly Log log = Factory.Log.GetLogger(typeof(App));
        private System.Windows.Forms.Button button1;
        private IContainer components;
        private TextBox Output;
        private System.Windows.Forms.Timer timer1;
        private TextBox bids;
        private TextBox asks;
        private TextBox BidsSize;
        private TextBox AsksSize;
        private Label LastDate;
        private Label Agreg_Bid;
        private Label Agreg_Ask;
        private Label label1;
        private Label label2;
        private Label label3;
        private CheckBox checkBox1;
        private Button button3;
		MbtInterface mbt;
        delegate void SetTextCallback(string msg);
        private void Echo(string msg)
        {
            if (this.Output.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Echo);
                this.Invoke(d, new object[] { msg });
            }
            else
            {
                Output.Text += msg + "\r\n";
                Output.SelectionStart = Output.Text.Length;
                Output.ScrollToCaret();
            }
        }
        private void ProcessMessages(Log log)
        {
            while (true)
            {
            	try {
		        	Echo(log.ReadLine());
				} catch( Exception ex) {
            		log.Error(ex.Message,ex);
					break;
				}	        	
            }
        }
        public App()
		{
			InitializeComponent();
			ConfigurationSettings.AppSettings["LogFile"] = "QuotesUI.log";
			
			StartEchoThread(log);
		}

        private void StartEchoThread(Log log) {
        	Thread thread;
			ThreadStart ts = delegate()
			{
				ProcessMessages(log);
			};
            thread = new Thread(ts);
            thread.Priority = ThreadPriority.Lowest;
            thread.Name = log.FileName+"ToScreen";
            thread.IsBackground = true;
            thread.Start();
        }
        
		protected override void Dispose( bool disposing )
		{
            if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
				mbt.Stop(null);
			}
			base.Dispose( disposing );
		}
		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button1 = new System.Windows.Forms.Button();
			this.Output = new System.Windows.Forms.TextBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.bids = new System.Windows.Forms.TextBox();
			this.asks = new System.Windows.Forms.TextBox();
			this.BidsSize = new System.Windows.Forms.TextBox();
			this.AsksSize = new System.Windows.Forms.TextBox();
			this.LastDate = new System.Windows.Forms.Label();
			this.Agreg_Bid = new System.Windows.Forms.Label();
			this.Agreg_Ask = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.bidPriceDepth = new System.Windows.Forms.Label();
			this.askPriceDepth = new System.Windows.Forms.Label();
			this.disconnectQuotesButton = new System.Windows.Forms.Button();
			this.disconnectOrdersButton = new System.Windows.Forms.Button();
			this.disconnectReaders = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 21);
			this.button1.TabIndex = 0;
			this.button1.Text = "Restart";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Output
			// 
			this.Output.Location = new System.Drawing.Point(15, 228);
			this.Output.Multiline = true;
			this.Output.Name = "Output";
			this.Output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.Output.Size = new System.Drawing.Size(759, 231);
			this.Output.TabIndex = 3;
			// 
			// timer1
			// 
			this.timer1.Interval = 500;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// bids
			// 
			this.bids.Location = new System.Drawing.Point(240, 64);
			this.bids.Multiline = true;
			this.bids.Name = "bids";
			this.bids.Size = new System.Drawing.Size(61, 147);
			this.bids.TabIndex = 7;
			// 
			// asks
			// 
			this.asks.Location = new System.Drawing.Point(302, 64);
			this.asks.Multiline = true;
			this.asks.Name = "asks";
			this.asks.Size = new System.Drawing.Size(61, 147);
			this.asks.TabIndex = 8;
			// 
			// BidsSize
			// 
			this.BidsSize.Location = new System.Drawing.Point(200, 64);
			this.BidsSize.Multiline = true;
			this.BidsSize.Name = "BidsSize";
			this.BidsSize.Size = new System.Drawing.Size(41, 147);
			this.BidsSize.TabIndex = 9;
			this.BidsSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AsksSize
			// 
			this.AsksSize.Location = new System.Drawing.Point(362, 64);
			this.AsksSize.Multiline = true;
			this.AsksSize.Name = "AsksSize";
			this.AsksSize.Size = new System.Drawing.Size(41, 147);
			this.AsksSize.TabIndex = 11;
			// 
			// LastDate
			// 
			this.LastDate.AutoSize = true;
			this.LastDate.Location = new System.Drawing.Point(12, 111);
			this.LastDate.Name = "LastDate";
			this.LastDate.Size = new System.Drawing.Size(0, 13);
			this.LastDate.TabIndex = 13;
			// 
			// Agreg_Bid
			// 
			this.Agreg_Bid.AutoSize = true;
			this.Agreg_Bid.Location = new System.Drawing.Point(256, 44);
			this.Agreg_Bid.Name = "Agreg_Bid";
			this.Agreg_Bid.Size = new System.Drawing.Size(35, 13);
			this.Agreg_Bid.TabIndex = 14;
			this.Agreg_Bid.Text = "label1";
			// 
			// Agreg_Ask
			// 
			this.Agreg_Ask.AutoSize = true;
			this.Agreg_Ask.Location = new System.Drawing.Point(318, 44);
			this.Agreg_Ask.Name = "Agreg_Ask";
			this.Agreg_Ask.Size = new System.Drawing.Size(35, 13);
			this.Agreg_Ask.TabIndex = 15;
			this.Agreg_Ask.Text = "label2";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(12, 44);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 16;
			this.button3.Text = "Backfill";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 152);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 17;
			this.label1.Text = "Perms:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 165);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(41, 13);
			this.label2.TabIndex = 18;
			this.label2.Text = "Orders:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 178);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 13);
			this.label3.TabIndex = 19;
			this.label3.Text = "Quotes";
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(92, 12);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(79, 17);
			this.checkBox1.TabIndex = 20;
			this.checkBox1.Text = "Switch Pair";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// bidPriceDepth
			// 
			this.bidPriceDepth.Location = new System.Drawing.Point(256, 16);
			this.bidPriceDepth.Name = "bidPriceDepth";
			this.bidPriceDepth.Size = new System.Drawing.Size(45, 17);
			this.bidPriceDepth.TabIndex = 21;
			this.bidPriceDepth.Text = "label4";
			// 
			// askPriceDepth
			// 
			this.askPriceDepth.Location = new System.Drawing.Point(318, 16);
			this.askPriceDepth.Name = "askPriceDepth";
			this.askPriceDepth.Size = new System.Drawing.Size(45, 17);
			this.askPriceDepth.TabIndex = 22;
			this.askPriceDepth.Text = "label5";
			// 
			// disconnectQuotesButton
			// 
			this.disconnectQuotesButton.Location = new System.Drawing.Point(560, 64);
			this.disconnectQuotesButton.Name = "disconnectQuotesButton";
			this.disconnectQuotesButton.Size = new System.Drawing.Size(118, 23);
			this.disconnectQuotesButton.TabIndex = 23;
			this.disconnectQuotesButton.Text = "Disconnect Quotes";
			this.disconnectQuotesButton.UseVisualStyleBackColor = true;
			this.disconnectQuotesButton.Click += new System.EventHandler(this.DisconnectQuotesButtonClick);
			// 
			// disconnectOrdersButton
			// 
			this.disconnectOrdersButton.Location = new System.Drawing.Point(560, 93);
			this.disconnectOrdersButton.Name = "disconnectOrdersButton";
			this.disconnectOrdersButton.Size = new System.Drawing.Size(118, 23);
			this.disconnectOrdersButton.TabIndex = 24;
			this.disconnectOrdersButton.Text = "Disconnect Orders";
			this.disconnectOrdersButton.UseVisualStyleBackColor = true;
			this.disconnectOrdersButton.Click += new System.EventHandler(this.DisconnectOrdersButtonClick);
			// 
			// disconnectReaders
			// 
			this.disconnectReaders.Location = new System.Drawing.Point(560, 122);
			this.disconnectReaders.Name = "disconnectReaders";
			this.disconnectReaders.Size = new System.Drawing.Size(118, 23);
			this.disconnectReaders.TabIndex = 25;
			this.disconnectReaders.Text = "Disconnect Readers";
			this.disconnectReaders.UseVisualStyleBackColor = true;
			this.disconnectReaders.Click += new System.EventHandler(this.DisconnectReadersClick);
			// 
			// App
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(786, 471);
			this.Controls.Add(this.disconnectReaders);
			this.Controls.Add(this.disconnectOrdersButton);
			this.Controls.Add(this.disconnectQuotesButton);
			this.Controls.Add(this.askPriceDepth);
			this.Controls.Add(this.bidPriceDepth);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.Agreg_Ask);
			this.Controls.Add(this.Agreg_Bid);
			this.Controls.Add(this.LastDate);
			this.Controls.Add(this.AsksSize);
			this.Controls.Add(this.BidsSize);
			this.Controls.Add(this.asks);
			this.Controls.Add(this.bids);
			this.Controls.Add(this.Output);
			this.Controls.Add(this.button1);
			this.Name = "App";
			this.Text = "Level II";
			this.Shown += new System.EventHandler(this.App_Shown);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AppFormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Button disconnectReaders;
		private System.Windows.Forms.Button disconnectOrdersButton;
		private System.Windows.Forms.Button disconnectQuotesButton;
		private System.Windows.Forms.Label askPriceDepth;
		private System.Windows.Forms.Label bidPriceDepth;
		#endregion
        int pairIndex = 0;
		private void button1_Click(object sender, System.EventArgs e)
		{
            Application.Restart();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mbt!=null )
            {
                label1.Text = "Perms: " + mbt.PermsHealth;
                label2.Text = "Orders: " + mbt.OrdersHealth;
                label3.Text = "Quotes: " + mbt.QuotesHealth;
                InstrumentReader reader = null;
                if (mbt.InstrumentReaders.Readers.Count > pairIndex) {
	                reader = mbt.InstrumentReaders.Readers[pairIndex];
                }
                if (mbt.InstrumentReaders.Readers.Count > pairIndex && reader.Advised) {
                    LastDate.Text = reader.LastBid.ToString() + "/" + reader.LastAsk.ToString();
//                    LastDate.Text = "Wayne: " + MBTInterface.InstrumentReaders.Readers[pairIndex].Level2Bids.Count.ToString() + "/" + MBTInterface.InstrumentReaders.Readers[pairIndex].Level2Asks.Count.ToString();
                    lock( reader.level2Locker) {
	                    //
	                    //TickConsole.bidask; //TickConsole.lastTickDate.ToLocalTime().ToLongTimeString();
	                    if (reader.Level2Bids.Count > 0)
	                    {
	                        double size = 0;
	                        reader.Level2Bids.UpdateDepth();
	                        ushort[] depthBidSizes = reader.Level2Bids.DepthSizes;
	                        long[] depthBidPrices = reader.Level2Bids.DepthPrices;
	                        string[] bidValues = new string[Math.Min(15, depthBidSizes.Length)];
	                        string[] bidSizes = new string[Math.Min(15, depthBidPrices.Length)];
	                        for (int i = 0; i < bidSizes.Length && i < reader.Level2Bids.Count; i++)
	                        {
	                        	bidValues[i] = depthBidPrices[i].ToString();
//	                        	bidValues[i] = mbt.InstrumentReaders.items[pairIndex].Level2Bids[i].dPrice.ToString();
	                            bidSizes[i] = depthBidSizes[i].ToString();
	                            size += depthBidSizes[i];
	                        }
	                        bids.Lines = bidValues;
	                        BidsSize.Lines = bidSizes;
	                        Agreg_Bid.Text = size.ToString();
	                        bidPriceDepth.Text = reader.Level2Bids.AveragePrice().ToString();
	                    }
	                    if (reader.Level2Asks.Count > 0)
	                    {
	                        double size = 0;
	                        reader.Level2Asks.UpdateDepth();
	                        ushort[] depthAskSizes = mbt.InstrumentReaders.Readers[pairIndex].Level2Asks.DepthSizes;
	                        long[] depthAskPrices = mbt.InstrumentReaders.Readers[pairIndex].Level2Asks.DepthPrices;
	                        string[] askValues = new string[Math.Min(15, depthAskPrices.Length)];
	                        string[] askSizes = new string[Math.Min(15, depthAskSizes.Length)];
	                        for (int i = 0; i < askSizes.Length && i < mbt.InstrumentReaders.Readers[pairIndex].Level2Asks.Count; i++)
	                        {
	                            askValues[i] = depthAskPrices[i].ToString();
//	                        	askValues[i] = mbt.InstrumentReaders.items[pairIndex].Level2Asks[i].dPrice.ToString();
	                            askSizes[i] = depthAskSizes[i].ToString();
	                            size += depthAskSizes[i];
	                        }
	                        asks.Lines = askValues;
	                        AsksSize.Lines = askSizes;
	                        Agreg_Ask.Text = size.ToString();
	                        askPriceDepth.Text = mbt.InstrumentReaders.Readers[pairIndex].Level2Asks.AveragePrice().ToString();
	                    }
                    }
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //HistoryBackfill hist = new HistoryBackfill();
            //hist.SyncDB("EUR/USD","60");
        }
        [STAThread]
        static void Main()
        {
                Application.Run(new App());
        }
		
		private void App_Shown(object sender, EventArgs e)
        {
            int iMaxWait = 2500; //Wait for another instance to finish unloading if there was one
			int dwStart = Environment.TickCount;
            while (true) {Application.DoEvents();
				if (Environment.TickCount > dwStart + iMaxWait) break;}
            if (mbt==null)
            {
	    		
                try
                {
                	StubReceiver receiver = new StubReceiver();
                	mbt = new MbtInterface();
                	mbt.Start(receiver);
                	SymbolInfo symbol = Factory.Symbol.LookupSymbol("MSFT");
                	StartSymbolDetail detail = new StartSymbolDetail();
                	detail.LastTime = new TimeStamp(DateTime.Now);
                	mbt.StartSymbol(receiver,symbol,detail);
                }
                catch (Exception problem)
                {
                    log.Notice(problem.Message,problem);
                }
                timer1.Enabled = true;
            }

        }
		
		void DisconnectQuotesButtonClick(object sender, EventArgs e)
		{
			mbt.QuotesDisconnect();
		}
		
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) pairIndex = 1; else pairIndex = 0;
        }
		
		void DisconnectReadersClick(object sender, EventArgs e)
		{
			mbt.ReadersDisconnect();			
		}
		
        void AppFormClosing(object sender, FormClosingEventArgs e)
		{
		}
		
		void DisconnectOrdersButtonClick(object sender, EventArgs e)
		{
			mbt.OrdersDisconnect();
		}
        
    }

    public class StubReceiver : Receiver {
    	
    	public bool CanReceive(SymbolInfo symbol) {
			return true;
		}
    	
		public ReceiverState ReceiverState {
			get {
				return ReceiverState.Historical;
			}
		}
    	
		public void OnHistorical(SymbolInfo symbol)
		{
			
		}
    	
		public void OnEndHistorical(SymbolInfo symbol)
		{
			
		}
    	
		public void OnRealTime(SymbolInfo symbol)
		{
			
		}
    	
		public void OnEndRealTime(SymbolInfo symbol)
		{
			
		}
    	
		public void OnSend(ref TickBinary o)
		{
			
		}
    	
		public void OnStop()
		{
			
		}
    	
		public void OnError(string error)
		{
			
		}
    	
		public void OnPositionChange(SymbolInfo symbol, LogicalFillBinary fill)
		{
		}
    	
		public ReceiverState OnGetReceiverState(SymbolInfo symbol)
		{
			return ReceiverState.Ready;
		}
		
		public bool OnEvent( SymbolInfo symbol, int eventType, object eventDetail) {
			return true;
		}
		
		private bool isDisposed;
	    public void Dispose() 
	    {
	        Dispose(true);
	        GC.SuppressFinalize(this);      
	    }
	
	    protected virtual void Dispose(bool disposing)
	    {
       		if( !isDisposed) {
	            isDisposed = true;   
	            if (disposing) {
	            	//
	            }
    		}
	    }
	    
    }
}
