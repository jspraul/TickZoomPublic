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
using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of EMA.
	/// </summary>
	public class SCT : IndicatorCommon
	{
		Volume volume;
		Integers volumePeaks;
		IndicatorCommon smartMoney;
		IndicatorCommon pace;
//		int avgVolLength = 65;
		public SCT() : base()
		{
			Drawing.Color = Color.Green;
			volumePeaks = Integers();
		}
		
		public override void OnInitialize() {
			
			smartMoney = new IndicatorCommon();
			smartMoney.Drawing.GraphType = GraphType.Histogram;
			smartMoney.Drawing.GroupName = "Smart $";
			AddIndicator( smartMoney );
			
			volume = new Volume();
			AddIndicator( volume );
			
			pace = new IndicatorCommon();
			pace.Drawing.GraphType = GraphType.Histogram;
			pace.Drawing.GroupName = "Pace";
			AddIndicator( pace );
//			
//			volumeFRV = new Indicator();
//			volumeFRV.PaneType = PaneType.Secondary;
//			volumeFRV.Color = Color.Magenta;
//			volumeFRV.GroupName = "Volume";
//			AddIndicator( volumeFRV );
//			
//			volumeDryUp = new Indicator();
//			volumeDryUp.PaneType = PaneType.Secondary;
//			volumeDryUp.Color = Color.Magenta;
//			volumeDryUp.GroupName = "Volume";
//			AddIndicator( volumeDryUp );
//			
//			volumeMax = new Indicator();
//			volumeMax.PaneType = PaneType.Secondary;
//			volumeMax.Color = Color.Black;
//			volumeMax.GroupName = "Volume";
//			AddIndicator( volumeMax );
		}

//		int highestVolume = 0;
		public override bool OnIntervalClose()
		{
			if( IntervalDefault.BarUnit == BarUnit.Tick ) {
				Elapsed span = Ticks[0].Time - Bars.Time[0];
				smartMoney[0] = (Bars.Sentiment[0]) / span.TotalSeconds;
				pace[0] = (Formula.Middle(Bars,0) - Formula.Middle(Bars,1)) *10000 / span.TotalMilliseconds;
			} else {
				smartMoney[0] = Bars.Sentiment[0];
				pace[0] = Bars.Close[0] - Bars.Close[1];
			}
			return true;
		}
		
		public int ProRatedVolume {
			get { 
				Elapsed timeInBar = Ticks[0].Time - Bars.Time[0];
				return (int) (Bars.Volume[0] * 60 / timeInBar.TotalSeconds);
			}
		}
		
		public int TickPace { 
			get { 
				if( Ticks.Count < 100) return 0;
				Elapsed span = Ticks[0].Time - Ticks[99].Time;
				int volume = 0;
				for( int i=0; i<Ticks.Count && i<100; i++) {
					volume+=Ticks[i].Volume;
				}
				return (int) (volume / span.TotalSeconds);
			}
		}
		
		private Color CalcGradient(Color start, Color end, int steps, int count) {
			int red = ((int)end.R - (int)start.R) / (steps - 1) * count + start.R;
			int green = ((int)end.G - (int)start.G) / (steps - 1) * count + start.G;
			int blue = ((int)end.B - (int)start.B) / (steps - 1) * count + start.B;
			return Color.FromArgb(red,green,blue);
		}
		
		public void ResetVolume() {
			this[0] = 0;
		}
		
		public IndicatorCommon SmartMoney {
			get { return smartMoney; }
		}
		
		public IndicatorCommon Volume {
			get { return volume; }
		}
		
		public IndicatorCommon Pace {
			get { return pace; }
		}
	}
}
