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
using System.Drawing;
using System.Windows.Forms;

using TickZoom.Api;
using WeifenLuo.WinFormsUI.Docking;

namespace TickZoom
{
	/// <summary>
	/// Description of PortfolioDoc.
	/// </summary>
	public partial class ProjectDoc : DockContent
	{
		Log log;
		RunManager runManager;
		MainForm mainForm;
	    private delegate void DisplayChartDelegate();
	    private delegate Chart CreateChartDelegate();
		
		public ProjectDoc()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			mainForm = MainForm.Instance;
			InitializeComponent();
		}
		
		
		void ProjectDocLoad(object sender, System.EventArgs e)
		{
			if( !DesignMode) {
				log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
				runManager = new RunManager(this);
				this.simulationMenuItem.Click += new System.EventHandler(runManager.SimulationMenuItemClick);
			}
			if( this.tabControl.SelectedTab == this.tabPage2) {
				chartControl.Size = new Size( tabControl.ClientRectangle.Width, tabControl.ClientRectangle.Height);
				chartControl.ChartResize(sender,e);
			}
		}
		
		void ProjectDocResize(object sender, EventArgs e)
		{
			tabControl.Size = new Size( ClientRectangle.Width, ClientRectangle.Height);
			if( this.tabControl.SelectedTab == this.tabPage2) {
				portfolioControl.Size = new Size( tabControl.ClientRectangle.Width, tabControl.ClientRectangle.Height);
			}
			if( this.tabControl.SelectedTab == this.tabPage2) {
				chartControl.Size = new Size( tabControl.ClientRectangle.Width, tabControl.ClientRectangle.Height);
				chartControl.ChartResize(sender, e);
			}
		}
		
        public Chart CreateChart()
        {
        	Chart chart = null;
        	try {
        		chart = (Chart) Invoke(new CreateChartDelegate(CreateChartPrivate));
        	} catch( Exception ex) {
        		log.Notice(ex.ToString());
        	}
        	return chart;
        }
        
        public void DisplayChart()
        {
        	try {
	        	Invoke(new DisplayChartDelegate(DisplayChartPrivate));
        	} catch( Exception ex) {
        		log.Notice(ex.ToString());
        	}
        }

        public void ClearChart() {
        	if( this.chartControl!=null) {
				this.chartControl = new TickZoom.ChartControl();
        	}
        }
        
        private Chart CreateChartPrivate() {
			return this.chartControl;
        }
        
        private void DisplayChartPrivate() {
        	if( this.tabPage2 == null) {
				this.tabPage2 = new System.Windows.Forms.TabPage();
        	}
			this.tabPage2.SuspendLayout();
			this.tabPage2.Controls.Clear();
			this.tabPage2.Controls.Add(this.chartControl);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = this.tabControl.ClientSize;
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Chart";
			this.tabPage2.UseVisualStyleBackColor = true;
			this.tabPage2.ResumeLayout(true);
        	if( !this.tabControl.Controls.Contains(this.tabPage2)) {
				this.tabControl.Controls.Add(this.tabPage2);
        	}
			this.tabControl.SelectTab(this.tabPage2);
			this.chartControl.Show();
			this.chartControl.Invalidate();
		}
		
	    protected override CreateParams CreateParams {
			get {
			CreateParams cp = base.CreateParams;
			cp.ExStyle |= 0x02000000;
			return cp;
			}
	    } 
		
        void ProjectDocClosing(object sender, FormClosingEventArgs e)
        {
        	runManager.Close();
        }
        
		public ChartControl ChartControl {
			get { return chartControl; } 
		}
		
		public PortfolioControl PortfolioControl {
        	get { return portfolioControl; }
		}
		
		public MainForm MainForm {
			get { return mainForm; }
		}
	}
}
