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
	public class ExampleScannerLoader : ModelLoaderCommon
	{
		public ExampleScannerLoader() {
			/// <summary>
			/// IMPORTANT: You can personalize the name of each model loader.
			/// </summary>
			category = "Example";
			name = "Scanner";
		}
		
		public override void OnInitialize(ProjectProperties properties) {
		}
		
		public override void OnLoad(ProjectProperties properties) {
			// Loop through and setup a default strategy to handle orders
			// for each symbol.
			for( int i=0; i<properties.Starter.SymbolProperties.Length; i++) {
				string symbol = properties.Starter.SymbolInfo[i].Symbol;
				ModelInterface market = CreateStrategy("StrategyCommon",symbol+"Strategy");
				market.SymbolDefault = symbol;
				AddDependency("ExampleScannerStrategy",symbol+"Strategy");
			}
			TopModel = GetPortfolio("ExampleScannerStrategy");
			TopModel.SymbolDefault = properties.Starter.SymbolInfo[0].Symbol;
		}
		
	}
}
