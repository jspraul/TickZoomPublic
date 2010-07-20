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

namespace TickZoom.PropertyEditor
{
	partial class IntervalEditorControl
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
			this.timeUnitCombo = new System.Windows.Forms.ComboBox();
			this.periodCombo = new System.Windows.Forms.ComboBox();
			this.periodLabel = new System.Windows.Forms.Label();
			this.timeUnitLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// timeUnitCombo
			// 
			this.timeUnitCombo.FormattingEnabled = true;
			this.timeUnitCombo.Location = new System.Drawing.Point(65, 3);
			this.timeUnitCombo.Name = "timeUnitCombo";
			this.timeUnitCombo.Size = new System.Drawing.Size(121, 21);
			this.timeUnitCombo.TabIndex = 0;
			// 
			// periodCombo
			// 
			this.periodCombo.FormattingEnabled = true;
			this.periodCombo.Location = new System.Drawing.Point(3, 3);
			this.periodCombo.Name = "periodCombo";
			this.periodCombo.Size = new System.Drawing.Size(56, 21);
			this.periodCombo.TabIndex = 1;
			// 
			// periodLabel
			// 
			this.periodLabel.Location = new System.Drawing.Point(3, 27);
			this.periodLabel.Name = "periodLabel";
			this.periodLabel.Size = new System.Drawing.Size(56, 18);
			this.periodLabel.TabIndex = 2;
			this.periodLabel.Text = "Period";
			this.periodLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// timeUnitLabel
			// 
			this.timeUnitLabel.Location = new System.Drawing.Point(65, 27);
			this.timeUnitLabel.Name = "timeUnitLabel";
			this.timeUnitLabel.Size = new System.Drawing.Size(121, 18);
			this.timeUnitLabel.TabIndex = 3;
			this.timeUnitLabel.Text = "Bar Unit";
			this.timeUnitLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// IntervalEditorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.timeUnitLabel);
			this.Controls.Add(this.periodLabel);
			this.Controls.Add(this.periodCombo);
			this.Controls.Add(this.timeUnitCombo);
			this.Name = "IntervalEditorControl";
			this.Size = new System.Drawing.Size(190, 45);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ComboBox periodCombo;
		private System.Windows.Forms.ComboBox timeUnitCombo;
		private System.Windows.Forms.Label timeUnitLabel;
		private System.Windows.Forms.Label periodLabel;
	}
}
