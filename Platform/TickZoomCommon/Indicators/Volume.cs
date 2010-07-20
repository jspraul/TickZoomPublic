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
using TickZoom.Api;
using System.Collections.Generic;
using System.Drawing;
using System.Media;

namespace TickZoom.Common
{
	public class Volume : IndicatorCommon
	{
		IndicatorCommon high, average, low;
		SMA averageVolume;
		
		public override void OnInitialize() {
			
			Drawing.PaneType = PaneType.Secondary;
			Drawing.GraphType = GraphType.Histogram;
			Drawing.GroupName = "Volume";
			
			averageVolume = new SMA(Bars.Volume,5);
			averageVolume.Drawing.GroupName = "Volume";
			AddIndicator(averageVolume);

			high = Formula.Line(266000,Color.Red);
			average = Formula.Line(187000,Color.Orange);
			low = Formula.Line(107000,Color.Green);
			
		}
		
		public override bool OnIntervalOpen()
		{
			if( Bars.Volume.Count>1) {
				this[0] = Bars.Volume[1];
			}
			return true;
		}
		
		public override bool OnProcessTick(Tick tick)
		{
			this[0] = Bars.Volume[0];
			CalcVolumeStrength();
			return true;
		}
		
		public override bool OnIntervalClose()
		{
			if( averageVolume[0] > average[0] &&
			    averageVolume[1] <= average[0] ) {
				Chart.AudioNotify(Audio.RisingVolume);
			}
			SetColorIndex();
			return true;
		}
	
		int lastBarColor = 1;
		public void SetColorIndex() {
			double avgVol = averageVolume[0];
			double avg = average[0];
			int barColor = 1;
			if( Formula.IsInsideBar(Bars,0) || Formula.IsOutsideBar(Bars,0)) {
				barColor = lastBarColor;
			} else if( this[0] < Bars.Volume[1]) {
				barColor = lastBarColor;
			} else {
				if( avgVol < avg ) {
					barColor = Bars.Close[0] > Bars.Close[1] ? 3 : 4;
				} else {
					barColor = Bars.Close[0] > Bars.Close[1] ? 1 : 2;
				}
			}
			// If below average, move it to pink and gray
			Drawing.ColorIndex = barColor;
			lastBarColor = barColor;
		}
		
		public bool IsDown {
			get { return direction == Trend.Down; }
		}
		
		public bool IsUp {
			get { return direction == Trend.Up; }
		}
		
		public IndicatorCommon High {
			get { return high; }
		}
		
		public IndicatorCommon Average {
			get { return average; }
		}
		
		public IndicatorCommon Low {
			get { return low; }
		}
		
		public SMA AverageVolume {
			get { return averageVolume; }
		}
		
		private Strength strength = Strength.Lo;
		
		public Strength Strength {
			get { return strength; }
		}
		
		Trend direction = Trend.Flat;
		
		public Trend Direction {
			get { return direction; }
		}
		
		private void CalcVolumeStrength() {
			if( Count < 2) return;
			double current = this[0];
			double last = this[1];
			if( current > high[0]) {
				strength = Strength.Ex;
			} else if( current > average[0]) {
				strength = Strength.Hi;
			} else if( current > low[0]) {
				strength = Strength.Lo;
			} else {
				strength = Strength.DU;
			}
			if( this.Count > 1) {
				if( current > last + 5000) {
					direction = Trend.Up;
				} else if( current < last - 5000) {
					direction = Trend.Down;
				} else {
					direction = Trend.Flat;
				}
			}
		}
		
		public bool IsSideLineVolume {
			get { return averageVolume[0] < high[0]; }
		}
		
		public bool IsDryUp {
			get { return strength == Strength.DU; }
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

	public enum Strength {
		DU,
		Lo,
		Hi,
		Ex,
	}
}
