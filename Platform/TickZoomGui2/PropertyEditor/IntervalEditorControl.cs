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
using System.Drawing;
using System.Windows.Forms;

using TickZoom.Api;

namespace TickZoom.PropertyEditor
{
	/// <summary>
	/// Description of IntervalEditorControl.
	/// </summary>
	public partial class IntervalEditorControl : UserControl
	{
        public bool IsCanceled = false;
        Interval interval = Factory.Engine.DefineInterval(BarUnit.Day,1);
		
		public IntervalEditorControl()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			timeUnitCombo.DataSource = System.Enum.GetValues(typeof(BarUnit));
			List<int> numbers = new List<int>();
			numbers.Add(1);
			numbers.Add(4);
			numbers.Add(5);
			numbers.Add(10);
			numbers.Add(15);
			numbers.Add(20);
			numbers.Add(30);
			numbers.Add(50);
			numbers.Add(100);
			periodCombo.DataSource = numbers;
		}
		
		private void UpdateControl() {
			timeUnitCombo.Text = interval.BarUnit.ToString();
			periodCombo.Text = interval.Period.ToString();
		}
		
		private void UpdateInterval() {
			BarUnit barUnit = (BarUnit) Enum.Parse(typeof(BarUnit),timeUnitCombo.Text,true);
			int period = Convert.ToInt32( periodCombo.Text);
			interval = Factory.Engine.DefineInterval(barUnit,period);
		}
		
		public Interval Interval {
			get { UpdateInterval(); return interval; }
			set { interval = value; UpdateControl(); }
		}
	}
}
