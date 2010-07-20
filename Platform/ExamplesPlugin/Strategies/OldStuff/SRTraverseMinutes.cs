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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TickZoom.Api;

using TickZoom.Common;

namespace TickZoom
{
	public struct CodeStats {
		public int Count;
		public int CountBars;
		public double ProfitLoss;
	}
		
	public class SRTraverseMinutes : Strategy
	{
		int contractSize = 1;
		IndicatorCommon stretch;
		DOMRatio domRatio;
		IndicatorCommon velocity;
		IndicatorCommon longRetrace;
		IndicatorCommon shortRetrace;
		IndicatorCommon trend;
		LRGraph lrGraph;
		
		public SRTraverseMinutes()
		{
			ExitStrategy.ControlStrategy  = false;
			lastCodes = Integers(3);
			lastPrices = Doubles(3);
		}
		
		public override void OnInitialize()
		{
			
			velocity = new IndicatorCommon();
			velocity.Drawing.PaneType = PaneType.Secondary;
			velocity.Drawing.GraphType = GraphType.Histogram;
			velocity.Drawing.GroupName = "Velocity";
			AddIndicator(velocity);
			
			longRetrace = new IndicatorCommon();
			longRetrace.Drawing.Color = Color.Blue;
			longRetrace.Drawing.GroupName = "retrace";
			AddIndicator(longRetrace);
			
			lrGraph = new LRGraph();
			AddIndicator(lrGraph);
			
			shortRetrace = new IndicatorCommon();
			shortRetrace.Drawing.Color = Color.Blue;
			shortRetrace.Drawing.GroupName = "retrace";
			AddIndicator(shortRetrace);
			
			stretch = new IndicatorCommon();
			stretch.Drawing.Color = Color.Red;
			stretch.Drawing.PaneType = PaneType.Secondary;
			stretch.Drawing.GroupName = "Stretch";
			AddIndicator(stretch);
			
			trend = new IndicatorCommon();
			trend.Drawing.Color = Color.Red;
			trend.Drawing.GroupName = "Trend";
			AddIndicator(trend);
			
			domRatio = new DOMRatio();
			domRatio.Drawing.Color = Color.Green;
			AddIndicator(domRatio);
		}
		
		int positionsize;
		public override bool OnProcessTick(Tick tick)
		{
//			ProcessOrders(tick);
			return true;
		}
		
		protected void ProcessOrders(Tick tick) {
			if( Bars.Count==1) { Orders.Exit.ActiveNow.GoFlat(); Reset(); return; }
//			if( tick.AskDepth + tick.BidDepth < 3000) {
//			 TradeSignal.GoFlat();
//				Reset();
//				return;
//			}			
			
			positionsize = (int) Math.Max(0,(Math.Abs(stretch[0])/50));
			

			double profitTarget = Position.Size*50;
			int current = Performance.ComboTrades.Current;
			double profitInPosition = Position.HasPosition ? Performance.ComboTrades.ProfitInPosition(current,Performance.Equity.CurrentEquity) : 0;
			if( trendUp) {
				if( Position.IsShort) {
					Orders.Exit.ActiveNow.GoFlat();
				}
				if( Position.IsLong && positionsize == 0 && profitInPosition > profitTarget) {
					if( velocity[0] < -20) {
						Orders.Exit.ActiveNow.GoFlat();
					}
				}
				if( positionsize > Position.Size && velocity[0] > 10) {
					int newPositions = (int) (Position.Size+contractSize);
					Orders.Enter.ActiveNow.BuyMarket(newPositions);
				} 
			} else {
				if( Position.IsLong && velocity[0] < -20) {
					Orders.Exit.ActiveNow.GoFlat();
				}
//				if( positionsize > TradeSignal.Positions) {
//					int newPositions = (int) (TradeSignal.Positions+contractSize);
//				 TradeSignal.GoShort()(newPositions);
//				}
			}
		}
		
		public void xNewPeriod(Interval timeFrame) {
			if( timeFrame.Equals(Intervals.Day1)) {
				Tick tick = Ticks[0];
				double gap = Math.Abs(tick.Bid - tick.Bid);
				if( gap > 500) {
					Reset();
				}
			}
			
			if( timeFrame.Equals(IntervalDefault)) {
//				for( int i=0; i<Ticks.Count; i++) {
//					Elapsed diff = tick.Time - Ticks[i].Time;
//					if( diff.TotalSeconds > 300) {
//						velocity[0] = (tick.Ask+tick.Bid) - (Ticks[i].Ask+Ticks[i].Bid);
//						break;
//					}
//				}
				velocity[0] = Formula.Middle(Minutes,0) - Formula.Middle(Minutes,3);

				int longStretch = (int) (Bars.Low[0] - lastShortPrice);
				int shortStretch = (int) (lastLongPrice - Bars.High[0]);

				if( Bars.High[0] > longRetrace[0]) {
					longRetrace[0] = Bars.High[0];
					lastLongBar = Bars.CurrentBar;
					lastLongPrice = Bars.High[0];
				} else if( double.IsNaN(longRetrace[0])) {
					longRetrace[0] = Bars.High[0];
				} else {
					longRetrace[0] -= 1;
				}
				
				
				if( Bars.Low[0] < shortRetrace[0]) {
					shortRetrace[0] = Bars.Low[0];
					lastShortBar = Bars.CurrentBar;
					lastShortPrice = Bars.Low[0];
				} else if( double.IsNaN(shortRetrace[0])) {
					shortRetrace[0] = Bars.Low[0];
				} else {
					shortRetrace[0] += 1;
				}
				
				if( longStretch <= 0) longStretchSum = 0;
				longStretchSum += longStretch;
				if( shortStretch <= 0) shortStretchSum = 0;
				shortStretchSum += shortStretch;
							 
				trendUp = longStretchSum>shortStretchSum;
				stretch[0] = trendUp ? shortStretch : longStretch;
//				trend[0] = trendUp ? 1 : -1;
				trend[0] = longStretch - shortStretch;
			}
		}
		bool trendUp = false;
		long longStretchSum	= 0;
		long shortStretchSum = 0;
		int lastShortBar = 0;
		double lastShortPrice = 0;
		int lastLongBar = 0;
		double lastLongPrice = 0;
		
		Dictionary<long,CodeStats> codes = new Dictionary<long,CodeStats>();
		Integers lastCodes;
		Doubles lastPrices;
		int lastCode = 0;
//		int lastPricex = 0;
		int lastBar = 0;

		
		public override bool OnIntervalClose() {
			int newCode = lrGraph.Code;
//			switch( newCode) {
//				case 1: newCode = 0; break;
//				case 6: newCode = 7; break;
//			}
			stretch[0] = newCode;
			if( newCode != lastCode) {
				// Got a code change, close out the previous one.
				if( lastCodes.Count > 2) {
					int combinedCode = lastCodes[1] * 8 + lastCodes[0];
					CodeStats codeStats;
					if(codes.TryGetValue(combinedCode, out codeStats)) {
						codeStats.Count++;
						codeStats.CountBars+=(Bars.CurrentBar-lastBar);
						codeStats.ProfitLoss+=(Bars.Close[0]-lastPrices[0]);
						codes[combinedCode] = codeStats;
					} else {
						codeStats.Count = 1;
						codeStats.CountBars = 1;
						codeStats.ProfitLoss = 0;
						codes[combinedCode] = codeStats;
					}
				}
				// Alright start the new code.
				lastCodes.Add(newCode);
				lastPrices.Add(Bars.Close[0]);
				if( lastCodes.Count > 2) {
					int combinedCode = lastCodes[1] * 8 + lastCodes[0];
					TradeDecision(combinedCode);
				}
				lastBar = Bars.CurrentBar;
				lastCode = newCode;
			} else {
				
			}
//			switch( lrGraph.Code) {
//				case 0: TradeSignal.GoShort(); break;
//				case 1: TradeSignal.GoShort(); break;
//				case 2: TradeSignal.GoShort(); break;
//				case 3: TradeSignal.GoShort(); break;
//				case 4: TradeSignal.GoLong(); break;
//				case 5: TradeSignal.GoLong(); break;
//				case 6: TradeSignal.GoLong(); break;
//				case 7: TradeSignal.GoLong(); break;
//			}
			return true;
		}
		
		public void TradeDecision( int combinedCode) {
			switch( combinedCode) {
				case 1:
				case 8:
				case 11:
				case 26:
				case 19:
				case 16:
					Orders.Enter.ActiveNow.SellMarket(); break;
				default:
					Orders.Exit.ActiveNow.GoFlat(); break;
			}
		}
		
		public override void OnEndHistorical()
		{
			Log.Notice("Bars Processed: " + Bars.CurrentBar);
			Log.Notice("Code combo count = " + codes.Count );
			Log.Notice( "From,From,To,Count,Bars,AverageBars,ProfitLoss,AveragePL");
			foreach( KeyValuePair<long, CodeStats> kvp in codes) {
				Log.Notice( (kvp.Key / 64) + "," + ((kvp.Key%64) / 8) + "," + kvp.Key % 8 + "," +
				                      kvp.Value.Count + "," + kvp.Value.CountBars + "," + (kvp.Value.CountBars / kvp.Value.Count) + "," +
				                      kvp.Value.ProfitLoss + "," + (kvp.Value.ProfitLoss/kvp.Value.Count) );
			}
		}
		TimeStamp lastResetTime;
		public void Reset() {
			lastResetTime = Ticks[0].Time;
		}
	}
}
