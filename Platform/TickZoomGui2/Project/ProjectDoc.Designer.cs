using System.Windows.Forms;
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

namespace TickZoom
{
	partial class ProjectDoc
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.portfolioControl = new TickZoom.PortfolioControl();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.simulationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.chartControl = new TickZoom.ChartControl();
			this.tabControl.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabPage1);
			this.tabControl.Location = new System.Drawing.Point(2, 1);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(801, 481);
			this.tabControl.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.portfolioControl);
			this.tabPage1.Controls.Add(this.menuStrip);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(793, 455);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Settings";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// portfolioControl
			// 
			this.portfolioControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.portfolioControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.portfolioControl.Location = new System.Drawing.Point(3, 3);
			this.portfolioControl.Name = "portfolioControl";
			this.portfolioControl.Size = new System.Drawing.Size(787, 449);
			this.portfolioControl.TabIndex = 0;
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.menuItem1});
			this.menuStrip.Location = new System.Drawing.Point(3, 3);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(787, 24);
			this.menuStrip.TabIndex = 1;
			this.menuStrip.Text = "Run";
			// 
			// menuItem1
			// 
			this.menuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.simulationMenuItem});
			this.menuItem1.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.menuItem1.MergeIndex = 1;
			this.menuItem1.Name = "menuItem1";
			this.menuItem1.Size = new System.Drawing.Size(38, 20);
			this.menuItem1.Text = "Run";
			// 
			// simulationMenuItem
			// 
			this.simulationMenuItem.Name = "simulationMenuItem";
			this.simulationMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.simulationMenuItem.Size = new System.Drawing.Size(152, 22);
			this.simulationMenuItem.Text = "Simulation";
			// 
			// chartControl
			// 
			this.chartControl.ChartBars = null;
			this.chartControl.ChartType = TickZoom.Api.ChartType.Bar;
			this.chartControl.IntervalChartBar = null;
			this.chartControl.IsDynamicUpdate = false;
			this.chartControl.Location = new System.Drawing.Point(-2, 0);
			this.chartControl.Name = "chartControl";
			this.chartControl.ShowPriceGraph = true;
			this.chartControl.Size = new System.Drawing.Size(742, 448);
			this.chartControl.StrategyForTrades = null;
			this.chartControl.TabIndex = 0;
			// 
			// ProjectDoc
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(806, 484);
			this.Controls.Add(this.tabControl);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ProjectDoc";
			this.Text = "PortfolioDoc";
			this.Load += new System.EventHandler(this.ProjectDocLoad);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProjectDocClosing);
			this.Resize += new System.EventHandler(this.ProjectDocResize);
			this.tabControl.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ToolStripMenuItem simulationMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menuItem1;
		private System.Windows.Forms.MenuStrip menuStrip;
		
		private TickZoom.PortfolioControl portfolioControl;
		private TickZoom.ChartControl chartControl;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabControl tabControl;
		
	}
}
