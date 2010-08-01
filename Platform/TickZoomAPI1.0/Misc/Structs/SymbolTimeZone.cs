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
using System.Text;
using System.Threading;

namespace TickZoom.Api
{
	public class SymbolTimeZone {
		private TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
		private SymbolInfo symbol;
		public SymbolTimeZone( SymbolInfo symbol) {
			this.symbol = symbol;
			if( symbol.DisplayTimeZone == "UTC" ) {
				SetUtcTimeZone();
			} else if( symbol.DisplayTimeZone == "Local") {
				SetLocalTimeZone();
			} else if( symbol.DisplayTimeZone == "Exchange") {
				SetExchangeTimeZone();
			} else {
				throw new ApplicationException("Please, set the DisplayTimeZone property for the symbol " + symbol.Symbol + " in the symbol dictionary to either Exchange, Local, or UTC.");
			}
		}
		
		/// <summary>
		/// Returns the UTC offset in seconds at the specified time while taking into 
		/// consideration daylight savings time adjustments relative to the time
		/// of the argument passed into the method.
		/// </summary>
		/// <param name="timeStamp"></param>
		/// <returns></returns>
		public long UtcOffset( TimeStamp timeStamp) {
			return (long) timeZoneInfo.GetUtcOffset(timeStamp.DateTime).TotalSeconds;
		}
		
		public void SetCustomUtcOffset(int offset) {
			timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("Custom",new TimeSpan(offset,0,0),"Custom","Custom");
		}
		
		public void SetCustomTimeZone(string timeZone) {
			timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
		}
		
		public void SetLocalTimeZone() {
			timeZoneInfo = TimeZoneInfo.Local;
		}
		
		public void SetUtcTimeZone() {
			timeZoneInfo = TimeZoneInfo.Utc;
		}
		
		public void SetExchangeTimeZone() {
			if( symbol.TimeZone == null || symbol.TimeZone.Length == 0) {
				throw new ApplicationException("Please, set the TimeZone property for the symbol " + symbol.Symbol + " in the symbol dictionary.");
			}
			string tz = symbol.TimeZone;
			if( tz.StartsWith("UTC-") ) {
				tz = tz.Substring(3);
				SetCustomUtcOffset( int.Parse(tz));
			} else if(tz.StartsWith("UTC+") ) {
				tz = tz.Substring(4);
				SetCustomUtcOffset( int.Parse(tz));
			} else {
				SetCustomTimeZone(symbol.TimeZone);
			}
		}
	}
}
