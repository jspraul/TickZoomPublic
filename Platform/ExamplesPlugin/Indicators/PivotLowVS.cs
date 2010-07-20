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
using TickZoom.Api;

using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of TEMA.
	/// </summary>
	public class PivotLowVs : IndicatorCommon
	{
		Doubles pivotLows;
		Integers pivotBars;
		int LSTREN;
		int RSTREN;
		int LENGTH;
		double SHBAR = 0;
		double pivot = 0;
		int pivotBar = 0;
		bool disableBoxes = false;
		bool rightPassed = false;
		bool leftPassed = false;
		double highest = 0;
		
		public PivotLowVs() : this( 2, 2, 5) {
		}
		
		public PivotLowVs( int left, int right, int length) {
			pivotLows = Doubles();
			pivotBars = Integers();
			LSTREN = left;
			RSTREN = right;
			LENGTH = Math.Max(length,left+right+1);
			Drawing.Color = Color.Aqua;
		}

		public PivotLowVs( int left, int right) : this( left, right, left+right+1) {
		}
		
		public override void OnInitialize() {
		}
		
		public override bool OnIntervalClose() {
			double currHigh = Bars.High[0];
			if( highest == 0) {
				highest = currHigh;
			} else {
				if( currHigh > highest) {
					highest = currHigh;
				}
			}       	
			// DO NOT DELETE. Turn to hidden instead if necessary.
			this[0] = highest;
			
			pivot = 0;
			for( int MAINLOOP = RSTREN; MAINLOOP < LENGTH; MAINLOOP ++) {
				rightPassed = true;
				leftPassed = true;
				SHBAR = Bars.Low[MAINLOOP] - 1;
			
				for( int VALUE1 = MAINLOOP - RSTREN; VALUE1 <= MAINLOOP -1; VALUE1++) {
					if( SHBAR > Bars.Low[VALUE1]) {
						rightPassed = false;
						break;
					}
				}
			
				if( rightPassed ) {
					for( int VALUE1 = MAINLOOP + 1; VALUE1 <= MAINLOOP + LSTREN; VALUE1++) {
					    if( SHBAR > Bars.Low[VALUE1]) {
							leftPassed = false;
							break;
					    }
					}
				}
			
				if( rightPassed && leftPassed) {
					pivot = SHBAR;
					pivotBar = MAINLOOP;
					break;
				}
			}
			
			if( leftPassed && rightPassed && ( pivotLows.Count == 0 || pivot != pivotLows[0])) {
				if( pivot < this[pivotBar]) {
					highest = pivot;
					pivotLows.Add(pivot);
					int bar = Bars.CurrentBar-pivotBar;
					pivotBars.Add(bar);
					Elapsed elapsed = default(Elapsed);
					if( pivotBars.Count>1) {
						elapsed = Bars.Time[pivotBar] - Bars.Time[Bars.CurrentBar-pivotBars[1]];
					}
					if( !disableBoxes) {
						Chart.DrawBox(Drawing.Color,bar,pivot);
						if( pivotBars.Count>1) {
							Log.Debug( "|Pivot Low," + Bars.Time[pivotBar] + "," + elapsed.TotalSeconds +
						              "," + Bars.Time[pivotBar] + "," + Bars.Time[Bars.CurrentBar-pivotBars[1]]);
						}
					}
				}
			}

			return true;
		}	
		public int Left {
			get { return LSTREN; }
			set { LSTREN = value; }
		}
		
		public int Right {
			get { return RSTREN; }
			set { RSTREN = value; }
		}
		
		public int Length {
			get { return LENGTH; }
			set { LENGTH = value; }
		}
		
		public Doubles PivotLows {
			get { return pivotLows; }
		}
		
		public Integers PivotBars {
			get { return pivotBars; }
		}
		
		public bool DisableBoxes {
			get { return disableBoxes; }
			set { disableBoxes = value; }
		}
	} 
}
