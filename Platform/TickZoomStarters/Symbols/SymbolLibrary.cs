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
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Symbols
{
	public class SymbolLibrary 
	{
		Dictionary<string,SymbolProperties> symbolMap;
		Dictionary<long,SymbolProperties> universalMap;
		public SymbolLibrary() {
			SymbolDictionary dictionary = SymbolDictionary.Create("universal",SymbolDictionary.UniversalDictionary);
			IEnumerable<SymbolProperties> enumer = dictionary;
			symbolMap = new Dictionary<string, SymbolProperties>();
			foreach( SymbolProperties symbolProperties in dictionary) {
				symbolMap[symbolProperties.Symbol] = symbolProperties;
			}
			dictionary = SymbolDictionary.Create("user",SymbolDictionary.UserDictionary);
			foreach( SymbolProperties symbolProperties in dictionary) {
				symbolMap[symbolProperties.Symbol] = symbolProperties;
			}
			AddAbbreviations();
			AdjustSessions();
			CreateUniversalIds();
		}
		
		private void CreateUniversalIds() {
			long universalIdentifier = 1;
			universalMap = new Dictionary<long, SymbolProperties>();
			foreach( var kvp in symbolMap) {
				kvp.Value.BinaryIdentifier = universalIdentifier;
				universalMap.Add(universalIdentifier,kvp.Value);
				universalIdentifier ++;
			}
		}

		private void AddAbbreviations() {
			var tempSymbolMap = new Dictionary<string,SymbolProperties>();
			foreach( var kvp in symbolMap) {
				SymbolProperties symbolProperties = kvp.Value;
				string symbol = kvp.Key;
				tempSymbolMap.Add(symbol,symbolProperties);
				string abbreviation = symbolProperties.Symbol.StripInvalidPathChars();
				if( !symbolMap.ContainsKey(abbreviation)) {
					tempSymbolMap[abbreviation] = symbolProperties;
				}
			}
			symbolMap = tempSymbolMap;
		}
		
		private void AdjustSessions() {
			foreach( var kvp in symbolMap) {
				string symbol = kvp.Key;
				SymbolProperties symbolProperties = kvp.Value;
				if( symbolProperties.TimeZone == null || symbolProperties.TimeZone.Length == 0) {
					continue;
				}
				if( symbolProperties.DisplayTimeZone == "Local" ||
				   symbolProperties.DisplayTimeZone == "UTC" ) {
					// Convert session times from Exchange to UTC.
					SymbolTimeZone timeZone = new SymbolTimeZone(symbolProperties);
					timeZone.SetExchangeTimeZone();
					int startOffset = (int) timeZone.UtcOffset(new TimeStamp());
					int endOffset = (int) timeZone.UtcOffset(new TimeStamp());
					Elapsed utcSessionStart = symbolProperties.SessionStart - new Elapsed(0,0,startOffset);
					Elapsed utcSessionEnd = symbolProperties.SessionEnd - new Elapsed(0,0,endOffset);
					// Convert UTCI session times to either Local or UTC as chosen
					// by the DisplayTimeZone property.
					timeZone = new SymbolTimeZone(symbolProperties);
					startOffset = (int) timeZone.UtcOffset(new TimeStamp());
					endOffset = (int) timeZone.UtcOffset(new TimeStamp());
					symbolProperties.SessionStart = utcSessionStart + new Elapsed(0,0,startOffset);
					symbolProperties.SessionEnd = utcSessionEnd + new Elapsed(0,0,endOffset);
				}
			}
		}
		
		private char[] splitChar = new char[] { '.' };
		public bool GetSymbolProperties(string symbol, out SymbolProperties properties) {
			string[] symbolParts = symbol.Split( splitChar);
			symbol = symbolParts[0];
			return symbolMap.TryGetValue(symbol.Trim(),out properties);
		}
		public SymbolProperties GetSymbolProperties(string symbol) {
			SymbolProperties properties;
			if( GetSymbolProperties( symbol, out properties)) {
				return properties;
			} else {
				throw new ApplicationException( "Sorry, symbol " + symbol + " was not found in any symbol dictionary.");
			}
		}
		
		public SymbolInfo LookupSymbol(string symbol) {
			return GetSymbolProperties(symbol);
		}
	
		public bool LookupSymbol(string symbol, out SymbolInfo symbolInfo) {
			SymbolProperties properties;
			if( GetSymbolProperties(symbol, out properties)) {
				symbolInfo = properties;
				return true;
			} else {
				symbolInfo = null;
				return false;
			}
		}
	
		public SymbolInfo LookupSymbol(long universalIdentifier) {
			SymbolProperties symbolProperties;
			if( universalMap.TryGetValue(universalIdentifier,out symbolProperties)) {
				return symbolProperties;
			} else {
				throw new ApplicationException( "Sorry, universal id " + universalIdentifier + " was not found in any symbol dictionary.");
			}
		}
		public bool LookupSymbol(long universalIdentifier, out SymbolInfo symbolInfo) {
			SymbolProperties symbolProperties;
			if( universalMap.TryGetValue(universalIdentifier,out symbolProperties)) {
				symbolInfo = symbolProperties;
				return true;
			} else {
				symbolInfo = null;
				return false;
			}
		}
	}
}
