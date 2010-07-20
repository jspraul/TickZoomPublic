using System;
using System.Collections.Generic;
using System.Text;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Examples

{
    public class ExampleDataConverterLoader : ModelLoaderCommon
    {
        public ExampleDataConverterLoader()
        {
			/// <summary>
			/// IMPORTANT: You can personalize the name of each model loader.
			/// </summary>
			category = "Example";
			name = "TS Data Converter";
		}
		
		public override void OnInitialize(ProjectProperties properties) {
			// uncomment as needed to convert Ascii into tck
			ISymbolProperties[] symbolProperties = properties.Starter.SymbolProperties;
			for( int i=0; i<symbolProperties.Length; i++) {
				TSTickConverter tsConverter = new TSTickConverter(symbolProperties[i]);
				tsConverter.Convert(false);
				// set to true for TS_ES_1Min_4wk_Feb09, otherwise false
			}
		}
        
		public override void OnLoad(ProjectProperties properties) {

			TopModel = null;
		}
    }
}
