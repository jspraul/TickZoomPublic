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
using NUnit.Framework;
using TickZoom.Api;

namespace TickZoom.Utilities
{
	[TestFixture]
	public class SymbolTest
	{
		[Test]
		public void TestSymbol()
		{
			string symbol = "USD/JPY";
			ulong uSymbol = ExtensionMethods.SymbolToULong(symbol);
			string symbolConverted = ExtensionMethods.ULongToSymbol(uSymbol);
			Assert.AreEqual(symbol,symbolConverted);
//			Assert.AreEqual(1515607893785,uSymbol);
		}
		
		[Test]
		public void TestSpecific()
		{
			string symbol = "Daily4Tic";
			ulong uSymbol = ExtensionMethods.SymbolToULong(symbol);
			string symbolConverted = ExtensionMethods.ULongToSymbol(uSymbol);
			Assert.AreEqual(symbol,symbolConverted);
//			Assert.AreEqual(1515607893785,uSymbol);
		}
		
		[Test]
		public void TestMaxString2()
		{
			string expectedSymbol = "AAAAAAAAA";
			ulong uSymbol = ExtensionMethods.SymbolToULong(expectedSymbol);
			string symbol = ExtensionMethods.ULongToSymbol(uSymbol);
			Assert.AreEqual(expectedSymbol,symbol);
		}
		
		[Test]
		public void TestCharToInt()
		{
			Assert.AreEqual(1,ExtensionMethods.CharToInt('A'));
			Assert.AreEqual(26,ExtensionMethods.CharToInt('Z'));
			Assert.AreEqual(27,ExtensionMethods.CharToInt('a'));
			Assert.AreEqual(52,ExtensionMethods.CharToInt('z'));
			Assert.AreEqual(53,ExtensionMethods.CharToInt('0'));
			Assert.AreEqual(62,ExtensionMethods.CharToInt('9'));
			Assert.AreEqual(63,ExtensionMethods.CharToInt('/'));
		}
		
		[Test]
		public void TestIntToChar()
		{
			Assert.AreEqual('A',ExtensionMethods.IntToChar(1));
			Assert.AreEqual('Z',ExtensionMethods.IntToChar(26));
			Assert.AreEqual('a',ExtensionMethods.IntToChar(27));
			Assert.AreEqual('z',ExtensionMethods.IntToChar(52));
			Assert.AreEqual('0',ExtensionMethods.IntToChar(53));
			Assert.AreEqual('9',ExtensionMethods.IntToChar(62));
			Assert.AreEqual('/',ExtensionMethods.IntToChar(63));
		}
		
		[Test]
		public void TestWildCard() {
			string testString = "APX_Master_USD/JPY";
			Assert.IsTrue( testString.CompareWildcard("APX_Master_USD/JPY",false));
			Assert.IsTrue( testString.CompareWildcard("APX_Master*",false));
			Assert.IsTrue( testString.CompareWildcard("*_Master_USD/JPY",false));
			Assert.IsTrue( testString.CompareWildcard("APX_*_USD/JPY",false));
			Assert.IsTrue( testString.CompareWildcard("APX_*_USD/*",false));
			Assert.IsFalse( testString.CompareWildcard("",false));
			Assert.IsFalse( testString.CompareWildcard("APZ_Master*",false));
			Assert.IsFalse( testString.CompareWildcard("*_Master4_USD/JPY",false));
			Assert.IsFalse( testString.CompareWildcard("APX_*_USD3/JPY",false));
			Assert.IsFalse( testString.CompareWildcard("AOPX_*_USD/*",false));
			testString = "ExampleReversalStrategy-Pass-1";
			Assert.IsFalse( testString.CompareWildcard("ExampleReversalStrategy",false));
//			testString = "";
//			Assert.IsTrue( testString.CompareWildcard("",false));
		}
		
	}
}
