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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

using TickZoom.Api;
using TickZoom.Statistics;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of IndicatorSupport.
	/// </summary>
	public class IndicatorCommon : Model, IndicatorInterface
	{
		private readonly Log instanceLog;
		private readonly bool instanceDebug;
		private readonly bool instanceTrace;
		private bool isChartDynamic = false;
		double startValue = Double.NaN;
		bool isStartValueSet = false;
		Interval fastUpdateInterval = null;
		Doubles output;
		Doubles input;
		object anyInput;
		
		private Performance performance;
		
		protected object AnyInput {
			set { anyInput = value; }
		}
		
		/// <summary>
		/// Create a new generic indicator. This allows you
		/// to control the calculation and features of your
		/// indicator within your strategy. You most often use
		/// this feature to try out a new indicator idea or for
		/// a very simple indicator. For better performance and
		/// organization, you should eventually move your
		/// indicator to a separate class.
		/// </summary>
		public IndicatorCommon()
		{
			instanceLog = Factory.Log.GetLogger(this.GetType());
			instanceDebug = instanceLog.IsDebugEnabled;
			instanceTrace = instanceLog.IsTraceEnabled;
			isIndicator = true;
			Drawing.GroupName = Name;
			if( fastUpdateInterval != null) {
				RequestUpdate(fastUpdateInterval);
			}
			output =  Doubles();
		}

		public override void OnConfigure()
		{
			if( anyInput == null) {
				input = Doubles(Bars.Close);
			} else {
				input = Doubles(anyInput);
			}
			isChartDynamic = Chart != null && Chart.IsDynamicUpdate;
		}
		
		public sealed override bool OnBeforeIntervalOpen() {
			base.OnBeforeIntervalOpen();
			if( isStartValueSet) { Add(this[0]); }
			else { Add(startValue); isStartValueSet = true; }
			return true;
		}
		
		public sealed override bool OnBeforeIntervalOpen(Interval interval) {
			return base.OnBeforeIntervalOpen(interval);
		}
		
		public sealed override bool OnBeforeIntervalClose() {
			return base.OnBeforeIntervalClose();
		}
		
		public sealed override bool OnBeforeIntervalClose(Interval interval) {
			return base.OnBeforeIntervalClose(interval);
		}

		public override bool OnProcessTick(Tick tick)
		{
			if( isChartDynamic) {
				Update();
				return true;
			} else {
				return false;
			}
		}
		
		public override bool OnIntervalClose() {
			Update();
			return true;
		}
		
		public override bool OnIntervalClose(Interval period)
		{
			if( period.Equals(fastUpdateInterval)) {
				Update();
			}
			return true;
		}	
		
		public virtual void Update() {
			
		}
		
		[Browsable(false)]
		public override string Name {
			get { return base.Name; }
			set { base.Name = value; /* propogateName(); */ }
		}
		
		public double StartValue {
			get { return startValue; }
			set { startValue = value; }
		}
		
		public Doubles Input {
			get { return input; }
		}
		
		[Browsable(true)]
		public override DrawingInterface Drawing {
			get { return base.Drawing; }
			set { base.Drawing = value; }
		}

		[Browsable(true)]
		public Interval FastUpdateInterval {
			get { return fastUpdateInterval; }
			set { fastUpdateInterval = value; }
		}

		#region Indicator Value Properties & Methods
		[Browsable(false)]
		public int Count {
			get { return output.Count; }
		}
		
		[Browsable(false)]
		public int BarCount {
			get { return output.BarCount; }
		}
		
		[Browsable(false)]
		public int CurrentBar {
			get { return output.CurrentBar; }
		}
		
		public void Add(double value)
		{
			output.Add(value);
		}

		public double this[int position]
		{
			get { return output[position]; }
			set { output[position] = value; }
		}
		
		public void Clear()
		{
			output.Clear();
		}
		#endregion
		
		public override string ToString()
		{
			if( Drawing == null == Drawing.GroupName.Equals(Name) ) {
				return Name;
			} else {
				return Name + "." + Drawing.GroupName;
			}
		}
		
		public Log Log {
			get { return instanceLog; }
		}
		
		public bool IsDebug {
			get { return instanceDebug; }
		}
		
		public bool IsTrace {
			get { return instanceTrace; }
		}
		
		public Performance Performance {
			get { return performance; }
			set { performance = value; }
		}
	}
}
