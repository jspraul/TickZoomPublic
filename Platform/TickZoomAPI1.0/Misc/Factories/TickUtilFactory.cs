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
using System.Reflection;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Factory.
	/// </summary>
	[CLSCompliant(false)]
	public interface TickUtilFactory
	{
		TickQueue TickQueue( Type type);
		TickQueue TickQueue( string name);
		TickIO TickIO();
		FastFillQueue FastFillQueue(string name, int maxSize);
		FastEventQueue FastEventQueue(string name, int maxSize);
		TickWriter TickWriter(bool overwriteFile);
		TickReader TickReader();
		FastQueue<T> FastQueue<T>(string name);
		Pool<T> Pool<T>() where T : new();
	}
}
