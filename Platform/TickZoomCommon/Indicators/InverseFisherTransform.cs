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


namespace TickZoom.Common
{
	public class InverseFisherTransform : IndicatorCommon
	{
		IndicatorCommon value1;
		IndicatorCommon value2;
		RSI rsi;
		WMA wma;
		
		public override void OnInitialize()
		{
			Name = "IFT";
			// Display below the price chart
			Drawing.PaneType = PaneType.Secondary;
			// Display below the price chart
			Drawing.IsVisible = true;
			
			// Creates the 
			value1 = Formula.Indicator();
			value2 = Formula.Indicator();
			rsi = Formula.RSI(Bars.Close,5);
			wma = Formula.WMA(value1, 5);
			Formula.Line(0.5,Color.LightGreen);
			Formula.Line(-0.5,Color.Red);
		}
		
		public override bool OnIntervalClose()
		{
			value1[0] = 0.1*(rsi[0]-50);
			value2[0] = 2*wma[0];
			this[0]=(Math.Exp(value2[0])-1)/(Math.Exp(value2[0])+1);
			
			Drawing.Color = this[1]<this[0] ? Color.Green : this[1] > this[0] ? Color.Red : Color.Black;
			
			if( IsTrace) Log.Trace("rsi="+rsi[0]+",wma="+wma[0]+",Value1="+value1[0]+",Value2="+value2[0]+",ifsh="+this[0]);
			
			return true;
		}
	}
}
