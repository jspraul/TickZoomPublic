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
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;
using TickZoom.Transactions;

namespace Loaders
{
	public class ExampleDualStrategyLoader : ModelLoaderCommon
	{
		public ExampleDualStrategyLoader() {
			category = "Example";
			name = "Dual Strategy";
			IsVisibleInGUI = false;
		}
		
		public override void OnInitialize(ProjectProperties properties) {
		}
		
		public override void OnLoad(ProjectProperties properties) {
			var portfolio = new Portfolio();
			var fourTicks = new ExampleOrderStrategy() {
				Name = "FourTicksData"
			};
			fourTicks.SymbolDefault = properties.Starter.SymbolInfo[0].Symbol;
			var reversal = new ExampleReversalStrategy() {
				SymbolDefault = properties.Starter.SymbolInfo[0].Symbol
			};
			AddDependency(portfolio,fourTicks);
			AddDependency(portfolio,reversal);
			portfolio.Performance.GraphTrades = false;
			TopModel = portfolio;
		}
	}
}
