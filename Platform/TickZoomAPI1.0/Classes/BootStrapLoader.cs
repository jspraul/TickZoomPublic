#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General Public License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General Public License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * 
 * 
 *
 * User: Wayne Walter
 * Date: 8/1/2010
 * Time: 1:53 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Reflection;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of BootStrapLoader.
	/// </summary>
	public class BootStrap
	{
		private readonly string assemblyName = "TickZoomLoader.dll";
		private readonly string interfaceName = "FactoryLoader";
	    public FactoryLoader FactoryLoader()
	    {
	    	try { 
				Assembly assembly = Assembly.LoadFrom(assemblyName);
				foreach (Type type in assembly.GetTypes())
				{
					if (type.IsClass && !type.IsAbstract && !type.IsInterface) {
						if (type.GetInterface(interfaceName) != null) {
							object obj = Activator.CreateInstance(type);
							return (FactoryLoader) obj;
						}
					}
				}
	      		throw new ApplicationException("Sorry, cannot find " + interfaceName + " in " + assemblyName);
	    	} catch( Exception ex) {
	      		throw new ApplicationException("Sorry, cannot find " + interfaceName + " in " + assemblyName, ex);
	    	}
	    }
	}
}
