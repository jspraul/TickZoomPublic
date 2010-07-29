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
	/// Description of Line.
	/// </summary>
	enum BarsToUse {
		Highs,
		Lows,
		Middle
	}
	public class LRLine 
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Bars bars;
		int interceptBar = 0;
		int breakoutBar = 0;
		int firstBar = int.MinValue;
		int lastBar;
		LinearRegression lr = new LinearRegression();
		int upperLineId = -1; // for charts
		int lowerLineId = -1; // for charts
		int lineId = -1; // for charts
		double highestDev = 0;
		double lowestDev = 0;	
		int highestX = 0;
		int lowestX = 0;	
		double highestHigh = double.MinValue;
		double lowestLow = double.MaxValue;	
		double fttHighestHigh = double.MinValue;
		double fttLowestLow = double.MaxValue;	
		Color longColor = Color.Green;
		Color shortColor = Color.Magenta;
		bool isCalculated = false;
		Trend direction = Trend.None;
		bool drawLines = true;
		bool reDrawLines = true;
		Chart chart = null;
		BarsToUse barsToUse = BarsToUse.Middle;
		double maxSlope = double.MaxValue;
		double minSlope = double.MinValue;

		Trend traverseDirection = Trend.Flat;
		Doubles highMaxVolume = Factory.Engine.Doubles();
		Doubles lowMaxVolume = Factory.Engine.Doubles();
		Model model;
		
		int length = 0;
		
		public LRLine(Model model, Bars bars, Chart chart, int length) 
		{
			this.model = model;
			this.bars = bars;
			this.length = length;
			this.DrawSolidLines = true;
			this.DrawDashedLines = true;
			this.chart = chart;
		}
		
		public void UseHighs() {
			barsToUse = BarsToUse.Highs; 
		}
		
		public void UseLows() {
			barsToUse = BarsToUse.Lows; 
		}
		
		public int GetYFromBar(int bar) {
			return GetY(bar-interceptBar);
		}
		
		public void AddBar(int bar) {
			switch( barsToUse) {
				case BarsToUse.Highs:
					lr.addPoint(bar-interceptBar, bars.High[bars.CurrentBar-bar]);
					break;
				case BarsToUse.Lows:
					lr.addPoint(bar-interceptBar, bars.Low[bars.CurrentBar-bar]);
					break;
				default:
				case BarsToUse.Middle:
					lr.addPoint(bar-interceptBar, model.Formula.Middle(bars,bars.CurrentBar-bar));
					break;
			}
		}
		
		public void ClearBars() {
			lr.clearPoints();
		}
		
		public void removePointsBefore( int bar) {
			lr.removePointsBefore(bar);
		}
		
		public double Correlation {
			get { return lr.Correlation; }
		}
		
		public void Calculate() {
			isCalculated = true;
			lr.calculate();
			lr.Slope = Math.Min( maxSlope, lr.Slope);
			lr.Slope = Math.Max( minSlope, lr.Slope);
			direction = lr.Slope > 0 ? Trend.Up : lr.Slope < 0 ? Trend.Down : Trend.Flat;
		}
		
		public void UpdateEnds() {
			if( !isCalculated) {
				throw new ApplicationException( "Linear Regression was never calculated.");
			}
			firstBar = int.MinValue;
			for(int i=0; i<lr.Coord.Count; i++) {
				if( lr.Coord[i].X > firstBar) {
					firstBar = (int) (lr.Coord[i].X);
				}
			}
			lastBar = firstBar;
			for(int i=0; i<lr.Coord.Count; i++) {
				if( lr.Coord[i].X < lastBar) {
					lastBar = (int) (lr.Coord[i].X);
				}
			}
		}
		
		int extend = 2;
		
		bool drawSolidLines = true;

		bool drawDashedLines = false;

		public void Update() {
			int bar=bars.CurrentBar;
			int lookBack = 1;
			int firstLineBar = bars.CurrentBar - (length-1);
			InterceptBar = firstLineBar;
			
			ClearBars();
			
			for( ; bar > bars.CurrentBar-bars.Count && bar >= firstLineBar; bar--) {
				AddBar( bar);
			}
			if( CountPoints > lookBack) {
				Calculate();
				UpdateEnds();
				calcMaxDev();
				if( drawLines ) {
					if( lineId < 0) {
						DrawChannel();
					} else {
						RedrawChannel();
					}
				}
			} else {
				return;
				throw new ApplicationException( "Must be at least 2 points for linear regression.");
			}
		}
		
		public int GetY(int x) {
			return (int) (lr.Intercept + x*lr.Slope);
		}
		
		public void calcHighest() {
			// Find max deviation from the line.
			highestHigh = int.MinValue;
			lowestLow = int.MaxValue;	
			for( int x = lastBar; x <= firstBar && firstBar-x < bars.Count; x++) {
				double high = bars.High[firstBar-x];
				double low = bars.Low[firstBar-x];
				if( firstBar-x < 4) {
					if( high > highestHigh) {
						highestHigh = high;
						highestX = x;
					}
					if( low < lowestLow) {
						lowestLow = low;
						lowestX = x;
					}
				}
			}
		}
		
		public void calcMaxDev() {
			calcHighest();
			calcDeviation();
		}
		
		public void calcDeviation() {
			// Find max deviation from the line.
			highestDev = 0;
			lowestDev = 0;
			for( int x = lastBar; x <= firstBar && firstBar-x < bars.Count; x++) {
				double highY = GetY(x);
				double high = bars.High[firstBar-x];
				double low = bars.Low[firstBar-x];
				double highDev = high - highY;
				double lowDev = highY - low;
				if( highDev > highestDev) {
					highestDev = highDev;
				}
				if( lowDev > lowestDev) {
					lowestDev = lowDev;
				}
			}
			CalcFTT(bars);
		}
		
		private void CalcFTT(Bars bars) {
			if( highMaxVolume.Count==0) {
				highMaxVolume.Add(0);
			} else {
				highMaxVolume.Add(highMaxVolume[0]);
			}
			if( lowMaxVolume.Count==0) {
				lowMaxVolume.Add(0);
			} else {
				lowMaxVolume.Add(lowMaxVolume[0]);
			}
			if( bars.High[0] > fttHighestHigh && bars.High[0] > bars.High[1]) {
				if( bars.Volume[0] > highMaxVolume[0] ) {
					highMaxVolume[0] = bars.Volume[0];
				}
				fttHighestHigh = bars.High[0];
			}
			if( bars.Low[0] < fttLowestLow && bars.Low[0] < bars.Low[1] ) {
				if( bars.Volume[0] > lowMaxVolume[0]) {
					lowMaxVolume[0] = bars.Volume[0];
					log.Debug("Low Max Volume Set to " + lowMaxVolume[0]);
				}
				fttLowestLow = bars.Low[0];
			}
		}
		
		public void DrawChannel() {
			int extendBar = firstBar + interceptBar + extend;
			int extendY = GetYFromBar( extendBar);
			int y2 = GetY(lastBar);
			Color topColor;
			Color botColor;
			topColor = botColor = direction==Trend.Up ? longColor : shortColor;
			if( drawDashedLines) {
				lineId = chart.DrawLine( topColor, extendBar, extendY,
					                                lastBar+interceptBar, y2, LineStyle.Dashed);
			}
			if( drawSolidLines) {
				upperLineId = chart.DrawLine( topColor, extendBar, extendY + highestDev,
					                                lastBar+interceptBar, y2 + highestDev, LineStyle.Solid);
				lowerLineId = chart.DrawLine( topColor, extendBar, extendY - lowestDev,
					                                lastBar+interceptBar, y2 - lowestDev, LineStyle.Solid);
			}
		}

		public void RedrawChannel() {
			int extendBar = firstBar + interceptBar + extend;
			int extendY = GetYFromBar( extendBar);
			int y2 = GetY(lastBar);
			Color topColor;
			Color botColor;
			topColor = botColor = direction==Trend.Up ? longColor : shortColor;
			if( drawDashedLines) {
				chart.ChangeLine( lineId, botColor, extendBar, extendY,
		                 lastBar+interceptBar, y2, LineStyle.Dashed);
			}
			if( drawSolidLines) {
				chart.ChangeLine( upperLineId, topColor, extendBar, extendY + highestDev,
			                 lastBar+interceptBar, y2 + highestDev, LineStyle.Solid);
				chart.ChangeLine( lowerLineId, topColor, extendBar, extendY - lowestDev,
			                 lastBar+interceptBar, y2 - lowestDev, LineStyle.Solid);
			}
		}

		public double Slope {
			get { return lr.Slope; }
			set { lr.Slope = value; }
		}
		
		// Try down and up true for slope = 0 (horizontal line)
		// so that we get correct top and bottom lines.
		public bool IsDown {
			get { return direction == Trend.Down; }
		}
		
		public bool IsUp {
			get { return direction == Trend.Up; }
		}
		
		public int Bar1 {
			get { return firstBar+interceptBar; }
		}
		
		public int Bar2 {
			get { return lastBar+interceptBar; }
		}
		
		public int BarsInChannel {
			get { return CountPoints; }
		}
		
		public double Middle {
			get { 
				return (Top + Bottom)/2;
			}
		}
	
		public double Top {
			get {
				int line = High;
				return line + highestDev;
			}
		}
		
		public double Width {
			get {
				return Top - Bottom;
			}
		}
		
		public int High {
			get { return (int) GetYFromBar(bars.CurrentBar); }
		}
		
		public double Bottom {
			get { 
				double line = High;
				return line - lowestDev;
			}
		}
		
		public int Length {
			get{ return firstBar - lastBar; }
		}
		
		public double HighestDev {
			get { return highestDev; }
		}
		
		public double LowestDev {
			get { return lowestDev; }
		}
		
		public int CountPoints {
			get { return lr.Coord.Count; }
		}
		
		public int UpperLineId {
			get { return upperLineId; }
			set { upperLineId = value; }
		}
		
		public int LineId {
			get { return lineId; }
			set { lineId = value; }
		}
		
		public int InterceptBar {
			get { return interceptBar; }
			set { interceptBar = value; }
		}
		
		public int LastLowBar {
			get { return lowestX + interceptBar; }
		}
		
		public double LastLowPrice {
			get { return lowestLow; }
		}
		
		public int LastBar {
			get { return highestX + interceptBar; }
		}
		
		public Color LongColor {
			get { return longColor; }
			set { longColor = value; }
		}
		
		public Color ShortColor {
			get { return shortColor; }
			set { shortColor = value; }
		}
		
		Tick tick;
		ChannelStatus status = ChannelStatus.Inside;
		
		public ChannelStatus Status {
			get { return status; }
		}
		
		private void UpdateTick() {
			if( tick.Ask > Top - 20) {
				status = ChannelStatus.Above;
			} else if( tick.Ask > High) {
				status = ChannelStatus.Upper;
			} else if( tick.Bid < Bottom + 20) {
				status = ChannelStatus.Below;
			} else if( tick.Bid < High) {
				status = ChannelStatus.Lower;
			} else  {
				status = ChannelStatus.Inside;
			}
			switch( status) {
				case ChannelStatus.Above:
				case ChannelStatus.Upper:
					traverseDirection = Trend.Down;
					break;
				case ChannelStatus.Below:
				case ChannelStatus.Lower:
					traverseDirection = Trend.Up;
					break;
			}
			isLongFTT = highMaxVolume.Count>0 && IsUp && tick.Ask > fttHighestHigh && bars.Volume[0] < highMaxVolume[0] - 5000;
			isShortFTT = lowMaxVolume.Count>0 && IsDown && tick.Bid < fttLowestLow && bars.Volume[0] < lowMaxVolume[0] - 5000;
//			if( TradeSignal.IsShortFTT) {
//				TickConsole.WriteFile( "TradeSignal.IsShortFTT: isLow="+(lowMaxVolume.Count>0 && IsDown && tick.Bid < fttLowestLow)+", volume="+bars.Volume[0] +", lowMaxVolume="+(lowMaxVolume[0]-5000));
//			}
			isLongTraverse = highMaxVolume.Count>0 && IsUp && tick.Ask > fttHighestHigh && bars.Volume[0] > highMaxVolume[0];
			isShortTraverse = lowMaxVolume.Count>0 && IsDown && tick.Bid < fttLowestLow && bars.Volume[0] > lowMaxVolume[0];
		}
	
		public Tick Tick {
			get { return tick; }
			set { tick = value; UpdateTick(); }
		}
		
		public void LogShortTraverse() {
			log.Debug( "LogShortTraverse=" + (lowMaxVolume.Count>0 && IsDown && tick.Bid < fttLowestLow) +
			                       ", volume=" + bars.Volume[0] +
			                       ", low max volume=" +lowMaxVolume[0]);
		}
		
		bool isLongFTT = false;
		bool isShortFTT = false;
		bool isLongTraverse = false;
		bool isShortTraverse = false;
		
		public bool IsLongTraverse {
			get { return isLongTraverse; }
		}
		
		public bool IsShortTraverse {
			get { return isShortTraverse; }
		}

		public bool IsLongFTT {
			get { return isLongFTT; }
		}
		
		public bool IsShortFTT {
			get { 
//				if( TradeSignal.IsShortFTT && lowMaxVolume.Count>0) {
//					TickConsole.WriteFile( "TradeSignal.IsShortFTT: isLow="+(IsDown && tick.Bid < fttLowestLow)+", volume="+bars.Volume[0] +", lowMaxVolume="+(lowMaxVolume[0]-5000));
//				}
				return isShortFTT; }
		}
		
		public bool IsAbove {
			get { return status == ChannelStatus.Above; }
		}
		
		public bool IsGoingUp {
			get { return traverseDirection == Trend.Up; }
		}
		
		public bool IsGoingDown {
			get { return traverseDirection == Trend.Down; }
		}
		
		public bool IsBelow {
			get { return status == ChannelStatus.Below; }
		}
		
		public bool IsUpper {
			get { return status == ChannelStatus.Upper; }
		}
		
		public bool IsLower {
			get { return status == ChannelStatus.Lower; }
		}
		
		public bool IsInside {
			get { return status == ChannelStatus.Inside; }
		}
		
		public enum ChannelStatus {
			Above,
			Upper,
			Inside,
			Lower,
			Below,
		}
		
		public Trend TraverseDirection {
			get { return traverseDirection; }
		}
		
		public double HighMaxVolume {
			get { return highMaxVolume[0]; }
		}
		
		public double LowMaxVolume {
			get { return lowMaxVolume[0]; }
		}
		
		public bool IsHalfLowMaxVolume {
			get { return bars.Volume.Count>0 && lowMaxVolume.Count>0 && bars.Volume[0] * 2 < lowMaxVolume[0]; }
		}
		
		public bool IsHalfHighMaxVolume {
			get { return bars.Volume.Count>0 && lowMaxVolume.Count>0 && bars.Volume[0] * 2 < highMaxVolume[0]; }
		}
		
		public Trend Direction {
			get { return direction; }
		}
		
		public bool DrawLines {
			get { return drawLines; }
			set { drawLines = value; }
		}
		
		public bool ReDrawLines {
			get { return reDrawLines; }
			set { reDrawLines = value; }
		}
		
		public int Extend {
			get { return extend; }
			set { extend = value; }
		}
		
		public bool DrawSolidLines {
			get { return drawSolidLines; }
			set { drawSolidLines = value; }
		}
		
		public bool DrawDashedLines {
			get { return drawDashedLines; }
			set { drawDashedLines = value; }
		}
		
		public Chart Chart {
			get { return chart; }
			set { chart = value; }
		}
		
		public int BreakoutBar {
			get { return breakoutBar; }
			set { breakoutBar = value; }
		}
		
		public double MinSlope {
			get { return minSlope; }
			set { minSlope = value; }
		}
		
		public double MaxSlope {
			get { return maxSlope; }
			set { maxSlope = value; }
		}
	}
}
