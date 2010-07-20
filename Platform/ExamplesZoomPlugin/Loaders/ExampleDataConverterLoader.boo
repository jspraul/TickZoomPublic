
namespace TickZoom

import System
import TickZoom.Api
import TickZoom.Common


class ExampleDataConverterLoader(ModelLoaderCommon):

	def constructor():
		category = 'ZoomScript'
		name = 'TS Data Converter'
	
	override def OnInitialize(properties as ProjectProperties):
		// uncomment as needed to convert Ascii into tck
		symbolProperties = properties.Starter.SymbolInfo
		for i in range(0, symbolProperties.Length):
			tsConverter = TSTickConverter(symbolProperties[i])
			tsConverter.Convert(false)
		// set to true for TS_ES_1Min_4wk_Feb09, otherwise false
	
	override def OnLoad(properties as ProjectProperties):
		TopModel = null
		
