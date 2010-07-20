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
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General License for more details.
 * 
 * You should have received a copy of the GNU General License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */

#endregion


namespace TickZoom

import System
import TickZoom.Api
import TickZoom.Common

class ExampleMixedLoader(ModelLoaderCommon):

	def constructor():
		category = 'ZoomScript'
		name = 'Mixed: Multi-Symbol, Multi-Strategy'

	
	override def OnInitialize(properties as ProjectProperties):
		if properties.Starter.SymbolInfo.Length < 2:
			raise ApplicationException('This loader requires at least 2 symbols.')
		//			properties.Starter.SymbolProperties[1].SimulateTicks = true;

	
	override def OnLoad(properties as ProjectProperties):
		// This portfolio has one strategy on symbol A
		// and 2 strategies on symbol B to demonstrate
		// portfolio "self-organization".
		symbol as string = properties.Starter.SymbolInfo[0].Symbol
		strategy = (CreateStrategy('ExampleOrderStrategy', ('ExampleOrder-' + symbol)) as StrategyCommon)
		strategy.SymbolDefault = symbol
		strategy.Performance.Equity.GraphEquity = false
		AddDependency('PortfolioCommon', ('ExampleOrder-' + symbol))
		
		symbol = properties.Starter.SymbolInfo[1].Symbol
		strategy = (CreateStrategy('ExampleOrderStrategy', ('ExampleOrder-' + symbol)) as StrategyCommon)
		strategy.SymbolDefault = symbol
		strategy.Performance.Equity.GraphEquity = false
		AddDependency('PortfolioCommon', ('ExampleOrder-' + symbol))
		
		strategy = (CreateStrategy('ExampleSimpleStrategy', ('ExampleSimple-' + symbol)) as StrategyCommon)
		strategy.SymbolDefault = symbol
		strategy.Performance.Equity.GraphEquity = false
		AddDependency('PortfolioCommon', ('ExampleSimple-' + symbol))
		
		TopModel = GetStrategy('PortfolioCommon')


