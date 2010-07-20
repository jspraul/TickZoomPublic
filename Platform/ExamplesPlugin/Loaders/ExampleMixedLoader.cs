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

namespace TickZoom.Examples
{
	/// <summary>
	/// Description of Starter.
	/// </summary>
	public class ExampleMixedLoader : ModelLoaderCommon
	{
		public ExampleMixedLoader() {
			/// <summary>
			/// IMPORTANT: You can personalize the name of each model loader.
			/// </summary>
			category = "Example";
			name = "Mixed: Multi-Symbol, Multi-Strategy";
		}
		
		public override void OnInitialize(ProjectProperties properties) {
			if( properties.Starter.SymbolProperties.Length < 2) {
				throw new ApplicationException( "This loader requires at least 2 symbols.");
			}
//			properties.Starter.SymbolProperties[1].SimulateTicks = true;
		}

		public override void OnLoad(ProjectProperties properties) {
			// This portfolio has one strategy on symbol A
			// and 2 strategies on symbol B to demonstrate
			// portfolio "self-organization".
			string symbol = properties.Starter.SymbolInfo[0].Symbol;
			Strategy strategy = CreateStrategy("ExampleOrderStrategy","ExampleOrder-"+symbol) as Strategy;
			strategy.SymbolDefault = symbol;
			strategy.Performance.Equity.GraphEquity = false;
	    	AddDependency( "Portfolio", "ExampleOrder-"+symbol);

			symbol = properties.Starter.SymbolInfo[1].Symbol;
			strategy = CreateStrategy("ExampleOrderStrategy","ExampleOrder-"+symbol) as Strategy;
			strategy.SymbolDefault = symbol;
			strategy.Performance.Equity.GraphEquity = false;
	    	AddDependency( "Portfolio", "ExampleOrder-"+symbol);
			
			strategy = CreateStrategy("ExampleReversalStrategy","ExampleReversal-"+symbol) as Strategy;
			strategy.SymbolDefault = symbol;
			strategy.Performance.Equity.GraphEquity = false;
	    	AddDependency( "Portfolio", "ExampleReversal-"+symbol);
			
			TopModel = GetPortfolio("Portfolio");
		}
	}

}
