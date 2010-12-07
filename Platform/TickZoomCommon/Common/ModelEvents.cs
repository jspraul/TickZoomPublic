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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

using TickZoom.Api;

namespace TickZoom.Common
{
	public class ModelEvents {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(ModelEvents));
		private readonly bool debug = log.IsDebugEnabled;
		private readonly bool trace = log.IsTraceEnabled;
		protected ModelProperties properties;
		public void OnProperties(ModelProperties properties)
		{
			this.properties = properties;
	   			if( trace) log.Trace(GetType().Name+".OnProperties() - NotImplemented");
			string[] propertyKeys = properties.GetPropertyKeys();
			for( int i=0; i<propertyKeys.Length; i++) {
				HandleProperty(propertyKeys[i],properties.GetProperty(propertyKeys[i]).Value);
			}
		}
		
		private void HandleProperty( string name, string str) {
			PropertyInfo property = this.GetType().GetProperty(name);
			Type propertyType = property.PropertyType;
			object value = Converters.Convert(propertyType,str);
			property.SetValue(this,value,null);
	//			log.WriteFile("Property " + property.Name + " = " + value);
		}		
		
		public virtual void OnConfigure() {
			
		}
		
		public virtual void OnInitialize() {
		}
		
		public virtual bool OnBeforeIntervalOpen() {
	   			return false;
		}
	
		public virtual bool OnBeforeIntervalOpen(Interval interval) {
	   			return false;
		}
	
		public virtual bool OnIntervalOpen() {
			return false;
		}
	
		public virtual bool OnIntervalOpen(Interval interval) {
			return false;
		}
		
		
		public virtual bool OnProcessTick(Tick tick) {
			return true;
		}
		
		public virtual bool OnBeforeIntervalClose() {
			return false;
		}
		
		public virtual bool OnBeforeIntervalClose(Interval interval) {
			return false;
		}
		
		public virtual bool OnIntervalClose() {
			return false;
		}
		
		public virtual bool OnIntervalClose(Interval interval) {
			return false;
		}
	
		public virtual void OnEndHistorical() {
		}
	}
}
