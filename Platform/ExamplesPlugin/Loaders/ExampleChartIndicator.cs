using System;
using System.Collections.Generic;
using System.Text;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Examples

{
    public class ExampleChartIndicator : ModelLoaderCommon
    {
    	
        public ExampleChartIndicator()
        {
			/// <summary>
			/// IMPORTANT: You can personalize the name of each model loader.
			/// </summary>
			category = "Example";
			name = "ChartIndicator";
		}
		
		public override void OnInitialize(ProjectProperties properties) {

		}
        
		public override void OnLoad(ProjectProperties properties) {
			TopModel = GetStrategy("ChartIndicator");
		}
    }
}

