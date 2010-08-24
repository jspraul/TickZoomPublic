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
	public class EMATest : IndicatorTest
	{
		protected override IndicatorCommon CreateIndicator()
		{
			return new EMA(Bars.Close,14);
		}
		
		protected override double[] GetExpectedResults() {
			return new double[] {
				10000,
				10013.333,
				10016.889,
				10041.304,
				10137.13,
				10277.513,
				10413.844,
				10629.332,
				10860.087,
				11032.076,
				11219.799,
				11445.159,
				11688.471,
				11803.342,
				11814.896,
				11886.243,
				11892.078,
				11893.134,
				11823.383,
				11689.598,
				11560.319,
				11561.609,
				11689.395,
				11902.809,
				12127.768,
				12357.399,
				12509.746,
				12683.113,
				12916.031,
				13033.894,
				13181.374,
				13270.525,
				13262.455,
				13291.461,
				13404.599,
				13517.319,
				13528.343,
				13479.231,
				13407.333,
				13442.356,
				13486.042,
				13441.236,
				13381.071,
				13244.928,
				13156.271,
				13164.768,
				13038.799,
				12873.626,
				12787.809,
				12626.768,
				12367.199,
				12043.572,
				11811.096,
				11706.95,
				11718.023,
				11740.954,
			};
		}
	}
}
