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
using System.Reflection;

using NUnit.Core;
using NUnit.Core.Extensibility;

namespace TickZoom.DynamicTest.Add
{
	[SuiteBuilder, NUnitAddin]
	public class SuiteBuilderLoader : ISuiteBuilder, IAddin
	{
		Assembly assembly;
		ISuiteBuilder builder;
		public Test BuildFrom(Type type)
		{
			TryLoadBuilder(type.Assembly);
			if( builder == null) {
				return null;
			}
			return builder.BuildFrom(type);
		}
		
		private void TryLoadBuilder(Assembly assembly) {
			if( this.assembly == assembly) return;
			this.assembly = assembly;
			builder = null;
			var count = 0;
			foreach( var type in assembly.GetTypes()) {
				if( Reflect.HasInterface(type,typeof(ISuiteBuilder).FullName)) {
			        count++;
					builder = (ISuiteBuilder) Reflect.Construct(type);
				}
			}
			if( count > 1) {
				throw new ApplicationException("Found more than one class with interface " + typeof(ISuiteBuilder).Name + " in the assembly: " + assembly.GetName().Name);
			}
		}
	
		public bool CanBuildFrom(Type type)
		{
			TryLoadBuilder(type.Assembly);
			if( builder == null) {
				return false;
			} else {
				return builder.CanBuildFrom(type);
			}
		}
		
		public bool Install(IExtensionHost host)
		{
		    IExtensionPoint builders = host.GetExtensionPoint("SuiteBuilders");
		    if (builders == null)
		        return false;
		
		    builders.Install(this);
		    return true;			
		}
	}
}