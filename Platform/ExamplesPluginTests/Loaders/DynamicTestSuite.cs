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
using NUnit.Core;
using NUnit.Core.Builders;
using NUnit.Core.Extensibility;
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Starters;

namespace Loaders
{
//	public class SetupAutoTests {
//		public SetupAutoTests() {
//			var startTime = new TimeStamp(2009,1,1);
//			var endTime = new TimeStamp(2010,1,1);
//			var intervalDefault = Intervals.Minute1;
//			var starter = new HistoricalStarter {
//				ProjectProperties.Starter.StartTime = startTime,
//	    		ProjectProperties.Starter.EndTime = endTime,
//	    		DataFolder = "Test\\DataCache",
////	    		ProjectProperties.Starter.SetSymbols( Symbols),
//				ProjectProperties.Starter.IntervalDefault = intervalDefault
//			};
//		}
//	}
	[SuiteBuilder]
	public class DynamicTestSuite : ISuiteBuilder
	{
		object[] arguments = new object[] {
			"APX_Systems: APX Multi-Symbol Loader",
			"USD/JPY",
			true,
			false,
			new TimeStamp( 1800, 1, 1),
			new TimeStamp( 2009, 6, 10),
		};
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(DynamicTestSuite));
		public Test BuildFrom(Type type)
		{
			var suite = new TestSuite(this.GetType().Name);
			var userFixtureType = typeof(StrategyTest);
			var strategyTest = (StrategyTest) Reflect.Construct(userFixtureType, arguments);
			var fixture = new NUnitTestFixture(userFixtureType,arguments);
			fixture.TestName.Name = "Dynamic-ApexStrategyTest";
			foreach( var modelName in strategyTest.GetModelNames()) {
				var paramaterizedTest = new ParameterizedMethodSuite(modelName);
				fixture.Add(paramaterizedTest);
				var parms = new ParameterSet();
				parms.Arguments = new object[] { modelName };
				var modelNames = userFixtureType.GetMethods();
				foreach( var method in modelNames ) {
					var parameters = method.GetParameters();
					if( !method.IsSpecialName && method.IsPublic && parameters.Length == 1 && parameters[0].ParameterType == typeof(string)) {
						var testCase = NUnitTestCaseBuilder.BuildSingleTestMethod(method,parms);
						testCase.TestName.Name = method.Name;
						paramaterizedTest.Add( testCase);
					}
				}
			}
			suite.Add(fixture);
			return suite;
		}
	
		public bool CanBuildFrom(Type type)
		{
			var result = false;
			if( type == typeof(DynamicTestSuite)) {
				var userFixtureType = typeof(StrategyTest);
				var strategyTest = (StrategyTest) Reflect.Construct(userFixtureType, arguments);
				try {
					foreach( var modelName in strategyTest.GetModelNames()) {
						result = true; // If at least one entry.
						break;
					}
				} catch( ApplicationException ex) {
					if( !ex.Message.Contains("not found") ) {
						throw;
					}
				}
			}
			return result;
		}		
	}
	
    public class ParameterizedMethodSuite : TestSuite
    {
        public ParameterizedMethodSuite(string name)
            : base(name, name)
        {
            this.maintainTestOrder = true;
        }

        public override TestResult Run(EventListener listener, ITestFilter filter)
        {
            if (this.Parent != null)
            {
                this.Fixture = this.Parent.Fixture;
                TestSuite suite = this.Parent as TestSuite;
                if (suite != null)
                {
                    this.setUpMethods = suite.GetSetUpMethods();
                    this.tearDownMethods = suite.GetTearDownMethods();
                }
            }
            return base.Run(listener, filter);
        }

        protected override void DoOneTimeSetUp(TestResult suiteResult)
        {
        }
        protected override void DoOneTimeTearDown(TestResult suiteResult)
        {
        }
    }
}
