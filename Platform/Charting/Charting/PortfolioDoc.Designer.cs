using TickZoom;
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
	partial class PortfolioDoc
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
			this.components = new System.ComponentModel.Container();
			this.chartControl1 = new TickZoom.ChartControl(execute);
			this.SuspendLayout();
			// 
			// chartControl1
			// 
			this.chartControl1.ChartBars = null;
			this.chartControl1.ChartType = TickZoom.Api.ChartType.Bar;
			this.chartControl1.StrategyForTrades = null;
			this.chartControl1.IntervalChartBar = null;
			this.chartControl1.IsDynamicUpdate = false;
			this.chartControl1.Location = new System.Drawing.Point(3, 6);
			this.chartControl1.Name = "chartControl1";
//			this.chartControl1.ScrollGrace = 0;
//			this.chartControl1.ScrollMaxX = 0;
//			this.chartControl1.ScrollMaxY = 0;
//			this.chartControl1.ScrollMaxY2 = 0;
//			this.chartControl1.ScrollMinX = 0;
//			this.chartControl1.ScrollMinY = 0;
//			this.chartControl1.ScrollMinY2 = 0;
			this.chartControl1.ShowPriceGraph = true;
			this.chartControl1.Size = new System.Drawing.Size(791, 452);
			this.chartControl1.TabIndex = 0;
			// 
			// PortfolioDoc
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(798, 456);
			this.Controls.Add(this.chartControl1);
			this.Name = "PortfolioDoc";
			this.Text = "PortfolioDoc";
			this.Load += new System.EventHandler(this.PortfolioDocLoad);
			this.Resize += new System.EventHandler(this.PortfolioDocResize);
			this.ResumeLayout(false);
		}
		
		private TickZoom.ChartControl chartControl1;
	}
}
