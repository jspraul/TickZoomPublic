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

namespace TickZoom.Api
{
	[Obsolete("Please use PositionInterface instead.",true)]
	public interface Position : PositionInterface {
		
	}
	
	public interface PositionInterface {
		[Obsolete("Please use Current instead.",true)]
		int Signal {
			get;
			set;
		}
		
		/// <summary>
		/// What is the current position? Returns a positive size for a long 
		/// and a negative size for a short position. Zero for a flat.
		/// </summary>
		int Current {
			get;
		}
		
		/// <summary>
		/// Changes the current position.
		/// </summary>
		/// <param name="position">A new position.</param>
		/// <exception cref="TickZoomException">
		/// if position parameter has the same value as the current position.
		/// </exception>
		void Change( int position);
		
		void Change( int  position, double price, TimeStamp time);
		
		void Change( SymbolInfo symbol, int position, double price, TimeStamp time);
		
		void Change( SymbolInfo symbol, LogicalFill fill);
		
		/// <summary>
		/// Copies a Position object including all properties to this position object.
		/// </summary>
		/// <param name="other">the other Position object.</param>
		void Copy( PositionInterface other);

		[Obsolete("Please use Price instead.",true)]
		double SignalPrice {
			get;
		}
		
		/// <summary>
		/// The price at which the current position most recently changed.
		/// </summary>
		double Price {
			get;
		}
		
		[Obsolete("Please use Time instead.",true)]
		TimeStamp SignalTime {
			get;
		}

		/// <summary>
		/// The when the current position last changed.
		/// </summary>
		TimeStamp Time {
			get;
		}
		
		/// <summary>
		/// Is the position currently either long or short?
		/// </summary>
		bool HasPosition {
			get;
		}
		
		/// <summary>
		/// Is the position currently flat?
		/// </summary>
		bool IsFlat {
			get;
		}
		
		/// <summary>
		/// Is the position currently long?
		/// </summary>
		bool IsLong {
			get;
		}
		
		/// <summary>
		/// Is the position currently short?
		/// </summary>
		bool IsShort {
			get;
		}
		
		/// <summary>
		/// What is the size of the position? Returns the absolute value of the current position.
		/// </summary>
		int Size {
			get;
		}

		/// <summary>
		/// Which logical order caused this fill?
		/// </summary>
		int OrderId {
			get;
		}
	}
}
