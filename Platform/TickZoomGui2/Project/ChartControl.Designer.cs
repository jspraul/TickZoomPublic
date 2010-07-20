using ZedGraph;
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
	partial class ChartControl
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
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
			this.dataGraph = new ZedGraph.ZedGraphControl();
			this.refreshTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// dataGraph
			// 
			this.dataGraph.AutoScroll = true;
			this.dataGraph.Location = new System.Drawing.Point(10, 3);
			this.dataGraph.Name = "dataGraph";
			this.dataGraph.ScrollGrace = 0;
			this.dataGraph.ScrollMaxX = 0;
			this.dataGraph.ScrollMaxY = 0;
			this.dataGraph.ScrollMaxY2 = 0;
			this.dataGraph.ScrollMinX = 0;
			this.dataGraph.ScrollMinY = 0;
			this.dataGraph.ScrollMinY2 = 0;
			this.dataGraph.Size = new System.Drawing.Size(767, 424);
			this.dataGraph.TabIndex = 0;
			this.dataGraph.MouseMoveEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.DataGraphMouseMoveEvent);
			this.dataGraph.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(this.DataGraphContextMenuBuilder);
			// 
			// refreshTimer
			// 
			this.refreshTimer.Enabled = true;
			this.refreshTimer.Tick += new System.EventHandler(this.refreshTick);
			// 
			// ChartControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.dataGraph);
			this.Name = "ChartControl";
			this.Size = new System.Drawing.Size(791, 452);
			this.Load += new System.EventHandler(this.ChartControlLoad);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Timer refreshTimer;
		private ZedGraph.ZedGraphControl dataGraph;
		
		public ZedGraphControl DataGraph {
			get { return dataGraph; }
		}
	}
}
