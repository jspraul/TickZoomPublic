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
import System.Drawing
import TickZoom.Api
import TickZoom.Common

class TEMACounterDaily(StrategyCommon):

	private slow = 28

	private fast = 14

	private slowTema as TEMA

	private fastTema as TEMA

	
	def constructor():
		pass
		// Set defaults here.

	
	override def OnInitialize():
		slowTema = TEMA(Bars.Close, slow)
		slowTema.Drawing.Color = Color.Orange
		fastTema = TEMA(Bars.Close, fast)
		AddIndicator(slowTema)
		AddIndicator(fastTema)

	
	override def OnProcessTick(tick as Tick) as bool:
		if (fastTema.Count > 0) and (slowTema.Count > 0):
			Position.Signal = ((-1) if (fastTema[0] > slowTema[0]) else 1)
		else:
			Exit.GoFlat()
		return true

	
	override def ToString() as string:
		return ((fast + ',') + slow)

	
	Slow as int:
		get:
			return slow
		set:
			slow = value

	
	Fast as int:
		get:
			return fast
		set:
			fast = value
	

