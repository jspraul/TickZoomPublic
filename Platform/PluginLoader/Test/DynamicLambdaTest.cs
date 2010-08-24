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
using System.Linq.Dynamic;
using System.Linq.Expressions;

using NUnit.Framework;
using TickZoom.Loader;

namespace Loader
{
	[TestFixture]
	public class DynamicLambdaTest
	{
		public DynamicLambdaTest()
		{
			StringParser.Properties["test"] = "Value";
			StringParser.PropertyObjects["obj"] = this;
		}
		
		public class Customer {
			
		}
		
		[Test]
		public void SymbolDynamicTest() {
			Expression<Func<Customer, bool>> e =
			    DynamicExpression.ParseLambda<Customer, bool>(
			        "45 + 37 > 10", null);
			var function = e.Compile();
			var result = function( new Customer());
			Assert.IsTrue(result);    
		}
		
		[Test]
		public void StringLambdaTest() {
			var e = DynamicExpression.ParseLambda<Customer, string>(
			        "\"Wayne\"", null);
			var function = e.Compile();
			var result = function( new Customer());
			Assert.AreEqual("Wayne",result);
		}
		
		[Test]
		public void MultipleParametersTest() {
			var parameters = new List<ParameterExpression>();
			var x = Expression.Parameter(typeof(int), "x");
			parameters.Add(x);
			string expresssion = "x+22";
			string stringExpression = "(" + expresssion + ").ToString()";
			var e = DynamicExpression.ParseLambda( parameters.ToArray(), typeof(string),
			        stringExpression, null);
			Delegate function = e.Compile();
			var result = function.DynamicInvoke( 12);
			Assert.AreEqual("34",result);
		}
	}
}
