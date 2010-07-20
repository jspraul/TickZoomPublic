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
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.EngineTests
{
	/// <summary>
	/// Description of Formula.
	/// </summary>
	public class StrategyTest : Model
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(StrategyTest));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		string name;
		Chain chain;
		Interval intervalDefault = Intervals.Default;
		PositionInterface position;
		DrawingInterface drawing;
		List<Interval> updateIntervals = new List<Interval>();
		
		public StrategyTest()
		{
			position = new PositionCommon(this);
			drawing = new DrawingCommon(this);
			indicator =  Doubles();
			
			if( trace) log.Trace(GetType().Name+".new");
			name = GetType().Name;
			chain = Factory.Engine.Chain(this);
		}
		
		public ModelInterface NextFormulax {
			get { return Chain.Next.Model; }
		}
		
		public Elapsed SignalElapsed {
			get { return Ticks[0].Time - Position.Time; }
		}
		
		public void AddIndicator( Model model) {
			chain.Dependencies.Add(model.Chain);
		}

		public override string ToString()
		{
			return name;
		}
		
		public virtual bool OnSave( string fileName) {
			return true;
		}
		
		Doubles indicator;
		public int Count {
			get { return indicator.Count; }
		}
		
		public int BarCount {
			get { return indicator.BarCount; }
		}
		
		public double this[int position]
		{
			get { return indicator[position]; }
			set { indicator[position] = value; }
		}
		
		public void Add(double value)
		{
			indicator.Add(value);
		}
		
		public void Clear()
		{
			indicator.Clear();
		}
		
		public PositionInterface Position {
			get { return position; }
			set { position = value; }
		}
		
		public bool WriteReport(string folder)
		{
			throw new NotImplementedException();
		}
		
		public string ToStatistics()
		{
			return "";
		}
		
	}
}
