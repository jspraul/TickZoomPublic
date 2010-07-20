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
using TickZoom.TickUtil;

namespace TickZoom.Indicators
{
	[TestFixture]
	public class EMATest : IndicatorTest
	{
		protected override IndicatorCommon CreateIndicator()
		{
			return new EMA(Bars.Close,14);
		}
		
		protected override double[] GetExpectedResults() {
			return new double[] {
				10000,
				10013,
				10017,
				10041,
				10137,
				10278,
				10414,
				10629,
				10860,
				11032,
				11220,
				11445,
				11688,
				11803,
				11815,
				11886,
				11892,
				11893,
				11823,
				11690,
				11560,
				11562,
				11689,
				11903,
				12128,
				12357,
				12510,
				12683,
				12916,
				13034,
				13181,
				13271,
				13262,
				13291,
				13405,
				13517,
				13528,
				13479,
				13407,
				13442,
				13486,
				13441,
				13381,
				13245,
				13156,
				13165,
				13039,
				12874,
				12788,
				12627,
				12367,
				12044,
				11811,
				11707,
				11718,
				11741,
			};
		}
	}
}
