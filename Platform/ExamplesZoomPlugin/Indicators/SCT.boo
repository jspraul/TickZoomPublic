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
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General License for more details.
 * 
 * You should have received a copy of the GNU General License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */

#endregion


namespace TickZoom

import System
import TickZoom.Api
import System.Drawing
import TickZoom.Common

class SCT(IndicatorCommon):

	private volume as Volume
	private volumePeaks as Integers
	private smartMoney as IndicatorCommon
	private pace as IndicatorCommon
	def constructor():
		super()
		Drawing.Color = Color.Green
		volumePeaks = Integers()
	
	override def OnInitialize():
		smartMoney = IndicatorCommon()
		smartMoney.Drawing.GraphType = GraphType.Histogram
		smartMoney.Drawing.GroupName = 'Smart $'
		AddIndicator(smartMoney)
		
		volume = Volume()
		AddIndicator(volume)
		
		pace = IndicatorCommon()
		pace.Drawing.GraphType = GraphType.Histogram
		pace.Drawing.GroupName = 'Pace'
		AddIndicator(pace)
		//			
		//			volumeFRV = new Indicator();
		//			volumeFRV.PaneType = PaneType.Secondary;
		//			volumeFRV.Color = Color.Magenta;
		//			volumeFRV.GroupName = "Volume";
		//			AddIndicator( volumeFRV );
		//			
		//			volumeDryUp = new Indicator();
		//			volumeDryUp.PaneType = PaneType.Secondary;
		//			volumeDryUp.Color = Color.Magenta;
		//			volumeDryUp.GroupName = "Volume";
		//			AddIndicator( volumeDryUp );
		//			
		//			volumeMax = new Indicator();
		//			volumeMax.PaneType = PaneType.Secondary;
		//			volumeMax.Color = Color.Black;
		//			volumeMax.GroupName = "Volume";
		//			AddIndicator( volumeMax );

	
	//		int highestVolume = 0;
	override def OnIntervalClose() as bool:
		if IntervalDefault.BarUnit == BarUnit.Tick:
			span as Elapsed = (Ticks[0].Time - Bars.Time[0])
			smartMoney[0] = (Bars.Sentiment[0] / span.TotalSeconds)
			pace[0] = (((Formula.Middle(Bars, 0) - Formula.Middle(Bars, 1)) * 10000) / span.TotalMilliseconds)
		else:
			smartMoney[0] = Bars.Sentiment[0]
			pace[0] = (Bars.Close[0] - Bars.Close[1])
		return true

	
	ProRatedVolume as int:
		get:
			timeInBar as Elapsed = (Ticks[0].Time - Bars.Time[0])
			return cast(int, ((Bars.Volume[0] * 60) / timeInBar.TotalSeconds))

	
	TickPace as int:
		get:
			if Ticks.Count < 100:
				return 0
			span as Elapsed = (Ticks[0].Time - Ticks[99].Time)
			volume as int = 0
			i = 0
			goto converterGeneratedName1
			while true:
				i += 1
				:converterGeneratedName1
				break  unless ((i < Ticks.Count) and (i < 100))
				volume += Ticks[i].Volume
			return cast(int, (volume / span.TotalSeconds))

	def ResetVolume():
		self[0] = 0
	
	SmartMoney as IndicatorCommon:
		get:
			return smartMoney

	
	Volume as IndicatorCommon:
		get:
			return volume

	
	Pace as IndicatorCommon:
		get:
			return pace

