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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Converters.
	/// </summary>
	public class Converters
	{
		private static Dictionary<Type,TypeConverter> converters = new Dictionary<Type,TypeConverter>();
		static Converters()
		{
			converters.Add(typeof(Interval),new IntervalTypeConverter());
			converters.Add(typeof(TimeStamp),new TimestampTypeConverter());
			converters.Add(typeof(Elapsed),new ElapsedTypeConverter());
		}
		
		public static TypeConverter GetConverter(Type type) {
			TypeConverter converter;
			if( !converters.TryGetValue(type, out converter)) {
				converter = TypeDescriptor.GetConverter(type);
			}
			return converter;
		}
		
		public static object Convert( Type type, string str) {
			Assembly assembly = Assembly.GetAssembly(typeof(Converters));
			CultureInfo cultureInfo = assembly.GetName().CultureInfo;
			TypeConverter converter = Converters.GetConverter(type);
			object obj = converter.ConvertFrom(new VoidContext(),cultureInfo,str);
			return obj;
		}		
		
		
#region VoidContext
		public class VoidContext : ITypeDescriptorContext {
			public IContainer Container {
				get {
					throw new NotImplementedException();
				}
			}
			
			public object Instance {
				get {
					throw new NotImplementedException();
				}
			}
			
			public PropertyDescriptor PropertyDescriptor {
				get {
					throw new NotImplementedException();
				}
			}
			
			public bool OnComponentChanging()
			{
				throw new NotImplementedException();
			}
			
			public void OnComponentChanged()
			{
				throw new NotImplementedException();
			}
			
			public object GetService(Type serviceType)
			{
				throw new NotImplementedException();
			}
		}
#endregion

	}
}
