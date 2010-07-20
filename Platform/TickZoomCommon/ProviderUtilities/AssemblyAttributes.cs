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
using System.ServiceProcess;
using TickZoom.Api;

namespace TickZoom.Common
{
	public static class AssemblyAttributes {
		
		public static string GetTitle() {
			System.Reflection.Assembly thisAssembly = Assembly.GetEntryAssembly();
			object[] attributes = thisAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
			if (attributes.Length == 1)
			{
			   return (((AssemblyTitleAttribute) attributes[0]).Title);
			} else {
				throw new ApplicationException("Found more than one Assembly Title attribute. Unable to choose service name.");
			}
		}
	}
}
