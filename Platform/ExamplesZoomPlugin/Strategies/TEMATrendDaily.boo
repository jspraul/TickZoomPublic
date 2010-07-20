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
import TickZoom.Common

class TEMATrendDaily(StrategyCommon):

	private fast as int

	private slow as int

	private entrySma as SMA

	private tema as TEMA

	
	def constructor():
		self(10, 20)

	
	def constructor(fast as int, slow as int):
		self.fast = fast
		self.slow = slow

	
	override def OnInitialize():
		tema = TEMA(Bars.Close, fast)
		AddIndicator(tema)
		entrySma = SMA(Bars.Close, slow)
		AddIndicator(entrySma)

	
	override def OnIntervalClose(timeFrame as Interval) as bool:
		if timeFrame.Equals(IntervalDefault):
			if (entrySma.Count > 0) and (tema.Count > 0):
				if tema[0] > entrySma[0]:
					Exit.GoFlat()
				else:
					Exit.GoFlat()
		return true

	
	override def ToString() as string:
		return ((tema.Period + ',') + entrySma.Period)

