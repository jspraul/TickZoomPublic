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
		Type userFixtureType = typeof(StrategyTest);
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(DynamicTestSuite));
		public Test BuildFrom(Type type)
		{
			var autoTestFixture = (IAutoTestFixture) Reflect.Construct(type);
			var mainSuite = new TestSuite("DynamicTest");
			var modesToRun = autoTestFixture.GetModesToRun();
			if( (modesToRun & AutoTestMode.Historical) == AutoTestMode.Historical) {
				AddDynamicTestFixtures(mainSuite,autoTestFixture, AutoTestMode.Historical);
			}
			if( (modesToRun & AutoTestMode.SimulateRealTime) == AutoTestMode.SimulateRealTime) {
				AddDynamicTestFixtures(mainSuite,autoTestFixture, AutoTestMode.SimulateRealTime);
			}
			if( (modesToRun & AutoTestMode.SimulateFIX) == AutoTestMode.SimulateFIX) {
				AddDynamicTestFixtures(mainSuite,autoTestFixture, AutoTestMode.SimulateFIX);
			}
			if( (modesToRun & AutoTestMode.RealTimePlayBack) == AutoTestMode.RealTimePlayBack) {
				AddDynamicTestFixtures(mainSuite,autoTestFixture, AutoTestMode.RealTimePlayBack);
			}
			return mainSuite;
		}
		
		private void AddDynamicTestFixtures(TestSuite mainSuite, IAutoTestFixture autoTestFixture, AutoTestMode autoTestMode) {
			var suite = new TestSuite(autoTestMode.ToString());
			mainSuite.Add(suite);
			foreach( var testSettings in autoTestFixture.GetAutoTestSettings() ) {
				if( (testSettings.Mode & autoTestMode) != autoTestMode) continue;
				testSettings.Mode = autoTestMode;
//				testSettings.StoreKnownGood = testSettings.StoreKnownGood && testSettings.Mode == AutoTestMode.Historical;
				var fixture = new NUnitTestFixture(userFixtureType, new object[] { testSettings } );
				fixture.TestName.Name = testSettings.Name;
				suite.Add(fixture);
				AddStrategyTestCases(fixture, testSettings);
				if( testSettings.Mode == AutoTestMode.SimulateRealTime) {
					AddSymbolTestCases(fixture, testSettings);
				}
			}
		}
			
		private void AddStrategyTestCases(NUnitTestFixture fixture, AutoTestSettings testSettings) {
			var strategyTest = (StrategyTest) Reflect.Construct(userFixtureType, new object[] { testSettings } );
			foreach( var modelName in strategyTest.GetModelNames()) {
				var paramaterizedTest = new ParameterizedMethodSuite(modelName);
				fixture.Add(paramaterizedTest);
				var parms = new ParameterSet();
				parms.Arguments = new object[] { modelName };
				var methods = strategyTest.GetType().GetMethods();
				foreach( var method in methods ) {
					var parameters = method.GetParameters();
					if( !method.IsSpecialName && method.IsPublic && parameters.Length == 1 && parameters[0].ParameterType == typeof(string)) {
						if( CheckIgnoreMethod(testSettings.IgnoreTests, method.Name)) {
							continue;
						}
						var testCase = NUnitTestCaseBuilder.BuildSingleTestMethod(method,parms);
						testCase.TestName.Name = method.Name;
						testCase.TestName.FullName = fixture.Parent.Parent.TestName.Name + "." +
							fixture.Parent.TestName.Name + "." + 
							fixture.TestName.Name + "." +
							modelName + "." +
							method.Name;
						paramaterizedTest.Add( testCase);
					}
				}
			}
		}
	
		private bool CheckIgnoreMethod(TestType ignoreTests, string methodName) {
        	var testTypeValues = Enum.GetValues(typeof(TestType));
	        foreach (TestType testType in testTypeValues)
	        {
	        	if ((ignoreTests & testType) == testType)
	            {
	        		if( methodName.Contains( testType.ToString())) {
	            	   	return true;
	            	}
	            }
	        }
	        return false;
		}
				           
		private void AddSymbolTestCases(NUnitTestFixture fixture, AutoTestSettings testSettings) {
			var strategyTest = (StrategyTest) Reflect.Construct(userFixtureType, new object[] { testSettings } );
			foreach( var symbol in strategyTest.GetSymbols()) {
				var paramaterizedTest = new ParameterizedMethodSuite(symbol.Symbol);
				fixture.Add(paramaterizedTest);
				var parms = new ParameterSet();
				parms.Arguments = new object[] { symbol };
				var methods = strategyTest.GetType().GetMethods();
				foreach( var method in methods ) {
					var parameters = method.GetParameters();
					if( !method.IsSpecialName && method.IsPublic && parameters.Length == 1 && parameters[0].ParameterType == typeof(SymbolInfo)) {
						var testCase = NUnitTestCaseBuilder.BuildSingleTestMethod(method,parms);
						testCase.TestName.Name = method.Name;
						testCase.TestName.FullName = fixture.Parent.Parent.TestName.Name + "." +
							fixture.Parent.TestName.Name + "." + 
							fixture.TestName.Name + "." +
							symbol + "." +
							method.Name;
						paramaterizedTest.Add( testCase);
					}
				}
			}
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
				if( !result && CheckCanBuild(autoTestFixture, AutoTestMode.SimulateRealTime)) {
					result = true;
				}
				if( !result && CheckCanBuild(autoTestFixture, AutoTestMode.SimulateFIX)) {
					result = true;
				}
				if( !result && CheckCanBuild(autoTestFixture, AutoTestMode.RealTimePlayBack)) {
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
