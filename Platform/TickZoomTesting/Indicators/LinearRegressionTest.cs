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
using System.Collections;
using NUnit.Framework;
using TickZoom.Common;

namespace TickZoom.Indicators
{
	[TestFixture]
	public class LinearRegressionTest
	{
		[Test]
		public void TestMethod()
		{
			LinearRegression lr = new LinearRegression();
			lr.addPoint( 1.0, 2.6);
			lr.addPoint( 2.3, 2.8);
			lr.addPoint( 3.1, 3.1);
			lr.addPoint( 4.8, 4.7);
			lr.addPoint( 5.6, 5.1);
			lr.addPoint( 6.3, 5.3);
			
			lr.calculate();
			Assert.IsTrue( 0.5842 == Math.Round(lr.Slope*10000)/10000, "Linear Regression returned wrong slope.");
			Assert.IsTrue( 1.6842 == Math.Round(lr.Intercept*10000)/10000, "Linear Regression wrong intercept.");
			Assert.IsTrue( 0.9741 == Math.Round(lr.Correlation*10000)/10000, "Linear Regression returned wrong correlation.");

			// Should be able to repeat the test and it still work
			// since a reset() was added to calculate.
			lr.calculate();
			Assert.IsTrue( 0.5842 == Math.Round(lr.Slope*10000)/10000, "Linear Regression returned wrong slope.");
			Assert.IsTrue( 1.6842 == Math.Round(lr.Intercept*10000)/10000, "Linear Regression wrong intercept.");
			Assert.IsTrue( 0.9741 == Math.Round(lr.Correlation*10000)/10000, "Linear Regression returned wrong correlation.");
		
		}
		// N = 6;
		// SumX = 23.1
		// SumY = 23.6
		// SumXY = 103.16
		// SumX2 = 110
		// SumY2 = 100.4

	}
}
