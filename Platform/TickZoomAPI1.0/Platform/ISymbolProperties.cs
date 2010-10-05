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

namespace TickZoom.Api
{
	public interface ISymbolProperties : SymbolInfo
	{
		/// <summary>
		/// The time of day that the primary session for this symbol starts.
		/// </summary>
		new Elapsed SessionStart {
			get;
			set;
		}
		
		/// <summary>
		/// The time of day that the primary session for this symbol ends.
		/// </summary>
		new Elapsed SessionEnd {
			get;
			set;
		}
	 
 		/// <summary>
 		/// With which other symbols does this one get drawn on a chart? Returns
 		/// a group number where 0 means never draw this symbol on any chart.
 		/// All symbols with the same ChartGroup number will appear on the same
 		/// chart. You can only set this property inside your Loader before
 		/// the engine initializes the portfolios and strategies.
 		/// </summary>
 		new int ChartGroup {
 			get;
 			set;
 		}
 		
 		/// <summary>
 		/// Returns the ProfitLoss calculation for this symbols. It's used
 		/// primarily to correctly calculate transaction costs for trades.
 		/// To use your own broker's transaction costs formulas, you can
 		/// implement the ProfitLoss interface and assign an instance of
 		/// your object to this property in your loader.
 		/// </summary>
 		new ProfitLoss ProfitLoss {
 			get;
 			set;
 		}
 		
 		/// <summary>
 		/// Used by validation and loading of data to ensure parsing of prices yields
 		/// appropriate values.
 		/// </summary>
 		new double MaxValidPrice {
 			get;
 			set;
 		}
 		
	}
}
