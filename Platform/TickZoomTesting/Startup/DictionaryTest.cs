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
using System.IO;
using NUnit.Framework;
using TickZoom.Common;
using TickZoom.Symbols;

namespace TickZoom.StarterTest 
{
	[TestFixture]
	public class DictionaryTest
	{
		[Test]
		public void LoadMBTrading()
		{
			string fileName = @"..\..\Platform\TickZoomTesting\Startup\dictionary.tzdict";
			SymbolDictionary dictionary = SymbolDictionary.Create(new StreamReader(fileName));
			foreach( SymbolProperties properties in dictionary) {
				InstrumentImpl instrument = InstrumentImpl.Get(properties.Symbol);
				Assert.AreEqual( instrument.DepthIncrement, properties.Level2Increment);
				Assert.AreEqual( instrument.LotSize, properties.Level2LotSize);
				Assert.AreEqual( instrument.LotSizeDomLimit, properties.Level2LotSizeMinimum);
			}
		}
		[Test]
		public void LoadMBTUSDJPY()
		{
			string fileName = @"..\..\Platform\TickZoomTesting\Startup\dictionary.tzdict";
			SymbolDictionary dictionary = SymbolDictionary.Create(new StreamReader(fileName));
			SymbolProperties properties = dictionary.Get("USD/JPY");
			InstrumentImpl instrument = InstrumentImpl.Get("USD/JPY");
			Assert.AreEqual( instrument.DepthIncrement, properties.Level2Increment);
			Assert.AreEqual( instrument.LotSize, properties.Level2LotSize);
			Assert.AreEqual( instrument.LotSizeDomLimit, properties.Level2LotSizeMinimum);
		}
	}
}
