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
using TickZoom.Common;

#if TESTING
namespace TickZoom.TradingFramework
{
	[TestFixture]
	public class ChainTest
	{
		Chain chain;
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(ChainTest));
		private readonly bool debug = log.IsDebugEnabled;
		private readonly bool trace = log.IsTraceEnabled;
		
		
		
		[SetUp]
		public void CreateChain()
		{
			log.Notice("Setup ChainTest");
			
			Model formula = new Model();
			formula.Name = "third";
			chain = formula.Chain;
			if( trace) log.Trace( formula.Chain.ToChainString());
			
			formula = new Model();
			formula.Name = "second";
			chain = chain.InsertBefore( formula.Chain);
			if( trace) log.Trace( chain.ToChainString());
			
			formula = new Model();
			formula.Name = "first";
			chain.InsertBefore( formula.Chain);
			if( trace) log.Trace( chain.ToChainString());
		}
		
		[TearDown]
		public void Destructor() {
//			Logger.Disconnect();
		}
		
		[Test]
		public void TestOrder()
		{
			int i = 0;
			Chain current=chain.Root;
			do{ 
				ModelInterface formula = current.Model;
				i++;
				switch( i) {
					case 1:
						Assert.AreEqual("first",formula.Name);
						break;
					case 2:
						Assert.AreEqual("second", formula.Name);
						break;
					case 3:
						Assert.AreEqual("third", formula.Name);
						break;
				}
				current=current.Next;
			} while( current.Model!=null);
			
			Assert.AreEqual(3,i);
		}
		
		[Test]
		public void TestInsert() {
			Chain link = chain.GetAt(1);
			Assert.AreEqual("second",link.Model.Name);
			Model formula = new Model();
			formula.Name = "inserted";
			link.InsertBefore(formula.Chain);
			link = chain.GetAt(1);
			Assert.AreEqual("inserted",link.Model.Name);
		}
	}
}
#endif