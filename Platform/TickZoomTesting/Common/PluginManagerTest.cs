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

namespace TickZoom.Common
{
	[TestFixture]
	public class PluginManagerTest
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		[SetUp]
		public void TestSetup() {
		}
		
		[Test]
		public void TestModelLoaderManager()
		{
			Plugins manager = Plugins.Instance;
			Assert.IsTrue(typeof(ModelLoaderInterface).IsAssignableFrom(manager.GetLoader("Test: Simple Single-Symbol").GetType()));
			Assert.IsTrue(typeof(ModelLoaderInterface).IsAssignableFrom(manager.GetLoaders()[0].GetType()));
			Assert.AreEqual(0,manager.ErrorCount, "Check the log for detailed errors.");
		}
		[Test]
		public void TestPluginsForModel()
		{
			Plugins manager = Plugins.Instance;
			Assert.IsTrue(typeof(ModelInterface).IsAssignableFrom(manager.GetModel("ExampleReversalStrategy").GetType()));
			Assert.IsTrue(typeof(ModelInterface).IsAssignableFrom(manager.Models[0]));
			Assert.AreEqual(0,manager.ErrorCount, "Check the log for detailed errors.");
		}
		[Test]
		public void TestPluginsForStrategy()
		{
			Plugins manager = Plugins.Instance;
			Assert.IsTrue(typeof(StrategyInterface).IsAssignableFrom(manager.GetModel("ExampleReversalStrategy").GetType()));
			Assert.IsTrue(typeof(ModelInterface).IsAssignableFrom(manager.Models[0]));
			Assert.AreEqual(0,manager.ErrorCount, "Check the log for detailed errors.");
		}
		[Test]
		public void TestPluginsForIndicator()
		{
			Plugins manager = Plugins.Instance;
			Assert.IsTrue(typeof(ModelInterface).IsAssignableFrom(manager.GetModel("IndicatorCommon").GetType()));
			Assert.IsTrue(typeof(ModelInterface).IsAssignableFrom(manager.Models[0]));
			Assert.AreEqual(0,manager.ErrorCount, "Check the log for detailed errors.");
		}
	}
}
