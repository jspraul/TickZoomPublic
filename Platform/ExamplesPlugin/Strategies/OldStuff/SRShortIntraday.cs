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
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of RandomStrategy.
	/// </summary>
	public class SRShortIntraday : Strategy
	{
		int range = 80;
		List<IndicatorCommon> lines;
		int lineCount = 0;
		int[] levels;
		int length = 30;
		double rewardFactor = 2;
		int levelCount = 0;
		
		public SRShortIntraday()
		{
	    	Drawing.Color = Color.Green;
	    	IntervalDefault = Intervals.Hour1;
		}
		
		public override void OnInitialize()
		{
			lines = new List<IndicatorCommon>();
			int maxLines = 2;
			levels = new int[maxLines];
			for(int i=0; i < maxLines; i++) {
				IndicatorCommon srLine = new IndicatorCommon();
				srLine.Drawing.Color = Color.Orange;
				lines.Add( srLine);
				AddIndicator( srLine);
			}
		}
		
		public override bool OnIntervalClose(Interval timeFrame)
		{
			if( timeFrame.Equals(Intervals.Minute1)) {
				// Only play if trend is flat.
				if( Next.Position.Current == 0) {
					if( Position.IsFlat && Formula.CrossesUnder( Bars.Low, Weeks.Low[1])) {
						Orders.Enter.ActiveNow.BuyMarket();
					}
					if(  Position.IsFlat && Bars.High[0] > Days.High[1]) {
						Orders.Exit.ActiveNow.GoFlat();
					}
				} else {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
			return true;
		}
		
		public int MatchesAny( int level) {
			for(int i=0; i<levelCount; i++) {
				if( Matches(level,levels[i]) ) {
					return i;
				}
			}
			return -1;
		}
		
		public bool Matches( int level1, int level2) {
			return Math.Pow(level1 - level2,2) <= Math.Pow(range*2,2);
		}
	
		public override bool OnIntervalClose() {
			lineCount=0;
			lines[lineCount++][0] = Days.High[1];
			lines[lineCount++][0] = Days.Low[1];
			for( int i=lineCount; i<lines.Count;i++) {
				lines[i][0] = Bars.Close[0];
			}
			// Remove any duplicate S/R levels which 
			// are closer together than range.
			if( lineCount < 1) return true;
			levelCount = 0;
			int sr;
			int match;
			for(int i=0; i<lineCount; i++) {
				sr = (int) lines[i][0];
				match = MatchesAny( sr );
				if( match >= 0) {
					levels[match] = (sr + levels[match])/2;
				} else {
					levels[levelCount] = sr;
					levelCount++;
				}
			}
			return true;
		}

		public override string ToString() {
			return rewardFactor + "," + length + "," + range;
		}
		
		public int Range {
			get { return range; }
			set { range = value; }
		}
		
		public int Length {
			get { return length; }
			set { length = value; }
		}

		public double RewardFactor {
			get { return rewardFactor; }
			set { rewardFactor = value; }
		}

	}
}
