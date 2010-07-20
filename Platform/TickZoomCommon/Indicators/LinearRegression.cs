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
using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of LinearRegression.
	/// </summary>
	public class LinearRegression
	{
		public LinearRegression() {
			coord = Factory.Engine.Series<ChartPoint>();
		}
		
		public LinearRegression(Series<ChartPoint> series) {
			coord = Factory.Engine.Series<ChartPoint>();
			for( int i=series.Count-1;i>=0;i--) {
				coord.Add(series[i]);
			}
		}
		
		public void reset() {
			X = 0;
			Y = 0;
			SumXY=0;
			SumY=0;
			SumX=0;
			SumXSqr=0;
			SumYSqr=0;
			Divisor=0;
			CorrelDivisor=0;			
		}
		
		public void calculate() {
			reset();
			int N = coord.Count;

			calculateSums();

			if( N > 1 ) {
				double sqrSumX = Math.Pow(SumX, 2);
				Divisor = N * SumXSqr - sqrSumX;
				CorrelDivisor = Math.Sqrt( Divisor * ( N * SumYSqr - Math.Pow(SumY,2) ));
				
				if( Divisor == 0 ) {
					throw new ApplicationException("LinearRegression failed.");
				} else {
					Slope = ( N * SumXY - SumX * SumY) / Divisor ;
//					Angle = Math.Atan( Slope ) ;
					Intercept = ( SumY - Slope * SumX ) / N ;
					
					if( CorrelDivisor == 0) {
						Correlation = 1;
					} else {
						Correlation = Math.Abs((N * SumXY - SumX * SumY) / CorrelDivisor);
					}
					
					RegValue = Intercept + Slope * (double) ( coord[N-1].X );
				}
			} else {
				throw new ApplicationException("LinearRegression failed. Only one point.");
			}
		}
			
		private void calculateSums() { 
			int J;
			int N = coord.Count;
			// Okay, sum up the matrix.
			for( J = 0; J < N; J++ ) {
				X = (double) coord[J].X;
				Y = (double) coord[J].Y;
				SumXY = SumXY + X * Y;
				SumX = SumX + X;
				SumXSqr = SumXSqr + Math.Pow(X,2);
				SumY = SumY + Y;
				SumYSqr = SumYSqr + Math.Pow(Y,2);
			}
		}
		
		public virtual void addPoint( double x, double y) {
			coord.Add(new ChartPoint(x,y));
		}
		
		public virtual void addPoint( ChartPoint point) {
			addPoint( point.X, point.Y);
		}
		
		public virtual void removePointsBefore( double x) {
			Series<ChartPoint> newCoord = Factory.Engine.Series<ChartPoint>();
			for( int i=coord.Count-1;i>=0;i--) {
				if( coord[i].X >= x || i < 3) {
					newCoord.Add( coord[i]);
				}
			}
			coord = newCoord;
		}
		
//		public void removePoint( int i) {
//			coord.RemoveAt(i);
//		}
		
		public int countPoints() {
			return coord.Count;
		}
		
		public void clearPoints() {
			coord.Clear();
		}
		
		public ChartPoint startPoint() {
			return coord[0];
		}

		public ChartPoint endPoint() {
			return coord[coord.Count-1];
		}
		
		public double GetY(double x) {
			return intercept + x*slope;
		}		
		
		private double X = 0;
		private double Y = 0;
		private double SumXY=0, SumY=0, SumX=0, SumXSqr=0, SumYSqr=0;
		private double Divisor=0;
		private double CorrelDivisor=0;
		public Series<ChartPoint> Coord {
			get { return coord; }
		}
		
		private Series<ChartPoint> coord;
		
		private double angle;
		public double Angle {
			get { return angle; }
			set { angle = value; }
		}
		
		private double slope;
		public double Slope {
			get { return slope; }
			set { slope = value; }
		}
		private double intercept;
		public double Intercept {
			get { return intercept; }
			set { intercept = value; }
		}
		public double regValue;
		public double RegValue {
			get { return regValue; }
			set { regValue = value; }
		}
		public double correlation;
		public double Correlation {
			get { return correlation; }
			set { correlation = value; }
		}
		public double getValue( int bar) {
			return intercept + slope * bar;
		}
	}
}
