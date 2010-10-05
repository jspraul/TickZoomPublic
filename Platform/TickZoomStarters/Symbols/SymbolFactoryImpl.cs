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
using TickZoom.Api;

namespace TickZoom.Symbols
{

	/// <summary>
	/// Description of SymbolFactory.
	/// </summary>
	public class SymbolFactoryImpl : SymbolFactory
	{
		SymbolLibrary library;
		
		public SymbolLibrary Library {
			get { 
				if( library == null) {
					lock( locker) {
						library = new SymbolLibrary();
					}
				}
				return library;
			}
		}
		object locker = new object();
		public SymbolFactoryImpl()
		{
		}
		
		public SymbolInfo LookupSymbol(string symbol)
		{
			return Library.LookupSymbol(symbol);
		}
		
		public SymbolInfo LookupSymbol(long identifier)
		{
			return Library.LookupSymbol(identifier);
		}
		
		public bool TryLookupSymbol(string symbol, out SymbolInfo symbolInfo)
		{
			return Library.LookupSymbol(symbol, out symbolInfo);
		}
		
		public bool TryLookupSymbol(long identifier, out SymbolInfo symbolInfo)
		{
			return Library.LookupSymbol(identifier, out symbolInfo);
		}
		
	}
}
