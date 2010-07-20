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
using System.Text;

namespace TickZoom.Api
{
	[Serializable]
	public struct Elapsed : IEquatable<Elapsed>
	{
		long elapsed;
		
		public const long MaxValue = long.MaxValue;
		public const long MinValue = long.MinValue;
		public const double XLDay1 = 2415018.5;
		public const long JulDayMin = 0L;
		public const long JulDayMax = 5373483;
		public const long XLDayMin = (long) (JulDayMin - XLDay1);
		public const long XLDayMax = (long) (JulDayMax - XLDay1);
		public const long MonthsPerYear = 12L;
		public const long HoursPerDay = 24L;
		public const long MinutesPerHour = 60L;
		public const long SecondsPerMinute = 60L;
		public const long MinutesPerDay = 1440L;
		public const long SecondsPerDay = 86400L;
		public const long MillisecondsPerSecond = 1000L;
		public const long MillisecondsPerMinute = 60000L;
		public const long MillisecondsPerHour = 3600000L;
		public const long MillisecondsPerDay = 86400000L;
		public const string DefaultFormatStr = "g";
		
		public Elapsed( long elapsed) {
			this.elapsed = elapsed;
		}
		
		public Elapsed( int hour, int minute, int second) {
			this.elapsed = hour * MillisecondsPerHour + minute * MillisecondsPerMinute + second * MillisecondsPerSecond;
		}
		
		public int Hours {
			get { return (int) ( (elapsed / MillisecondsPerHour) % HoursPerDay); }
		}
		
		public int Minutes {
			get { return (int) ( (elapsed / MillisecondsPerMinute) % MinutesPerHour); }
		}
		
		public int Seconds {
			get { return (int) ( (elapsed / MillisecondsPerSecond) % SecondsPerMinute); }
		}
		
		public int Milliseconds {
			get { return (int) ( elapsed % MillisecondsPerSecond); }
		}
		
		public long TotalSeconds {
			get { return (long) (elapsed / MillisecondsPerSecond); }
		}
		
		public long TotalMilliseconds {
			get { return elapsed; }
		}
		
		public int TotalHours {
			get { return (int) (elapsed / MillisecondsPerHour); }
		}
		
		public int TotalDays {
			get { return (int) (elapsed / MillisecondsPerDay); }
		}
		
		public int TotalMinutes {
			get { return (int) (elapsed / MillisecondsPerMinute); }
		}
		
		public static implicit operator Elapsed( long elapsed )
		{
			Elapsed retVal = default(Elapsed);
			retVal.Internal = elapsed;
			return retVal;
		}
		
		public static Elapsed operator -( Elapsed lhs, Elapsed rhs )
		{
			return new Elapsed(lhs.Internal - rhs.Internal);
		}
		
		public static Elapsed operator +( Elapsed lhs, Elapsed rhs )
		{
			return new Elapsed(lhs.Internal + rhs.Internal);
		}
		
		public static bool operator >=( Elapsed lhs, Elapsed rhs)
		{
			return lhs.CompareTo(rhs) >= 0;
		}
		
		public static bool operator ==( Elapsed lhs, Elapsed rhs)
		{
			return lhs.CompareTo(rhs) == 0;
		}
		
		public static bool operator !=( Elapsed lhs, Elapsed rhs)
		{
			return lhs.CompareTo(rhs) != 0;
		}
		
		public static bool operator <=( Elapsed lhs, Elapsed rhs)
		{
			return lhs.CompareTo(rhs) <= 0;
		}
		
		public static bool operator >( Elapsed lhs, Elapsed rhs)
		{
			return lhs.CompareTo(rhs) > 0;
		}
		
		public static bool operator <( Elapsed lhs, Elapsed rhs)
		{
			return lhs.CompareTo(rhs) < 0;
		}
		
		public int CompareTo( Elapsed target )
		{
			long value = elapsed - target.elapsed;
			return value == 0 ? 0 : value > 0 ? 1 : -1;
		}
		
//		public static implicit operator long( Elapsed elapsed )
//		{
//			return elapsed.elapsed;
//		}
		
		#region Equals and GetHashCode implementation
		// The code in this region is useful if you want to use this structure in collections.
		// If you don't need it, you can just remove the region and the ": IEquatable<Elapsed>" declaration.
		
		public override bool Equals(object obj)
		{
			if (obj is Elapsed)
				return Equals((Elapsed)obj); // use Equals method below
			else
				return false;
		}
		
		public bool Equals(Elapsed other)
		{
			// add comparisions for all members here
			return this.elapsed == other.elapsed;
		}
		
		public override int GetHashCode()
		{
			// combine the hash codes of all members here (e.g. with XOR operator ^)
			return elapsed.GetHashCode();
		}
		
		public long Internal {
			get { return elapsed; }
			set { elapsed = value; }
		}
		#endregion
		
		public override string ToString()
		{
			return TotalHours.ToString().PadLeft(2,'0') + ":" + Minutes.ToString().PadLeft(2,'0') + ":" + Seconds.ToString().PadLeft(2,'0') + "." + Milliseconds.ToString().PadLeft(3,'0');
		}
	}
}
