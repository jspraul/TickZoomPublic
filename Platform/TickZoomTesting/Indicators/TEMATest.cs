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

namespace TickZoom.Indicators
{

	[TestFixture]
	public class TEMATest : IndicatorTest
	{
		protected override IndicatorCommon CreateIndicator()
		{
			return new TEMA(Bars.Close,14);
		}
		
		protected override double[] GetExpectedResults() {
			return new double[] {
				10000,
				10034.904,
				10040,
				10099.318,
				10338.677,
				10666.443,
				10946.273,
				11403.889,
				11852.342,
				12101.798,
				12374.02,
				12724.909,
				13094.84,
				13100.14,
				12852.472,
				12808.735,
				12613.987,
				12444.495,
				12122.412,
				11682.131,
				11314.599,
				11337.599,
				11690.462,
				12226.21,
				12730.867,
				13193.413,
				13407.35,
				13662.631,
				14056.037,
				14114.956,
				14260.481,
				14251.654,
				14005.571,
				13901.64,
				14043.54,
				14178.902,
				14043.674,
				13779.535,
				13498.254,
				13539.053,
				13603.423,
				13433.954,
				13250.258,
				12893.787,
				12706.971,
				12796.656,
				12521.658,
				12175.761,
				12075.902,
				11785.356,
				11265.468,
				10632.273,
				10302.431,
				10332.719,
				10643.562,
				10931.216,
			};
		}		
	}
}