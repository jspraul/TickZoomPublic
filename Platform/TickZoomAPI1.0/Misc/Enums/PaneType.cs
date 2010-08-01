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

namespace TickZoom.Api
{
	public enum PaneType {
		/// <summary>
		/// Display this in the primary price graph in the same scale as the price bars.
		/// </summary>
		Primary,
		/// <summary>
		/// Display in a sub chart below the price graph. You can control which indicator
		/// appear in the same graph using the GroupName property.
		/// </summary>
		Secondary,
		/// <summary>
		/// Deprecated.
		/// </summary>
		Signal,
		/// <summary>
		/// Overlay in the primary price graph with a separate scale from the price bars.
		/// </summary>
		OverlayPrimary
	}
}
