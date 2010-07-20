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

namespace TickZoom.Common
{
	/// <summary>
	/// Description of Optimizer.
	/// </summary>
	public struct OptimizeVar : IEquatable<OptimizeVar>, OptimizeVariable
	{
		string name;
		double start;
		double end;
		double increment;
		int count;
		
		public static OptimizeVar Create(string name, double start, double end, double increment) {
			OptimizeVar var = new OptimizeVar(name, start, end, increment);
			var.UpdateInfo();
			return var;
		}

		private OptimizeVar( string name, double start, double end, double increment) {
			this.name = name;
			this.start = start;
			this.end = end;
			this.increment = increment;
			this.count = 0;
		}
		
		void UpdateInfo() {
			count = (int) ((end - start) / increment + 1);
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public double Start {
			get { return start; }
			set { start = value; }
		}
		
		public double End {
			get { return end; }
			set { end = value; }
		}
		
		public double Increment {
			get { return increment; }
			set { increment = value; }
		}
		
		public int Count {
			get { return count; }
		}
		
		#region Equals and GetHashCode implementation
		// The code in this region is useful if you want to use this structure in collections.
		// If you don't need it, you can just remove the region and the ": IEquatable<Optimizer>" declaration.
		
		public override bool Equals(object obj)
		{
			if (obj is OptimizeVar)
				return Equals((OptimizeVar)obj); // use Equals method below
			else
				return false;
		}
		
		public bool Equals(OptimizeVar other)
		{
			// add comparisions for all members here
			return this.name == other.name && this.start == other.start && this.end == other.end && this.increment == other.increment;
		}
		
		public override int GetHashCode()
		{
			// combine the hash codes of all members here (e.g. with XOR operator ^)
			return (name + start + end + increment).GetHashCode();
		}
		
		public static bool operator ==(OptimizeVar lhs, OptimizeVar rhs)
		{
			return lhs.Equals(rhs);
		}
		
		public static bool operator !=(OptimizeVar lhs, OptimizeVar rhs)
		{
			return !(lhs.Equals(rhs)); // use operator == and negate result
		}
		#endregion
	}
}
