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
	public class Channel2
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Bars bars;
		int interceptBar = 0;
		int x1;
		int y1;
		int x2;
		int y2;		
		LinearRegression lr = new LinearRegression();
		LinearRegression lr2 = new LinearRegression();
		int upperLineId = 0; // for charts
		int lowerLineId = 0; // for charts
		int extraLineId = 0; // for reversal point, intrabar.
		double highestDev = 0;
		double lowestDev = 0;	
		int highestX = 0;
		int lowestX = 0;	
		double highestHigh = double.MinValue;
		double lowestLow = double.MaxValue;	
		double fttHighestHigh = double.MinValue;
		double fttLowestLow = double.MaxValue;	
		Color longColor = Color.LightGreen;
		Color shortColor = Color.Magenta;
		bool isCalculated = false;
		Trend direction = Trend.None;

		bool isDirectionSet = false;
		Trend traverseDirection = Trend.Flat;
		Doubles highMaxVolume = Factory.Engine.Doubles();
		Doubles lowMaxVolume = Factory.Engine.Doubles();
		
		public Channel2(Bars bars) 
		{
			this.bars = bars;
		}
		
		public void ResetMaxVolume() {
			highMaxVolume = Factory.Engine.Doubles();
			lowMaxVolume = Factory.Engine.Doubles();
			UpdateTick();
		}
		
		public int GetYFromBar(int bar) {
			return GetY(bar-interceptBar);
		}
		
		public void addPoint(int bar, int y) {
			lr.addPoint(bar-interceptBar, y);
		}
		
		public void addPoint2(int bar, int y) {
			lr2.addPoint(bar-interceptBar, y);
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
			if( ! Calculate2() ) {
//				double mult = (1 - (1 / (double) lr.Coord.Count));
//				lr.Slope = lr.Slope * mult;
			}
			if( !isDirectionSet) {
				direction = lr.Slope > 0 ? Trend.Up : lr.Slope < 0 ? Trend.Down : Trend.Flat;
				isDirectionSet = true;
			}
		}
		
		private bool Calculate2() {
			if( lr2.Coord.Count>1) {
				lr2.calculate();
				double newSlope = lr2.Slope; // (lr.Slope + lr2.Slope)/2;
				if( IsUp && newSlope > lr.Slope) {
					lr.Slope = newSlope;
					return true;
				}
				if( IsDown && newSlope < lr.Slope) {
					lr.Slope = newSlope;
					return true;
				}
			}
			return false;
		}
		
		public double CheckNewSlope(ChartPoint point) {
			LinearRegression tempLR = new LinearRegression(lr.Coord);
			tempLR.addPoint(point.Bar-interceptBar,point.Y);
			tempLR.calculate();
			double mult = (1 - (1 / (double) tempLR.Coord.Count));
			tempLR.Slope = tempLR.Slope * mult;
			return tempLR.Slope;
		}
		
		public void UpdateEnds() {
			if( !isCalculated) {
				throw new ApplicationException( "Linear Regression was never calculated.");
			}
			x1 = int.MinValue;
			for(int i=0; i<lr.Coord.Count; i++) {
				if( lr.Coord[i].X > x1) {
					x1 = (int) (lr.Coord[i].X);
				}
			}
			y1 = GetY(x1);
			x2 = x1;
			for(int i=0; i<lr.Coord.Count; i++) {
				if( lr.Coord[i].X < x2) {
					x2 = (int) (lr.Coord[i].X);
				}
			}
			y2 = GetY(x2);
		}
		
		public int GetY(int x) {
			return (int) (lr.Intercept + x*lr.Slope);
		}
		
		public void calcHighest(Bars bars) {
			// Find max deviation from the line.
			highestHigh = int.MinValue;
			lowestLow = int.MaxValue;	
			for( int x = x2; x <= x1 && x1-x < bars.Count; x++) {
				int lineY = GetY(x);
				double highY = bars.High[x1-x];
				double lowY = bars.Low[x1-x];
				if( x1-x < 4) {
					if( highY > highestHigh) {
						highestHigh = highY;
						highestX = x;
					}
					if( lowY < lowestLow) {
						lowestLow = lowY;
						lowestX = x;
					}
				}
			}
		}
		
		public void calcMaxDev(Bars bars) {
			calcHighest(bars);
			calcDeviation(bars);
		}
		
		public void calcDeviation(Bars bars) {
			// Find max deviation from the line.
			highestDev = 0;
			lowestDev = 0;
			for( int x = x2; x <= x1 && x1-x < bars.Count; x++) {
				double lineY = GetY(x);
				double highY = bars.High[x1-x];
				double lowY = bars.Low[x1-x];
				double highDev = highY - lineY;
				double lowDev = lineY - lowY;
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
		
		public void DrawChannel(Chart chart, int extend, bool drawDashedLines) {
			int extendBar = x1 + interceptBar + extend;
			int extendY = GetYFromBar( extendBar);
			Color topColor;
			Color botColor;
			topColor = botColor = direction==Trend.Up ? longColor : shortColor;
			if( IsUp ) {
				upperLineId = chart.DrawLine( topColor, extendBar, extendY + highestDev,
					                                x2+interceptBar, y2 + highestDev, LineStyle.Solid);
				if( drawDashedLines) {
					lowerLineId = chart.DrawLine( botColor, extendBar, extendY,
						                                x2+interceptBar, y2, LineStyle.Dashed);
				}
				extraLineId = chart.DrawLine( botColor, extendBar, extendY - lowestDev,
					                                x2+interceptBar, y2 - lowestDev, LineStyle.Solid);
			} else {
				extraLineId = chart.DrawLine( topColor, extendBar, extendY + highestDev,
					                                x2+interceptBar, y2 + highestDev, LineStyle.Solid);
				if( drawDashedLines) {
					upperLineId = chart.DrawLine( topColor, extendBar, extendY,
						                                x2+interceptBar, y2, LineStyle.Dashed);
				}
				lowerLineId = chart.DrawLine( botColor, extendBar, extendY - lowestDev,
					                                x2+interceptBar, y2 - lowestDev, LineStyle.Solid);
			}
		}

		public void RedrawChannel(Chart chart, int extend, bool drawDashedLines) {
			int extendBar = x1 + interceptBar + extend;
			int extendY = GetYFromBar( extendBar);
			Color topColor;
			Color botColor;
			topColor = botColor = direction==Trend.Up ? longColor : shortColor;
			if( IsUp ) {
				chart.ChangeLine( upperLineId, topColor, extendBar, extendY + highestDev,
			                 x2+interceptBar, y2 + highestDev, LineStyle.Solid);
				if( drawDashedLines) {
					chart.ChangeLine( lowerLineId, botColor, extendBar, extendY,
			                 x2+interceptBar, y2, LineStyle.Dashed);
				}
				chart.ChangeLine( extraLineId, botColor, extendBar, extendY - lowestDev,
			                 x2+interceptBar, y2 - lowestDev, LineStyle.Solid);
			} else {
				chart.ChangeLine( extraLineId, topColor, extendBar, extendY + highestDev,
			                 x2+interceptBar, y2 + highestDev, LineStyle.Solid);
				if( drawDashedLines) {
					chart.ChangeLine( upperLineId, topColor, extendBar, extendY,
			                 x2+interceptBar, y2, LineStyle.Dashed);
				}
				chart.ChangeLine( lowerLineId, botColor, extendBar, extendY - lowestDev,
			                 x2+interceptBar, y2 - lowestDev, LineStyle.Solid);
			}
		}

		public double Slope {
			get { return lr.Slope; }
			set { lr.Slope = value; }
		}
		
		public double Slope2 {
			get { return lr2.Slope; }
			set { lr2.Slope = value; }
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
			get { return x1+interceptBar; }
		}
		
		public int Y1 {
			get { return y1; }
		}
		
		public int Bar2 {
			get { return x2+interceptBar; }
		}
		
		public int Y2 {
			get { return y2; }
		}
		
		public int BarsInChannel {
			get { return CountPoints; }
		}
		
		public double Middle {
			get { 
				return (Top + Bottom)/2;
			}
		}
	
		public double UpperMiddle {
			get { 
				return Top - Math.Min(Width/5,50);
			}
		}
		
		public double LowerMiddle {
			get {
				return Bottom + Math.Min(Width/5,50);
			}
		}
		
		public double Top {
			get {
				double line = CurrentLine;
				return line + (IsDown ? highestDev : highestDev);
			}
		}
		
		public double Width {
			get {
				return Top - Bottom;
			}
		}
		
		protected int CurrentLine {
			get { return (int) GetYFromBar(bars.CurrentBar); }
		}
		
		public double Bottom {
			get { 
				int line = CurrentLine;
				return line - (IsUp ? lowestDev : lowestDev);
			}
		}
		
		public int Length {
			get{ return x1 - x2; }
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
		
		public int LowerLineId {
			get { return lowerLineId; }
			set { lowerLineId = value; }
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
		
		public int LastHighBar {
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
			} else if( tick.Ask > UpperMiddle) {
				status = ChannelStatus.Upper;
			} else if( tick.Bid < Bottom + 20) {
				status = ChannelStatus.Below;
			} else if( tick.Bid < LowerMiddle) {
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
		
	}
}
