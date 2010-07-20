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
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom
{

	/// <summary>
	/// Description of SMA.
	/// </summary>
	public class NarrowChannels : IndicatorCommon
	{
		Strategy strategy;
		Series<NarrowChannel> tapes;
		Series<WideChannel> channels;
		
		bool reDrawLines = true;
		bool drawDashedLines = false;
		bool drawSolidLines = true;
		bool drawLines = true;
		Series<ChartPoint> highs;
		Series<ChartPoint> lows;
		int extend = 10;
		Color longColor = Color.Green;
		Color shortColor = Color.Red;
//		bool hasNewChannel;
		bool isActivated = true;

//		Trend trend = Trend.Flat;
		public NarrowChannels() {
			tapes = Series<NarrowChannel>();
			channels = Series<WideChannel>();
			highs = Series<ChartPoint>();
			lows = Series<ChartPoint>();
		}
			
		public NarrowChannels(Strategy strategy)
		{
			this.strategy = strategy;
		}

		public override void OnInitialize()
		{
			Extend = 2;
			LongColor = Color.LightGreen;
			ShortColor = Color.Pink;
		}
		
		public void Reset() {
			tapes.Clear();
		}
		
		public override bool OnIntervalClose() {
			
			if( tapes.Count == 0) {
				TryNewTape(Bars.CurrentBar, Trend.Flat);
				return true;
			} 

			ContinueOrEnd();
			
			if( tapes.Count > 1 ) {
				NarrowChannel line = tapes[1];
				int lastBO = line.BreakoutBar;
				if( line.IsUp && line.Direction != tapes[0].Direction) {
					int lastHighBar = line.LastHighBar;
					double lastHigh = Bars.High[Bars.CurrentBar-lastHighBar];
					if( lows.Count == 0 || lastHighBar - lows[0].X > 1 ) {
						if( highs.Count == 0 || lastHighBar != highs[0].X) {
							NewHigh(lastHighBar,lastHigh);
						}
					}
					
				} else if( line.IsDown && line.Direction != tapes[0].Direction) {
					int lastLowBar = line.LastLowBar;
					double lastLow = Bars.Low[Bars.CurrentBar-lastLowBar];
					
					if( highs.Count == 0 || lastLowBar - highs[0].X > 1 ) {
						if( lows.Count == 0 || lastLowBar != lows[0].X) {
							NewLow(lastLowBar,lastLow);
						}
					}
				}
			}
			return true;
		}

		public ChartPoint FindPoint( Series<ChartPoint> points, double bar) {
			for( int i=0; i<points.Count; i++) {
				if( points[i].X < bar) {
					return points[i];
				}
			}
			throw new ApplicationException("No Point found.");
		}
		
		public void NewHigh( int bar, double price) {
			// If no low since last high and this high is higher,
			// the replace the last high point, else add it.
			if( highs.Count > 0 && lows.Count > 0 &&
			   lows[0].X < highs[0].X) {
				if( price > highs[0].Y) {
					// Make the replaced one Red.
					Chart.DrawBox(Color.Red,(int)highs[0].X,highs[0].Y);
					highs[0] = new ChartPoint(bar,price);
				} else {
					// Skip a lower high without a low in between.
					return;
				}
			} else {
				highs.Add(new ChartPoint(bar,price));
			}
			Chart.DrawBox(Color.Blue,bar,price);
			
			if( highs.Count>1 && lows.Count>1) {
				try {
					ChartPoint point1 = highs[0];
					ChartPoint point2 = FindPoint(lows,point1.X);
					ChartPoint point3 = FindPoint(highs,point2.X);
					if( point1.Y < point3.Y ) {
						WideChannel wchannel = new WideChannel(Trend.Down,Bars);
						wchannel.Chart = Chart;
						wchannel.addHigh(point1.X);
						wchannel.addLow(point2.X);
						wchannel.addHigh(point3.X);
						wchannel.TryDraw();
						channels.Add(wchannel);
	//					Chart.DrawLine(Color.Red,highs[1].X,highs[1].Y,highs[0].X,highs[0].Y,LineStyle.Solid);
					}
				} catch( ApplicationException) {
					// Return if FindPoint failed.
					return;
				}
			}
		}
		
		public void NewLow( int bar, double price) {
			// If no low since last high and this high is higher,
			// the replace the last high point, else add it.
			if( highs.Count > 0 && lows.Count > 0 &&
			   highs[0].X < lows[0].X) {
				if( price < lows[0].Y) {
					Chart.DrawBox(Color.Red,(int)lows[0].X,lows[0].Y);
					lows[0] = new ChartPoint(bar,price);
				} else {
					// Skip a high low with out a high in between.
					return;
				}
			} else {
				lows.Add(new ChartPoint(bar,price));
			}
			
			Chart.DrawBox(Color.Blue,bar,price);
			if( lows.Count>1) {
				try {
					ChartPoint point1 = lows[0];
					ChartPoint point2 = FindPoint(highs,point1.X);
					ChartPoint point3 = FindPoint(lows,point2.X);
					if( point1.Y > point3.Y) {
						WideChannel wchannel = new WideChannel(Trend.Up,Bars);
						wchannel.Chart = Chart;
						wchannel.addLow(point1.X);
						wchannel.addHigh(point2.X);
						wchannel.addLow(point3.X);
						wchannel.TryDraw();
						channels.Add(wchannel);
	//					Chart.DrawLine(Color.Red,lows[1].X,lows[1].Y,lows[0].X,lows[0].Y,LineStyle.Solid);
					}
				} catch( ApplicationException) {
					// Return if FindPoint failed.
					return;
				}
			}
		}
		
		public virtual void ContinueOrEnd() {
			NarrowChannel channel = Tapes[0];
			int top = channel.High;
			int bottom = channel.Low;
			double close = Bars.Close[0];
			bool isSuccess = false;
			if( Tapes[0].IsDown) {
				if( close > top ) {
					isSuccess = TryNewTape(Tapes[0].LastLowBar,Trend.Up);
				} else if( close < bottom ) {
					isSuccess = TryNewTape(Tapes[0].LastHighBar,Trend.Down);
				}
			} else if (Tapes[0].IsUp) {
				if( close < bottom ) {
					isSuccess = TryNewTape(Tapes[0].LastHighBar,Trend.Down);
				} else if( close > top ) {
					isSuccess = TryNewTape(Tapes[0].LastLowBar,Trend.Up);
				}
			} else {
				if( close < bottom ) {
					isSuccess = TryNewTape(Tapes[0].LastHighBar,Trend.Down);
				} else if( close > top ) {
					isSuccess = TryNewTape(Tapes[0].LastLowBar,Trend.Up);
				}
			}
			if( ! isSuccess) {
				channel.TryExtend();
				return;
			}
		}
		
		protected bool TryNewTape(int lastLineBar, Trend trend) {
			NarrowChannel channel= new NarrowChannel(Bars);
			channel.BreakoutBar = Bars.CurrentBar;
			channel.LongColor = LongColor;
			channel.ShortColor = ShortColor;
			channel.Chart = Chart;
			channel.DrawLines = drawLines;
			channel.ReDrawLines = RedrawLines;
			channel.DrawSolidLines = drawSolidLines;
			channel.DrawDashedLines = drawDashedLines;
			channel.Extend = extend;
			if( channel.TryInitialize(lastLineBar,trend)) {
				tapes.Add(channel);
//				hasNewChannel = true;
//				if( tapes.Count < 2 || channel.Direction != tapes[1].Direction) {
//					strategy.FireEvent(new NewChannel().SetDirection(channel.Direction).ExpirySeconds(30));
//				}
				return true;
			}
			return false;
		}
		
		
		public bool RedrawLines {
			get { return reDrawLines; }
			set { reDrawLines = value; }
		}
		
		public bool DrawLines {
			get { return drawLines; }
			set { drawLines = value; }
		}
		
		public Series<NarrowChannel> Tapes {
			get { return tapes; }
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
		
//		public bool HasNewChannel {
//			get { return hasNewChannel; }
//		}
//		
		public bool IsActivated {
			get { return isActivated; }
			set { isActivated = value; }
		}
		
		public Series<WideChannel> Channels {
			get { return channels; }
		}
	}
}
