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
using System.ComponentModel;

namespace SimpleExpressionEvaluator.Utilities
{
    /// <summary>
    /// Summary description for TypeNormalizer.
    /// </summary>
    public class TypeNormalizer
    {
        public static void NormalizeTypes(ref object left,ref object right)
        {
            NormalizeTypes(ref left,ref right,0);
        }

        public static void NormalizeTypes(ref object left,ref object right,object nullValue)
        {
            if (left == null)
                left = 0;
            if (right == null)
                right = 0;

            if (left.GetType() == right.GetType())
                return;

            try
            {
                right = Convert.ChangeType(right,left.GetType());				
            }
            catch
            {
                try
                {
                    left = Convert.ChangeType(left, right.GetType());
                }
                catch
                {
                    throw new Exception(String.Format("Error converting from {0} type to {1}", left.GetType().FullName, right.GetType().FullName));
                }
            }
        }
		
        public static void EnsureTypes(ref object[] values,Type targetType)
        {
            object nullValue = null;
            if (targetType.IsValueType)
                nullValue = Activator.CreateInstance(targetType);
            EnsureTypes(ref values,targetType,nullValue);
				
        }

        public static void EnsureTypes(ref object[] values,Type targetType,object nullValue)
        {
            for (int i=0;i<values.Length;i++)
            {
                values[i] = EnsureType(values[i],targetType,nullValue);				
            }
        }

        public static T EnsureType<T>(object value)
        {
            return EnsureType<T>(value, default(T));
        }

        public static T EnsureType<T>(object value,object nullValue)
        {
            return (T) EnsureType(value, typeof (T), nullValue);
        }

        public static object EnsureType(object value, Type targetType)
        {
            if (value != null && value.GetType() == targetType)
                return value;

            object defaultValue = null;
            if (targetType.IsValueType)
                defaultValue = Activator.CreateInstance(targetType);
            return EnsureType(value, targetType, defaultValue);            
        }

        public static object EnsureType(object value,Type targetType,object nullValue)
        {
            if (value == null)
                return nullValue;
            
            if (targetType == typeof(object))
                return value;

            if (value.GetType() == targetType)
                return value;

            /*
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(value.GetType()))
            {
            	return converter.ConvertFrom(value);
            }
            */
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            { }
            return nullValue;
        }
    }
}