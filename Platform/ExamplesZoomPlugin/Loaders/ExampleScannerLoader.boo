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

class ExampleScannerLoader(ModelLoaderCommon):

	def constructor():
		category = 'ZoomScript'
		name = 'Scanner'

	
	override def OnInitialize(properties as ProjectProperties):
		pass

	
	override def OnLoad(properties as ProjectProperties):
		for i in range(0, properties.Starter.SymbolInfo.Length):
		// Loop through and setup a default strategy to handle orders
		// for each symbol.
			symbol as string = properties.Starter.SymbolInfo[i].Symbol
			market as ModelInterface = CreateStrategy('StrategyCommon', (symbol + 'Strategy'))
			market.SymbolDefault = symbol
			AddDependency('ExampleScannerStrategy', (symbol + 'Strategy'))
		TopModel = GetStrategy('ExampleScannerStrategy')
		TopModel.SymbolDefault = properties.Starter.SymbolInfo[0].Symbol
	

