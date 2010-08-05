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
	public class SRRetraceMinutes : Strategy
	{
		Retrace retrace;
		int contractSize = 1;
		IndicatorCommon stretch;
		DOMRatio domRatio;
		IndicatorCommon velocity;
		LRGraph lrGraph;
		
		public SRRetraceMinutes()
		{
			ExitStrategy.ControlStrategy  = false;
		}
		
		public override void OnInitialize()
		{
			
			base.OnInitialize();
			retrace = new Retrace();
			retrace.Drawing.Color = Color.Red;
			AddIndicator(retrace);
			
			velocity = new IndicatorCommon();
			velocity.Drawing.PaneType = PaneType.Secondary;
			velocity.Drawing.GraphType = GraphType.Histogram;
			velocity.Drawing.GroupName = "Velocity";
			AddIndicator(velocity);
			
			lrGraph = new LRGraph();
			AddIndicator(lrGraph);
			
			stretch = new IndicatorCommon();
			stretch.Drawing.Color = Color.Red;
			stretch.Drawing.GroupName = "Stretch";
			AddIndicator(stretch);
			
			domRatio = new DOMRatio();
			domRatio.Drawing.Color = Color.Green;
			AddIndicator(domRatio);
			
			Reset(); // Initialize
		}
		
		double positionsize;
		double newPositions;
		public override bool OnProcessTick(Tick tick)
		{
			if( Bars.Count==1) { Orders.Exit.ActiveNow.GoFlat(); Reset(); return true; }
//			if( tick.AskDepth + tick.BidDepth < 3000) {
//			 TradeSignal.GoFlat();
//				Reset();
//				return;
//			}			
			
			for( int i=0; i<Ticks.Count; i++) {
				Elapsed diff = tick.Time - Ticks[i].Time;
				if( diff.TotalSeconds > 120) {
					velocity[0] = (tick.Ask+tick.Bid) - (Ticks[i].Ask+Ticks[i].Bid);
					break;
				}
			}

			if( Position.HasPosition) {
				// TODO: Handle commission costs correctly inside Trade object.
				int profitTarget = (int) (Position.Size*30); // *2 to double cost of commission.
				int current = Performance.ComboTrades.Current;
				if( Performance.ComboTrades.ProfitInPosition(current,Performance.Equity.CurrentEquity) >= profitTarget) {
					Orders.Exit.ActiveNow.GoFlat();
					Reset();
					return true;
				}
			}
			
			positionsize = Math.Max(0,(retrace.Stretch/100));
			newPositions = Position.Size;
			
			if( positionsize > Position.Size) {
				newPositions = Position.Size+contractSize;
//				if( retrace.Stretch > 1000) { newPositions+=contractSize; };
//				if( retrace.Stretch > 1500) { newPositions+=contractSize; };
//				if( retrace.Stretch > 2000) { newPositions+=contractSize; };
//				if( retrace.Stretch > 2500) { newPositions+=contractSize; };
//				if( retrace.Stretch > 3000) { newPositions+=contractSize; };
//				if( retrace.Stretch > 3500) { newPositions+=contractSize; };
			
				if(  (Position.IsFlat||Position.IsLong) && tick.Bid < retrace[0]) {
					if( velocity[0]>0) {
						Orders.Enter.ActiveNow.BuyMarket( newPositions);
					}
				} 
				
				if(  (Position.IsFlat||Position.IsShort) && tick.Ask > retrace[0]) {
					if( velocity[0]<0 ) {
						Orders.Enter.ActiveNow.SellMarket( newPositions);
					}
				}
			}
			return true;
		}
		
		public override bool OnIntervalOpen(Interval timeFrame) {
			if( timeFrame.Equals(Intervals.Day1)) {
				Tick tick = Ticks[0];
				double gap = Math.Abs(tick.Bid - tick.Bid);
				if( gap > 500) {
					Reset();
				}
			}
			
			if( timeFrame.Equals(IntervalDefault)) {
				stretch[0] = retrace.Stretch;
			}
			return true;
		}
		
		TimeStamp lastResetTime;
		public void Reset() {
			lastResetTime = Ticks[0].Time;
			if( retrace.Count > 0) {
				retrace.Reset();
			}
		}
	}
}
