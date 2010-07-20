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

namespace TickZoom.Api
{
	/// <summary>
	/// Description of StarterProperties.
	/// </summary>
	public interface StarterProperties
	{
		[Obsolete("In loaders, lease use SymbolProperties instead so you can override properties from the dictionary. The read only SymbolInfo is used everywhere else.",true)]
		SymbolInfo[] SymbolInfo {
			get;
		}
		
		ISymbolProperties[] SymbolProperties {
			get;
		}
		
		TimeStamp StartTime {
			get;
			set;
		}
		
		TimeStamp EndTime {
			get;
			set;
		}
		
		Interval IntervalDefault {
			get;
			set;
		}
		
		void SetSymbols( string symbols);
		
		string[] SymbolArray {
			get;
		}
		
		[Obsolete("Please call SetSymbols() method instead.",true)]
		string Symbols {
			set;
		}
		
		void CopyProperties(object obj);
	}
}
