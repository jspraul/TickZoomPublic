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
	/// <summary>
	/// Description of RandomStrategy.
	/// </summary>
	public class PivotHighTrend : Strategy
	{
		PivotHighVs pivotHigh;
		PivotLowVs pivotLow;
		int strength=6;
		IndicatorCommon pivotDiff;
		int maxPivotDiff = 1000;
		
		public PivotHighTrend()
		{
		}
		
		public override void OnInitialize() {
			pivotHigh = new PivotHighVs(strength, strength);
			pivotHigh.IntervalDefault = Intervals.Hour1;
			AddIndicator( pivotHigh);
			
			pivotLow = new PivotLowVs(strength, strength);
			pivotLow.IntervalDefault = Intervals.Hour1;
			AddIndicator( pivotLow);
			
			pivotDiff = new IndicatorCommon();
			pivotDiff.IntervalDefault = Intervals.Hour1;
			AddIndicator( pivotDiff);
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
			if( timeFrame.Equals(Intervals.Minute1)) {
				pivotDiff[0] = pivotHigh.PivotHighs[0] - pivotLow.PivotLows[0];
				if( pivotDiff[0] < maxPivotDiff) {
					if( (Position.IsShort| Position.IsFlat) && Formula.CrossesOver(Hours.High,(int)pivotHigh[1])) {
						Orders.Enter.ActiveNow.BuyMarket();
					}
				}
				if( (Position.IsLong| Position.IsFlat) && Formula.CrossesUnder(Hours.Low,(int)pivotLow[1])) {
					Orders.Exit.ActiveNow.GoFlat();
				}
			}
			return true;
		}

		public override string OnGetOptimizeHeader(System.Collections.Generic.Dictionary<string, object> optimizeValues)
		{
			return "DailyCount,DailyWinRate,DailyProfitFactor," + base.OnGetOptimizeHeader(optimizeValues);
		}
		
		public override string OnGetOptimizeResult(Dictionary<string,object> optimizeValues)
		{
			string result = base.OnGetOptimizeResult(optimizeValues);
			return pivotHigh.LeftStrength + "," + pivotHigh.RightStrength + "," + pivotHigh.Length + "," + result;
		}
		
		public int Strength {
			get { return strength; }
			set { strength = value; }
		}
		
	}
}
