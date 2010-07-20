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
using TickZoom.Api;

namespace TickZoom.Properties
{
	/// <summary>
	/// Description of ModelProperty.
	/// </summary>
	public class ModelPropertyCommon : ModelProperty
	{
		string name;
		string value;
		double start = 0D;
		double end = 0D;
		double increment = 0D;
		int count = 0;
		bool optimize = false;
		
		public ModelProperty Clone() {
			return new ModelPropertyCommon(name, value, start, end, increment, optimize);
		}
		
		public ModelPropertyCommon(string name, string value)
		{
			this.name = name;
			this.value = value;
		}
		
		public ModelPropertyCommon(string name, string value, double start, double end, double increment, bool optimize)
			: this( name, value)
		{
			this.start = start;
			this.end = end;
			this.increment = increment;
			this.value = value;
			if( increment == 0D) {
				this.count = 0;
				this.optimize = false;
			} else {
				this.count = (int) ((end - start) / increment + 1);
				this.optimize = optimize;
			}
		}
		
		public string Name {
			get { return name; }
		}
		
		public string Value {
			get { return this.value; }
			set { this.value = value; }
		}
		
		public double Start {
			get { return start; }
		}
		
		public double End {
			get { return end; }
		}
		
		public double Increment {
			get { return increment; }
		}
		
		public int Count {
			get { return count; }
		}
		
		public bool Optimize {
			get { return optimize; }
		}
		
	}
}
