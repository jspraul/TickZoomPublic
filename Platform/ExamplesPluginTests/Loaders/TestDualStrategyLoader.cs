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
using System.Threading;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Examples;

namespace Loaders
{
	public class TestDualStrategyLoader : ModelLoaderCommon
	{
		public TestDualStrategyLoader() {
			/// <summary>
			/// IMPORTANT: You can personalize the name of each model loader.
			/// </summary>
			category = "Example";
			name = "Dual Symbol";
			this.IsVisibleInGUI = false;
		}
		
		public override void OnInitialize(ProjectProperties properties) {
		}
		
		public override void OnLoad(ProjectProperties properties) {
			properties.Engine.RealtimeOutput = false;
			Portfolio portfolio = CreatePortfolio("Portfolio","EntirePortfolio");
			foreach( var symbol in properties.Starter.SymbolProperties) {
				string name = "ExampleOrderStrategy+" + symbol.Symbol;
				ExampleOrderStrategy strategy = (ExampleOrderStrategy) CreateStrategy("ExampleOrderStrategy",name);
				strategy.Multiplier = 10D;
				strategy.SymbolDefault = symbol.Symbol;
	//				portfolio.Performance.Equity.StartingEquity = 100000;
				AddDependency(portfolio,strategy);
			}
			TopModel = portfolio;
		}
	}
}
