#region Copyright

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
