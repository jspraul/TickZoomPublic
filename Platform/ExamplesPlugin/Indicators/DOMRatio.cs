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
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of SMA.
	/// </summary>
	public class DOMRatio : IndicatorCommon
	{
		IndicatorCommon bidSize;
		IndicatorCommon askSize;
		IndicatorCommon ratio;
		IndicatorCommon extreme, average, dryUp;
		
		public IndicatorCommon Ratio {
			get { return ratio; }
		}
		
		public DOMRatio()
		{
			Drawing.PaneType = PaneType.Secondary;
		}
		
		public override void OnInitialize() {
			Drawing.GroupName = "DOM";
			
			ratio = new IndicatorCommon();
			ratio.Drawing.Color = Color.Magenta;
			ratio.Drawing.GroupName = "Ratio";
			AddIndicator(ratio);
			
			bidSize = new IndicatorCommon();
			bidSize.Drawing.Color = Color.Blue;
			bidSize.Drawing.GroupName = "InnerDOM";
			AddIndicator(bidSize);

			askSize = new IndicatorCommon();
			askSize.Drawing.Color = Color.Magenta;
			askSize.Drawing.GroupName = "InnerDOM";
			AddIndicator(askSize);
			
			extreme = Formula.Line(Extreme,Color.Red);
			average = Formula.Line(Average,Color.Orange);
			dryUp = Formula.Line(DryUp,Color.Green);
		}

		public override bool OnIntervalOpen() {
			bidSize[0] = 0;
			askSize[0] = 0;
			return true;
		}
		
		long totalBidSize = 0;
		long totalAskSize = 0;
		long tickCount = 0;
		public override bool OnProcessTick(Tick tick)
		{
			int askDepth = tick.AskDepth;
			int bidDepth = tick.BidDepth;
			totalAskSize += askDepth;
			totalBidSize += bidDepth;
			tickCount ++;

			int total = askDepth + bidDepth;
			if( bidDepth > askDepth) {
				ratio[0] = askDepth == 0 ? 0 : bidDepth / askDepth;
			} else if( bidDepth < askDepth) { 
				ratio[0] = bidDepth == 0 ? 0 : - askDepth / bidDepth;
			} else {
				ratio[0] = 0;
			}
			
			bidSize[0] = tick.BidLevel(0) + tick.BidLevel(1);
			askSize[0] = tick.AskLevel(0) + tick.AskLevel(1);
			
			return true;
		}
		
		TimeStamp endDate = new TimeStamp(2007,8,29);
		public override bool OnIntervalClose() {
			if( totalAskSize > 0 && tickCount > 0) {
				this[0] = (int) ((totalBidSize + totalAskSize)/tickCount);
			} 
			
			tickCount = 0;
			totalBidSize = 0;
			totalAskSize = 0;
			
			CalcStrength();
			return true;
		}
		
		public int Extreme {
			get { return 6000; }
		}
		
		public int Average {
			get { return 4000; }
		}
		
		public int DryUp {
			get { return 2000; }
		}
		
		public IndicatorCommon BidSize {
			get { return bidSize; }
		}
		
		public IndicatorCommon AskSize {
			get { return askSize; }
		}
		
		Strength strength = Strength.Lo;
		
		public Strength Strength {
			get { return strength; }
		}
		TimeStamp dryUpTimer;
		private void CalcStrength() {
			if( this[0] > Extreme ) {
				strength = Strength.Ex;
			} else if( this[0] > Average ) {
				strength = Strength.Hi;
			} else if( this[0] > DryUp ) {
				strength = Strength.Lo;
			} else {
				strength = Strength.DU;
				dryUpTimer = Ticks[0].Time;
				dryUpTimer.AddMinutes(5);
			}
		}
		
		public bool IsDryUp {
			get { return strength == Strength.DU || Ticks[0].Time < dryUpTimer; }
		}
		
		public bool IsLow {
			get { return strength == Strength.Lo; }
		}
		
		public bool IsHigh {
			get { return strength == Strength.Hi; }
		}
		
		public bool IsExtreme {
			get { return strength == Strength.Ex; }
		}		
	}
}
