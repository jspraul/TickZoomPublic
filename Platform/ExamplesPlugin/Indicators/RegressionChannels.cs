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
using System.IO;

using TickZoom.Api;

using TickZoom.Common;

namespace TickZoom
{

	/// <summary>
	/// Description of SMA.
	/// </summary>
	public class RegressionChannels : IndicatorCommon
	{
		Series<Channel> lines;
		
		IndicatorCommon slope;
		bool reDrawLines = false;
		bool drawDashedLines = true;
		bool drawLines = true;
		Series<ChartPoint> highs;
		Series<ChartPoint> lows;
		int extend = 10;
		Color longColor = Color.Green;
		Color shortColor = Color.Red;
		bool hasNewChannel;
		bool isActivated = true;

//		Trend trend = Trend.Flat;
			
		public RegressionChannels()
		{
			lines = Series<Channel>();
			highs = Series<ChartPoint>();
			lows = Series<ChartPoint>();
		}

		public override void OnInitialize()
		{
			slope = new IndicatorCommon();
			slope.Drawing.GroupName = "Slope";
			AddIndicator(slope);
		}
		
		public void Reset() {
			lines.Clear();
		}
		
		public override bool OnIntervalClose(Interval timeFrame) {
//			if( !isActivated ) { return; }
			
			if( timeFrame.Equals(IntervalDefault)) {
				hasNewChannel = false;
				
				
				if( lines.Count == 0) {
					NewChannel(Bars.CurrentBar, Trend.Flat);
					return true;
				} 

				ContinueOrEnd();
//				if( lines[0].IsUp && direction != Trend.Up) {
//					NewChannel(lines[0].LastLowBar,Trend.Up);					
//				}
//				if( lines[0].IsDown && direction != Trend.Down) {
//					NewChannel(lines[0].LastHighBar,Trend.Down);					
//				}
				
				if( lines.Count > 1 ) {
					Channel line = lines[1];
					if( line.IsUp) {
						int lastHighBar = line.LastHighBar;
						NewHigh(lastHighBar,Bars.High[Bars.CurrentBar-lastHighBar]);
					}
					if( line.IsDown) {
						int lastLowBar = line.LastLowBar;
						NewLow(lastLowBar,Bars.Low[Bars.CurrentBar-lastLowBar]);
					}
					slope[0] = lines[0].Slope;
				}
			}
			return true;
		}

		public virtual void ContinueOrEnd() {
			bool longTrend = lines[0].IsUp;
			bool shortTrend = lines[0].IsDown;
			double high = Bars.High[0];
			double close = Bars.Close[0];
			double low = Bars.Low[0];
			double top = lines[0].Top;
			double bottom = lines[0].Bottom;
			if( lines[0].IsDown) {
				if( Bars.Close[0] > lines[0].Top) {
					NewChannel(lines[0].LastLowBar,Trend.Flat);
					return;
				}
				if( Bars.Volume[0] > 266000) {
					NewChannel(lines[0].LastLowBar,Trend.Flat);
					return;
				}
			}
			if (lines[0].IsUp) {
				if( Bars.Close[0] < lines[0].Bottom) {
					NewChannel(lines[0].LastHighBar,Trend.Flat);
					return;
				}
				if( Bars.Volume[0] > 266000) {
					NewChannel(lines[0].LastHighBar,Trend.Flat);
					return;
				}
			}
			TryExtendChannel();
			return;
		}
		
		public void NewHigh( int bar, double price) {
			if( highs.Count > 0 && bar == highs[0].X) return;
//			Chart.DrawBox(Color.Blue,bar,price);
			highs.Add(new ChartPoint(bar,price));
//			if( false && highs.Count > 1) {
//				Line line = new Line(Bars.CurrentBar);
//				line.setPoints(highs[0],highs[1]);
//				line.calculate();
//				line.extend( 20);
//				Chart.DrawLine(Color.Blue,highs[0].X,highs[0].Y,line.Bar1,line.Y1,LineStyle.Solid);
//			}
		}
		
		public void NewLow( int bar, double price) {
			if( lows.Count > 0 && bar == lows[0].X) return;
//			Chart.DrawBox(Color.Blue,bar,price);
			lows.Add(new ChartPoint(bar,price));
//			if( false && lows.Count > 1) {
//				Line line = new Line(Bars.CurrentBar);
//				line.setPoints(lows[0],lows[1]);
//				line.calculate();
//				line.extend( 20);
//				Chart.DrawLine(Color.Red,lows[0].X,lows[0].Y,line.Bar1,line.Y1,LineStyle.Solid);
//			}
		}
		
		protected bool NewChannel(int lastLineBar, Trend trend) {
			Channel channel= new Channel(Bars);
			channel.LongColor = LongColor;
			channel.ShortColor = ShortColor;
			int bar=Bars.CurrentBar;
			int lookBack = 1;
			channel.InterceptBar = lastLineBar;
			
			for( ; bar > Bars.CurrentBar-Bars.Capacity && bar >= lastLineBar; bar--) {
				switch( trend) {
					case Trend.Flat:
						channel.addPoint( bar, Formula.Middle(Bars,Bars.CurrentBar-bar));
						break;
					case Trend.Up:
						channel.addPoint( bar, Bars.Low[Bars.CurrentBar-bar]);
						channel.addPoint2( bar, Bars.High[Bars.CurrentBar-bar]);
						break;
					case Trend.Down:
						channel.addPoint( bar, Bars.High[Bars.CurrentBar-bar]);
						channel.addPoint2( bar, Bars.Low[Bars.CurrentBar-bar]);
						break;
				}
			}
			if( channel.CountPoints > lookBack) {
				channel.Calculate();
				channel.UpdateEnds();
				// If new channel isn't in the direction expected
				// then forget it.
				if( trend == Trend.Up && !channel.IsUp ) {
					return false;
				} else if( trend == Trend.Down && !channel.IsDown ) {
					return false;
				}
				channel.calcMaxDev(Bars);
				if( drawLines ) {
					channel.DrawChannel(Chart,extend,drawDashedLines);
				}
				lines.Add(channel);
				hasNewChannel = true;
				return true;
			}
			return false;
		}
		
		protected bool TryExtendChannel() {
			bool recalculate = true;
			if( lines.Count == 0) return false;
			Channel lr = lines[0];
			if( lr.CountPoints > 100) { return false; }
			int bar=Bars.CurrentBar;

			ChartPoint newPoint;
			switch( lr.Direction) {
				default:
				case Trend.Flat:
					newPoint = new ChartPoint( bar, Formula.Middle(Bars,Bars.CurrentBar-bar));
					break;
				case Trend.Up:
					newPoint = new ChartPoint( bar, Bars.Low[Bars.CurrentBar-bar]);
					break;
				case Trend.Down:
					newPoint = new ChartPoint( bar, Bars.High[Bars.CurrentBar-bar]);
					break;
			}
			double newSlope = lr.CheckNewSlope(newPoint);
			switch( lr.Direction) {
				default:
				case Trend.Flat:
					if( newSlope != 0) recalculate = false;
					break;
				case Trend.Up:
//					if( newSlope < lr.Slope) recalculate = false;
					if( newSlope <= 0) recalculate = false;
					break;
				case Trend.Down:
//					if( newSlope > lr.Slope) recalculate = false;
					if( newSlope >= 0) recalculate = false;
					break;
			}
			lr.addPoint(newPoint.Bar,(int)newPoint.Y);
			try {
				if( recalculate) {
					lr.Calculate();
				}
				lr.UpdateEnds();  // Was before calcMaxDev
				lr.calcHighest(Bars);
				if( recalculate) {
					lr.calcMaxDev(Bars);
				}
				if( drawLines) {
					if( reDrawLines) {
						lr.RedrawChannel(Chart,extend,drawDashedLines);
					} else {
						lr.DrawChannel(Chart,extend,drawDashedLines);
					}
				}
			} catch( ApplicationException ex) {
				Log.Notice( ex.ToString() );
				return false;
			}
			return true;
		}
		
		protected bool TryShortenChannel(int lastLineBar) {
			if( lines.Count == 0) return false;
			Channel lr = lines[0];
			lr.removePointsBefore(lastLineBar);
			
			try {
				lr.Calculate();
				lr.UpdateEnds();
				lr.calcMaxDev(Bars);
				if( drawLines) {
					lr.DrawChannel(Chart,extend,drawDashedLines);
				}
			} catch( ApplicationException ex) {
				Log.Notice( ex.ToString() );
				return false;
			}
			return true;
		}
		
		public bool RedrawLines {
			get { return reDrawLines; }
			set { reDrawLines = value; }
		}
		
		public bool DrawLines {
			get { return drawLines; }
			set { drawLines = value; }
		}
		
		public Series<Channel> Lines {
			get { return lines; }
		}
		
		public int Extend {
			get { return extend; }
			set { extend = value; }
		}
		
		public Color LongColor {
			get { return longColor; }
			set { longColor = value; }
		}
		
		public Color ShortColor {
			get { return shortColor; }
			set { shortColor = value; }
		}
		
		public bool HasNewChannel {
			get { return hasNewChannel; }
		}
		
		public bool IsActivated {
			get { return isActivated; }
			set { isActivated = value; }
		}
	}
}
