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

using TickZoom.Api;

using TickZoom.Common;

namespace TickZoom
{
	/// <summary>
	/// Description of EMA.
	/// </summary>
	public class Traversal : IndicatorCommon
	{
		PivotLowVs low;
		PivotHighVs high;
		Integers zigBars;
		Doubles zigHighs;
		Integers zagBars;
		Doubles zagLows;
		int threshold = 100;
		int costs = 25;
		TimeStamp simTradeStart;
		TimeStamp simTradeLast;
		bool debug = true;
		Elapsed startMorning = new Elapsed(00,15,0);
		Elapsed endMorning = new Elapsed(14,0,0);
		
		// Fields that must be reset outside trading times.
		double simTradeTotal = 0;
		int simTradeCount = 0;
		int lastHighBar = 0;
		int lastLowBar = 0;
		double lastHighPrice = 0;
		double lastLowPrice = double.MaxValue;
		
		public Traversal() : base()
		{
			zigBars = Integers(5);
			zigHighs = Doubles(5);
			zagBars = Integers(5);
			zagLows = Doubles(5);
			Drawing.Color = Color.LightGreen;
		}
		
		public void Reset() {
			simTradeTotal = 0;
			simTradeCount = 0;
			lastHighBar = 0;
			lastLowBar = 0;
			lastHighPrice = 0;
			lastLowPrice = int.MaxValue;
			zigBars.Clear();
			zagBars.Clear();
			zigHighs.Clear();
			zagLows.Clear();
		}
		
		public override void OnInitialize() {
			low = new PivotLowVs(1,1);
			low.IntervalDefault = IntervalDefault;
			low.DisableBoxes = true;
			AddIndicator(low);
			
			high = new PivotHighVs(1,1);
			high.IntervalDefault = IntervalDefault;
			high.DisableBoxes = true;
			AddIndicator(high);
		}
		
		bool drawHigh = true;
		public override bool OnIntervalClose()
		{
			Tick tick = Ticks[0];
//			if( Bars.Time[0].TimeOfDay >= StartMorning && Bars.Time[1].TimeOfDay < StartMorning ) {
//				Reset();
//			}
			CheckForConfirmedPivot();
			FindNewPivots();
			return true;
//			if( tick.Time.TimeOfDay < StartMorning) {
//				FindNewPivots();
//			} else if( tick.Time.TimeOfDay > EndMorning) {
//				Reset();
//			} else {
//				CheckForConfirmedPivot();
//				FindNewPivots();
//			}
		}

		public void FindNewPivots() {
			if( drawHigh && high.PivotBars.Count > 0 &&
				high.PivotBars[0] > lastLowBar &&
				high.PivotHighs[0] > lastHighPrice) {
				lastHighBar = high.PivotBars[0];
				lastHighPrice = high.PivotHighs[0];
			}
			if( !drawHigh && low.PivotBars.Count > 0 &&
			    low.PivotBars[0] > lastHighBar && 
			    low.PivotLows[0] < lastLowPrice) {
				lastLowBar = low.PivotBars[0];
				lastLowPrice = low.PivotLows[0];
			}
		}
	
		public void CheckForConfirmedPivot() {
			if( drawHigh && Bars.Low[0] <= lastHighPrice - threshold) {
				if( zagBars.Count == 0 || lastHighBar > zagBars[0]) {
					zigBars.Add( lastHighBar);
					zigHighs.Add( lastHighPrice);
					drawHigh = false;
					if( zagBars.Count>0) {
						Chart.DrawLine(Drawing.Color,zagBars[0],lastLowPrice,
					    	zigBars[0],lastHighPrice,LineStyle.Solid);
//						DrawDownTrend();
//						DrawHorizontal(lastHighPrice-100);
						LogNewHigh();
						lastLowPrice = int.MaxValue;
					}
				}
			} else if( !drawHigh && Bars.High[0] >= lastLowPrice + threshold) {
				if( zigBars.Count == 0 || lastLowBar > zigBars[0]) {
					zagBars.Add( lastLowBar);
					zagLows.Add( lastLowPrice);
					drawHigh = true;
					if( zigBars.Count>0) {
						Chart.DrawLine(Drawing.Color,zagBars[0],lastLowPrice,
					    	zigBars[0],lastHighPrice,LineStyle.Solid);
//						DrawUpTrend();
//						DrawHorizontal(lastLowPrice+100);
						LogNewLow();
						lastHighPrice = 0;
					}
				}
			}
		}
		
		public void DrawHorizontal(int price) {
			Chart.DrawLine(Color.Purple,Bars.CurrentBar,price,Bars.CurrentBar+5,price,LineStyle.Dashed);
		}
		
		public void DrawUpTrend() {
			if( zagBars.Count > 1) {
				int currBar = Bars.CurrentBar-zagBars[0];
				int prevBar = Bars.CurrentBar-zagBars[1];
				int length = zagBars[0] - zagBars[1];
				double currPrice = Bars.Low[currBar];
				double prevPrice = Bars.Low[prevBar];
				double height = prevPrice - currPrice;
				if( currPrice >= prevPrice ) {
					double slope = (height) / (length);
					int nextBar = zagBars[0] + length*2;
					double nextPrice = currPrice - height*2;
					Chart.DrawLine(Color.Blue,zagBars[0],currPrice,nextBar,nextPrice, LineStyle.Dashed);
				}
			}
		}
		
		public void DrawDownTrend() {
			if( zigBars.Count > 1) {
				int currBar = Bars.CurrentBar-zigBars[0];
				int prevBar = Bars.CurrentBar-zigBars[1];
				int length = zigBars[0] - zigBars[1];
				double currHigh = Bars.High[currBar];
				double prevHigh = Bars.High[prevBar];
				double height = currHigh - prevHigh;
				double simProfit = SimTradeProfit();
				if( currHigh <= prevHigh ) {
					double slope = (height) / (length);
					int nextBar = zigBars[0] + length*2;
					double nextHigh = currHigh + height*2;
					Chart.DrawLine(Color.Magenta,zigBars[0],currHigh,nextBar,nextHigh, LineStyle.Dashed);
				}
			}
		}
		
		public void LogNewHigh() {
			if( debug && Ticks[0].Time.TimeOfDay >= StartMorning && Ticks[0].Time.TimeOfDay <= EndMorning ) {
				int prevBar = Bars.CurrentBar-zagBars[0];
				int currBar = Bars.CurrentBar-lastHighBar;
				string traversal = "None";
				if( zagBars.Count>1) {
					int trendBar = Bars.CurrentBar-zagBars[1];
					traversal = Bars.Low[prevBar] > Bars.Low[trendBar] ? "Dom" : "Sub";
				}
	   			Log.Notice("High," + traversal + "," + Bars.Time[currBar] + "," + Bars.Low[currBar] + "," + SimTradeProfit() + "," + (simTradeTotal-simTradeCount*costs) + ", " + SimTradePreviousTimeStr(prevBar,currBar) + "," + SimTradeAverageTimeStr());
				// If we got an above average traversal, start the timer over.
//				if( IsPastAverageTime ) {
					simTradeLast = Bars.Time[currBar];
//				}
			}
		}
		
		public void LogNewLow() {
			if( debug && Ticks[0].Time.TimeOfDay >= StartMorning && Ticks[0].Time.TimeOfDay <= EndMorning ) {
				int prevBar = Bars.CurrentBar-zigBars[0];
				int currBar = Bars.CurrentBar-lastLowBar;
				string traversal = "None";
				if( zigBars.Count>1) {
					int trendBar = Bars.CurrentBar-zigBars[1];
					traversal = Bars.High[prevBar] < Bars.High[trendBar] ? "Dom" : "Sub";
				}
				Log.Notice("Low," + traversal + "," + Bars.Time[currBar] + "," + Bars.Low[currBar] + "," + SimTradeProfit() + "," + (simTradeTotal-simTradeCount*costs) + "," + SimTradePreviousTimeStr(prevBar,currBar) + "," + SimTradeAverageTimeStr());
				// If we got an above average traversal, start the timer over.
//				if( IsPastAverageTime ) {
					simTradeLast = Bars.Time[currBar];
//				}
			}
		}
		
		
		
		public double SimTradeProfit() {
			double simProfit = 0;
			if( simTradeCount > 0) {
				int highBar = Bars.CurrentBar-zigBars[0];
				int lowBar = Bars.CurrentBar-zagBars[0];
				simProfit = Bars.High[highBar] - Bars.Low[lowBar];
				simTradeTotal += simProfit;
			} else {
				simTradeStart = Ticks[0].Time;
			}
			simTradeCount++;
			return simProfit;
		}
		
		public int Pace {
			get { return (int) ((IsTraverseLong ? Ticks[0].Bid - Bars.Low[Bars.CurrentBar-zagBars[0]] :
			                     Bars.High[Bars.CurrentBar-zigBars[0]] - Ticks[0].Ask) / SimTradeCurrentTime.TotalMinutes); }
		}
		
		public Elapsed SimTradeAverageTime {
			get { if( simTradeCount > 0) {
					Elapsed elapsed = Ticks[0].Time - simTradeStart;
					long avg = (elapsed.TotalDays / simTradeCount) * 100 / 90;
					return new Elapsed(avg);
				} else {
					return new Elapsed();
				}
			}
		}
		
		public bool IsDownTrend {
			get { if( zigBars.Count < 2 || zagBars.Count < 1) return false;
				  if( zigHighs[0] <= zigHighs[1] && Bars.Close[0] < zigHighs[0]) return true;
				  return false;
			}
		}
		
		public bool IsUpTrend {
			get { if( zagBars.Count < 2 || zigBars.Count < 1) return false;
				  if( zagLows[0] >= zagLows[1] && Bars.Close[0] > zagLows[0]) return true;
				  return false;
			}
		}
		
		public bool IsTraverseLong {
			get { return zigBars.Count > 0 && zagBars.Count > 0 && zagBars[0] > zigBars[0]; }
		}
		
		public bool IsTraverseShort {
			get { return zigBars.Count > 0 && zagBars.Count > 0 && zigBars[0] > zagBars[0]; }
		}
		
		public bool IsPastAverageTime {
			get { return SimTradeCurrentTime > SimTradeAverageTime; }
		}
		
		public Elapsed SimTradePreviousTime(int startBar, int endBar) {
			return Bars.Time[endBar] - Bars.Time[startBar];
		}
		
		public Elapsed SimTradeCurrentTime {
			get { return Ticks[0].Time - simTradeLast; }
		}

		public string SimTradePreviousTimeStr(int startBar, int endBar) {
			Elapsed elapsed = SimTradePreviousTime(startBar, endBar);
			return String.Format("00:{0:00}:{0:00}",elapsed.TotalMinutes,elapsed.TotalSeconds);
		}

		
		public string SimTradeAverageTimeStr() {
			Elapsed elapsed = SimTradeAverageTime;
			return String.Format("00:{0:00}:{0:00}",elapsed.TotalMinutes,elapsed.TotalSeconds);
		}
		
		public string SimTradeCurrentTimeStr() {
			Elapsed elapsed = SimTradeCurrentTime;
			return String.Format("00:{0:00}:{0:00}",elapsed.TotalMinutes,elapsed.TotalSeconds);
		}
		
		public Elapsed StartMorning {
			get { return startMorning; }
			set { startMorning = value; }
		}
		
		public Elapsed EndMorning {
			get { return endMorning; }
			set { endMorning = value; }
		}
	}
}
