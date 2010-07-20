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

namespace TickZoom.Common
{
	/// <summary>
	/// Description of Optimizer.
	/// </summary>
	public struct OptimizeRange : IEquatable<OptimizeRange>
	{
		string from;
		string to;
		Operator oper;
		
		public static OptimizeRange Create(string from, string to, Operator oper) {
			OptimizeRange rule = new OptimizeRange(from, to, oper);
			return rule;
		}
	
		private OptimizeRange( string from, string to, Operator oper) {
			this.from = from;
			this.to = to;
			this.oper = oper;
		}
		
		public string From {
			get { return from; }
			set { from = value; }
		}
		
		public string To {
			get { return to; }
			set { to = value; }
		}
		
		public Operator Operator {
			get { return oper; }
			set { oper = value; }
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
		
		public bool Equals(OptimizeRange other)
		{
			// add comparisions for all members here
			return this.from == other.from && this.to == other.to && this.oper == other.oper;
		}
		
		public override int GetHashCode()
		{
			// combine the hash codes of all members here (e.g. with XOR operator ^)
			return (from + to + oper).GetHashCode();
		}
		
		public static bool operator ==(OptimizeRange lhs, OptimizeRange rhs)
		{
			return lhs.Equals(rhs);
		}
		
		public static bool operator !=(OptimizeRange lhs, OptimizeRange rhs)
		{
			return !(lhs.Equals(rhs)); // use operator == and negate result
		}
		#endregion
	}
}
