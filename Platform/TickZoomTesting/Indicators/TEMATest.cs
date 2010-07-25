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
				10035,
				10040,
				10099,
				10339,
				10666,
				10946,
				11404,
				11852,
				12102,
				12374,
				12725,
				13095,
				13100,
				12852,
				12809,
				12614,
				12444,
				12122,
				11682,
				11315,
				11338,
				11690,
				12226,
				12731,
				13193,
				13407,
				13663,
				14056,
				14115,
				14260,
				14252,
				14006,
				13902,
				14044,
				14179,
				14044,
				13780,
				13498,
				13539,
				13603,
				13434,
				13250,
				12894,
				12707,
				12797,
				12522,
				12176,
				12076,
				11785,
				11265,
				10632,
				10302,
				10333,
				10644,
				10931
			};
		}		
	}
}