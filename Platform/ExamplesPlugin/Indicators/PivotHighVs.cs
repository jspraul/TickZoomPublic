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
	public class PivotHighVs : IndicatorCommon
	{
		Integers pivotBars;
		Doubles pivotHighs;
		int leftStrength;
		int rightStrength;
		int length;
		double pivotHigh = 0;
		double pivot = 0;
		int pivotBar = 0;
		bool disableBoxes = false;
		bool pivotPassed = false;
		bool leftPassed = false;
		protected double lowest;
		
		public PivotHighVs( ) : this ( 2, 2, 5) {
		}
		
		public PivotHighVs( int left, int right, int length) {
			pivotHighs = Doubles();
			pivotBars = Integers();
			this.leftStrength = left;
			this.rightStrength = right;
			this.length = Math.Max(length,left+right+1);
			Drawing.Color = Color.Orange;
		}
		
		public PivotHighVs( int left, int right) : this ( left, right, left+right+1) {
		}

		public override void OnInitialize() {
		}
		
		public override bool OnIntervalClose() {
			double currLow = Bars.Low[0];
			if( lowest == 0) {
				lowest = currLow;
			} else {
				if( currLow < lowest) {
					lowest = currLow;
				}
			}
			// DO NOT DELETE : Turn to hidden instead if necessary.
			this[0] = lowest;
			
			pivot = 0;
			pivotBar = 0;
			for( int mainLoop = rightStrength; mainLoop < length; mainLoop ++) {
				pivotPassed = true;
				leftPassed = true;
				pivotHigh = Bars.High[mainLoop] + 1;
			
				for( int i = mainLoop - rightStrength; i < mainLoop; i++) {
				    if( pivotHigh < Bars.High[i]) {
						pivotPassed = false;
						break;
				    }
				}
				if( pivotPassed ) {
					for( int i = mainLoop + 1; i <= mainLoop + leftStrength; i++) {
					    if( pivotHigh < Bars.High[i]) {
							leftPassed = false;
							break;
					    }
					}
				}
			
				if( leftPassed && pivotPassed) {
					pivot = pivotHigh;
					pivotBar = mainLoop;
					break;
				}
			}
			if( leftPassed && pivotPassed && (pivotHighs.Count == 0 || pivot != pivotHighs[0])) {
				if( pivot > this[pivotBar]) {
					lowest = pivot;
					pivotHighs.Add(pivot);
					int bar = Bars.CurrentBar-pivotBar;
					pivotBars.Add(bar);
					Elapsed elapsed = default(Elapsed);
					if( pivotBars.Count>1) {
						elapsed = Bars.Time[pivotBar] - Bars.Time[Bars.CurrentBar-pivotBars[1]];
					}
					if( !disableBoxes) {
						Chart.DrawBox(Drawing.Color,bar,pivot);
						if( pivotBars.Count>1) {
							Log.Debug( "|Pivot High," + Bars.Time[pivotBar] + "," + elapsed.TotalSeconds +
						              "," + Bars.Time[pivotBar] + "," + Bars.Time[Bars.CurrentBar-pivotBars[1]]);
						}
					}
				}
			}

			return true;
		}	
		public int LeftStrength {
			get { return leftStrength; }
			set { leftStrength = value; }
		}
		
		public int RightStrength {
			get { return rightStrength; }
			set { rightStrength = value; }
		}
		
		public int Length {
			get { return length; }
			set { length = value; }
		}
		
		public Doubles PivotHighs {
			get { return pivotHighs; }
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
