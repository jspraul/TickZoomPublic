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
using TickZoom.Api;

namespace Loaders
{
	[SuiteBuilder]
	public class DynamicTestSuite : ISuiteBuilder
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(DynamicTestSuite));
		public Test BuildFrom(Type type)
		{
			var autoTestFixture = (IAutoTestFixture) Reflect.Construct(type);
			var mainSuite = new TestSuite("DynamicTest");
			AddDynamicTestFixtures(mainSuite,autoTestFixture, AutoTestMode.Historical);
			AddDynamicTestFixtures(mainSuite,autoTestFixture, AutoTestMode.RealTime);
			AddDynamicTestFixtures(mainSuite,autoTestFixture, AutoTestMode.SimulateFIX);
			return mainSuite;
		}
		
		private void AddDynamicTestFixtures(TestSuite mainSuite, IAutoTestFixture autoTestFixture, AutoTestMode autoTestMode) {
			var suite = new TestSuite(autoTestMode.ToString());
			mainSuite.Add(suite);
			foreach( var testSettings in autoTestFixture.GetAutoTestSettings() ) {
				if( (testSettings.Mode & autoTestMode) != autoTestMode) continue;
				testSettings.Mode = autoTestMode;
				AddDynamicTestCases(suite, testSettings);
			}
		}
			
		private void AddDynamicTestCases(TestSuite suite, AutoTestSettings testSettings) {
			var userFixtureType = typeof(StrategyTest);
			var strategyTest = (StrategyTest) Reflect.Construct(userFixtureType, new object[] { testSettings } );
			var fixture = new NUnitTestFixture(userFixtureType, new object[] { testSettings } );
			fixture.TestName.Name = testSettings.Name;
			foreach( var modelName in strategyTest.GetModelNames()) {
				var paramaterizedTest = new ParameterizedMethodSuite(modelName);
				fixture.Add(paramaterizedTest);
				var parms = new ParameterSet();
				parms.Arguments = new object[] { modelName };
				var methods = strategyTest.GetType().GetMethods();
				foreach( var method in methods ) {
					var parameters = method.GetParameters();
					if( !method.IsSpecialName && method.IsPublic && parameters.Length == 1 && parameters[0].ParameterType == typeof(string)) {
						var testCase = NUnitTestCaseBuilder.BuildSingleTestMethod(method,parms);
						testCase.TestName.Name = method.Name;
						testCase.TestName.FullName = suite.Parent.TestName.Name + "." +
							suite.TestName.Name + "." + 
							fixture.TestName.Name + "." +
							modelName + "." +
							method.Name;
						paramaterizedTest.Add( testCase);
					}
				}
			}
			suite.Add(fixture);
		}
	
		public bool CanBuildFrom(Type type)
		{
			var result = false;
			if( Reflect.HasAttribute( type, typeof(AutoTestFixtureAttribute).FullName, false) 
			   && Reflect.HasInterface( type, typeof(IAutoTestFixture).FullName) ) {
				var autoTestFixture = (IAutoTestFixture) Reflect.Construct(type);
				if( !result && CheckCanBuild(autoTestFixture, AutoTestMode.Historical)) {
					result = true;
				}
				if( !result && CheckCanBuild(autoTestFixture, AutoTestMode.RealTime)) {
					result = true;
				}
				if( !result && CheckCanBuild(autoTestFixture, AutoTestMode.SimulateFIX)) {
					result = true;
				}
			}
			return result;
		}		
		
		private bool CheckCanBuild(IAutoTestFixture autoTestFixture, AutoTestMode autoTestMode) {
			var result = false;
			foreach( var testSettings in autoTestFixture.GetAutoTestSettings() ) {
				if( (testSettings.Mode & autoTestMode) != autoTestMode) continue;
				testSettings.Mode = autoTestMode;
				var userFixtureType = typeof(StrategyTest);
				var strategyTest = (StrategyTest) Reflect.Construct(userFixtureType, new object[] { testSettings } );
				foreach( var modelName in strategyTest.GetModelNames()) {
					result = true; // If at least one entry.
					break;
				}
			}
			return result;
		}
	}
}
