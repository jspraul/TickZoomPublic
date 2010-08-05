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
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	public class SRBreakoutSupport : Strategy
	{
		int minimumMove = 370;
		AvgRange avgRange;
		SRHigh resistance;
		SRLow support;
		IndicatorCommon entryLevel;
		int filterPercent = 10;
		IndicatorCommon middle;
		
		public override void OnInitialize()
		{
			base.OnInitialize();
			resistance = new SRHigh(minimumMove);
			resistance.IntervalDefault = IntervalDefault;
			AddIndicator(resistance);
			
			support = new SRLow(minimumMove);
			support.IntervalDefault = IntervalDefault;
			AddIndicator(support);
			
			avgRange = new AvgRange(14);
			avgRange.IntervalDefault = Intervals.Day1;
			AddIndicator(avgRange);
			
			entryLevel = new IndicatorCommon();
			entryLevel.IntervalDefault = IntervalDefault;
			AddIndicator(entryLevel);
			
			middle = new IndicatorCommon();
			middle.IntervalDefault = Intervals.Day1;
			middle.Drawing.Color = Color.Orange;
			AddIndicator(middle);
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( timeFrame.Equals(Intervals.Day1)) {
				updateMinimumMove();
			}
			if( timeFrame.Equals(Intervals.Hour1)) {
				middle[0] = (Resistance[0] + Support[0]) / 2;
			}
			return true;
		}
		
		public bool IsNewResistance {
			get { return resistance[0] != resistance[1]; }
		}

		public bool IsNewSupport {
			get { return resistance[0] != resistance[1]; }
		}
		
		void updateMinimumMove() {
			support.MinimumMove = (int) (avgRange[0] * minimumMove / 100);
			resistance.MinimumMove = (int) (avgRange[0] * minimumMove / 100);
		}
		
		protected virtual bool LongTrend {
			get { return resistance.Pivots[0] > resistance.Pivots[1] + avgRange.HistoricalStdDev &&
					support.Pivots[0] > support.Pivots[1] + avgRange.HistoricalStdDev; }
		}
		
		protected virtual bool ShortTrend {
			get { return resistance.Pivots[0] < resistance.Pivots[1] - avgRange.HistoricalStdDev &&
			   		support.Pivots[0] < support.Pivots[1] - avgRange.HistoricalStdDev; }
		}
		
		public int MinimumMove {
			get { return minimumMove; }
			set { minimumMove = value; }
		}
		
		protected bool LowVolatility {
			get { return avgRange[0] < avgRange.HistoricalAverage; }
		}
		
		protected bool HighVolatility {
			get { return avgRange[0] > avgRange.HistoricalAverage + avgRange.HistoricalStdDev; }
		}
		public AvgRange AvgRange {
			get { return avgRange; }
		}
		public SRHigh Resistance {
			get { return resistance; }
		}
		
		public SRLow Support {
			get { return support; }
		}
		
		public IndicatorCommon EntryLevel {
			get { return entryLevel; }
		}
		
		public int FilterPercent {
			get { return filterPercent; }
			set { filterPercent = value; }
		}
		
		public int Filter {
			get { return (int) (avgRange[0] * filterPercent)/100; }
		}
		
		public IndicatorCommon Middle {
			get { return middle; }
		}
	}
}
