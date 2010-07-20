#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * 
 * 
 *
 * User: Wayne Walter
 * Date: 8/10/2009
 * Time: 5:07 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */

#endregion



namespace TickZoom.Examples.Indicators

import System
import TickZoom.Api
import TickZoom.Common

class MQ_ValueChart(IndicatorCommon):

	
	private vcOpen as IndicatorCommon

	private vcHigh as IndicatorCommon

	private vcLow as IndicatorCommon

	private vcClose as IndicatorCommon

	
	def constructor():
		pass

	
	override def OnInitialize():
		
		vcOpen = IndicatorCommon()
		vcOpen.Drawing.GroupName = 'ValueChart'
		vcOpen.Drawing.PaneType = PaneType.Secondary
		vcOpen.Drawing.IsVisible = true
		vcOpen.Drawing.GraphType = GraphType.Histogram
		AddIndicator(vcOpen)
		
		vcHigh = IndicatorCommon()
		vcHigh.Drawing.GroupName = 'ValueChart'
		vcHigh.Drawing.PaneType = PaneType.Secondary
		vcHigh.Drawing.IsVisible = true
		vcHigh.Drawing.GraphType = GraphType.Histogram
		AddIndicator(vcHigh)
		
		vcLow = IndicatorCommon()
		vcLow.Drawing.GroupName = 'ValueChart'
		vcLow.Drawing.PaneType = PaneType.Secondary
		vcLow.Drawing.IsVisible = true
		vcLow.Drawing.GraphType = GraphType.Histogram
		AddIndicator(vcLow)
		
		vcClose = IndicatorCommon()
		vcClose.Drawing.GroupName = 'ValueChart'
		vcClose.Drawing.PaneType = PaneType.Secondary
		vcClose.Drawing.IsVisible = true
		vcClose.Drawing.GraphType = GraphType.Histogram
		AddIndicator(vcClose)

	
	override def OnIntervalClose() as bool:
		vcOpen[0] = Bars.Open[0]
		vcHigh[0] = Bars.High[0]
		vcLow[0] = Bars.Low[0]
		vcClose[0] = Bars.Close[0]
		
		return true
	

