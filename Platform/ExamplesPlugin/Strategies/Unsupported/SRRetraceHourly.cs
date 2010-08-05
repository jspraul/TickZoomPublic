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
	public class SRRetraceHourly : Strategy
	{
		Retrace retrace;
		int contractSize = 1;
		IndicatorCommon stretch;
		DOMRatio domRatio;
		
		public SRRetraceHourly()
		{
			ExitStrategy.ControlStrategy  = false;
			IntervalDefault = Intervals.Define(BarUnit.Change,50);
		}
		
		public override void OnInitialize()
		{
			base.OnInitialize();
			retrace = new Retrace();
			retrace.Drawing.Color = Color.Red;
			retrace.IntervalDefault = Intervals.Hour1;
			AddIndicator(retrace);
			
			stretch = new IndicatorCommon();
			stretch.Drawing.Color = Color.Red;
			stretch.Drawing.PaneType = PaneType.Secondary;
			stretch.IntervalDefault = Intervals.Hour1;
			AddIndicator(stretch);
			
			domRatio = new DOMRatio();
			domRatio.Drawing.Color = Color.Green;
			domRatio.Drawing.PaneType = PaneType.Secondary;
			domRatio.IntervalDefault = Intervals.Minute1;
			AddIndicator(domRatio);
			
			Reset(); // Initialize
		}
		
		double largestSize = 0;
		double positionsize;
		double newPositions;
		public override bool OnProcessTick(Tick tick)
		{
			if( Hours.Count==1) { Orders.Exit.ActiveNow.GoFlat(); Reset(); return true; }
			if( Position.HasPosition) {
				// TODO: Handle commission costs correctly inside Trade object.
				int profitTarget = (int) (100 + Position.Size*contractSize*10); // *2 to double cost of commission.
				if( Performance.ComboTrades.OpenProfitLoss >= profitTarget) {
					Orders.Exit.ActiveNow.GoFlat();
					Reset();
					return true;
				}
			}
			
			positionsize = Math.Max(0,(retrace.Stretch/100)-1);
			newPositions = Position.Size;
			
			if( positionsize > largestSize) {
				newPositions = Position.Size+contractSize;
				largestSize = positionsize;
				if( retrace.Stretch > 1000) { newPositions+=contractSize; };
				if( retrace.Stretch > 1500) { newPositions+=contractSize; };
				if( retrace.Stretch > 2000) { newPositions+=contractSize; };
				if( retrace.Stretch > 2500) { newPositions+=contractSize; };
				if( retrace.Stretch > 3000) { newPositions+=contractSize; };
				if( retrace.Stretch > 3500) { newPositions+=contractSize; };
			}
			
			if(  (Position.IsFlat||Position.IsLong) && Ticks[0].Bid < retrace[0]) {
				Orders.Enter.ActiveNow.BuyMarket( newPositions);
			} 
			
			if(  (Position.IsFlat||Position.IsShort) && Ticks[0].Ask > retrace[0]) {
				Orders.Enter.ActiveNow.SellMarket( newPositions);
			}
			return true;
		}
		
		public override bool OnIntervalOpen(Interval timeFrame) {
			if( timeFrame.Equals(Intervals.Day1)) {
				double gap = Math.Abs(Ticks[0].Bid - Ticks[1].Bid);
				if( gap > 500) {
					Reset();
//				 TradeSignal.GoFlat();
				}
			}
			
			if( timeFrame.Equals(Intervals.Minute1)) {
				stretch[0] = retrace.Stretch;
			}
			return true;
		}
		
		TimeStamp lastResetTime;
		public void Reset() {
			largestSize = 0;
			lastResetTime = Ticks[0].Time;
			if( retrace.Count > 0) {
				retrace.Reset();
			}
		}
	}
}
