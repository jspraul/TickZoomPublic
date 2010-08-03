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
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TickZoom.Api
{
	public static class ExtensionMethods {
		public const double ConvertDouble = 1000000000D;
		public const int PricePrecision = 9;

		public static long ToLong( this double value) {
			return Convert.ToInt64(Math.Round(value,PricePrecision)*ConvertDouble);
		}
		
		public static double ToDouble( this long value) {
			return Math.Round(value / ConvertDouble, PricePrecision );
		}
		
		public static double Round( this double value) {
			return Math.Round(value, PricePrecision);
		}
		
		private static readonly object encodeLocker = new object();
		public static long ToULong(this string symbol) {
			lock( encodeLocker) {
				long result = Factory.Symbol.LookupSymbol(symbol).BinaryIdentifier;
				return result;
			}
		}
		
		#region base64
		public static readonly int Combinations = 26 + 26 + 10 + 2;
		
		public static long SymbolToULong(string symbol) {
			symbol = symbol.Substring(0,Math.Min(symbol.Length,9));
			char[] chars = symbol.ToCharArray();
			long result = 0;
			for( int i=chars.Length-1; i>=0;i--) {
				char chr = chars[i];
				long digit = (long) CharToInt(chr);
				long pow = (long) Math.Pow(Combinations,i);
				result += digit * pow ;
			}
			return result;
		}
			
		public static string ULongToSymbol(long symbol) {
			char[] chars = new char[10];
			int index=0;
			for( int i=0; i<chars.Length;i++) {
				long pow = (long) Math.Pow(Combinations,i);
				if( pow == 0) {
					System.Diagnostics.Debugger.Break();
				}
				long remain = (long) (symbol / pow);
				long ulDigit = remain % (long) Combinations;
				int digit = (int) ulDigit;
				if( digit > 0) {
					chars[index] = IntToChar(digit);
					index ++;
				}
			}
			return new string(chars,0,index);
		}
		
		public static int CharToInt(char chr) {
			int result = 0;
			if( chr >= 'A' && chr <= 'Z') {
				result = chr - 'A' + 1;
			} else if( chr >= 'a' && chr <= 'z') {
				result = chr - 'a' + 27;
			} else if( chr >= '0' && chr <= '9') {
				result = chr - '0' + 53;
			} else if( chr == '/') {
				result = 63;
			} else {
				throw new FormatException("Invalid symbol character: '"+chr+"'");
			}
			return result;
		}
		
		public static char IntToChar(int digit) {
			char result;
			if( digit < 27) {
				result = (char) ('A' + digit - 1);
			} else if( digit < 53) {
				result = (char) ('a' + digit - 27);
			} else if( digit < 63) {
				result = (char) ('0' + digit - 53);
			} else if( digit == 63) {
				result = '/';
			} else {
				throw new FormatException("Invalid symbol digit: '"+digit+"'");
			}
			return result;
		}
		#endregion
		
		private static readonly object decodeLocker = new object();
		private static readonly ASCIIEncoding decoder = new ASCIIEncoding();
		public static string ToSymbol(this long symbol) {
			lock( decodeLocker) {
				string result = Factory.Symbol.LookupSymbol(symbol).Symbol;
				return result;
			}
		}
		
		public static string StripInvalidPathChars(this string symbolStr) {
       		List<char> invalidChars = new List<char>(Path.GetInvalidPathChars());
       		invalidChars.Add('\\');
       		invalidChars.Add('/');
       		foreach( char invalid in invalidChars) {
       			symbolStr = symbolStr.Replace(new string(invalid,1),"");
       		}
       		return symbolStr;
		}
		
		public static void OnProperties(this object obj, ModelProperties properties)
		{
			string[] propertyKeys = properties.GetPropertyKeys();
			for( int i=0; i<propertyKeys.Length; i++) {
				obj.HandleProperty(propertyKeys[i],properties.GetProperty(propertyKeys[i]).Value);
			}
		}
		
		private static void HandleProperty(this object obj, string name, string str) {
			PropertyInfo property = obj.GetType().GetProperty(name);
			Type propertyType = property.PropertyType;
			object value = Converters.Convert(propertyType,str);
			property.SetValue(obj,value,null);
	//			log.WriteFile("Property " + property.Name + " = " + value);
		}		
		
		public static bool CompareWildcard(this string WildString, string Mask, bool IgnoreCase)
		{
		    int i = 0;
		 
		    if (String.IsNullOrEmpty(Mask))
		        return false;
		    if (Mask == "*")
		        return true;
		 
		    while (i != Mask.Length)
		        {
		        if (CompareWildcardInternal(WildString, Mask.Substring(i), IgnoreCase))
		            return true;
		 
		        while (i != Mask.Length && Mask[i] != ';')
		            i += 1;
		 
		        if (i != Mask.Length && Mask[i] == ';')
		            {
		            i += 1;
		 
		            while (i != Mask.Length && Mask[i] == ' ')
		                i += 1;
		            }
		        }
		 
		    return false;
		}
		 
		internal static bool CompareWildcardInternal(string compare, string mask, bool ignoreCase)
		{
		    int maskIndex = 0, compareIndex = 0;
		 
		    while (compareIndex != compare.Length && maskIndex != mask.Length) {
		        switch (mask[maskIndex]) {
		            case '*':
		 
		                if ((maskIndex + 1) == mask.Length)
		                    return true;
		 
		                while (compareIndex != compare.Length)
		                    {
		                    if (CompareWildcardInternal(compare.Substring(compareIndex + 1), mask.Substring(maskIndex + 1), ignoreCase))
		                        return true;
		 
		                    compareIndex += 1;
		                    }
		 
		                return false;
		 
		            case '?':
		 
		                break;
		 
		            default:
		 
		                if (ignoreCase == false && compare[compareIndex] != mask[maskIndex])
		                    return false;
		                if (ignoreCase && Char.ToLower(compare[compareIndex]) != Char.ToLower(mask[maskIndex]))
		                    return false;
		 
		                break;
		            }
		 
		        maskIndex += 1;
		        compareIndex += 1;
		        
		    }
		 
		    if (compareIndex == compare.Length) {
		    	if (maskIndex == mask.Length || mask[maskIndex] == ';' || mask[maskIndex] == '*') {
		    		return true;
		    	}
	        }
		    
		 
		    return false;
		}		
		
		public static void AddLast<T>(this LinkedList<T> list1, Iterable<T> list2) {
			foreach( var item in list2.Iterate()) {
				list1.AddLast(item);
			}
		}
	}
}
