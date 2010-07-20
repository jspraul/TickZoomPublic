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

namespace TickZoom.Properties
{
	/// <summary>
	/// Description of PropertiesBase.
	/// </summary>
	[Serializable]
	public class PropertiesBase
	{
		public PropertiesBase()
		{
		}
		
		public void CopyProperties( object otherObj) {
			Type type = this.GetType();
			PropertyInfo[] properties = type.GetProperties();
			for( int i=0; i<properties.Length; i++) {
				PropertyInfo property = properties[i];
				PropertyInfo otherProperty = otherObj.GetType().GetProperty(property.Name);
				if( otherProperty == null) {
					throw new ApplicationException( "Sorry, " + otherObj.ToString() + " doesn't have the property: "+property.Name);
				}
				if( !otherProperty.CanWrite) {
					throw new ApplicationException( "Sorry, " + otherObj.ToString() + " doesn't have a setter for property: "+property.Name);
				}
				object[] objs = property.GetCustomAttributes(typeof(ObsoleteAttribute),true);
				bool obsoleteProperty = objs.Length > 0;
				objs = otherProperty.GetCustomAttributes(typeof(ObsoleteAttribute),true);
				bool obsoleteOther = objs.Length > 0;
				if( obsoleteOther != obsoleteProperty) {
					if( obsoleteProperty) {
						throw new ApplicationException( "Sorry, the property " + property.Name + " is obsolete for " + this.GetType().Name + " but not for " + otherObj.GetType().Name);
					} else {
						throw new ApplicationException( "Sorry, the property " + otherProperty.Name + " is obsolete for " + otherObj.GetType().Name + " but not for " + this.GetType().Name);
					}
				}
				if( obsoleteProperty || obsoleteOther) {
					// Skip this one.
					continue;
				}
				otherProperty.SetValue(otherObj,property.GetValue(this,null), null);
			}
		}
	}
}
