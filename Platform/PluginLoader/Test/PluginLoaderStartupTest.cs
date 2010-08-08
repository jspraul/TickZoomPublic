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
using System.Reflection;

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Loader;

namespace TickZoom.Loader
{
	[TestFixture]
	public class PluginLoaderStartupTest
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(PluginLoaderStartupTest));
		[Test]
		public void StartupPluginLoaderTest()
		{
			CoreStartup start = new CoreStartup("TickZoomTest");
			start.StartCoreServices();
			start.RunInitialization();
			string addInConfig = 
@"<Plugin name        = ""LoaderTests""
       author      = ""Wayne Walter""
       url         = """"
       description = ""TODO: Put description here"">
	
	<Runtime>
		<Import assembly = ""TickZoomLoaderTests.dll""/>
	</Runtime>
	
	<Path name = ""/Tests/Loaders"">
		<Extension id=""MyLoader"" class=""TestLoader""/>
	</Path>
</Plugin>";
			var reader = new StringReader(addInConfig);
			Plugin plugin = Plugin.Load(reader,".");
			plugin.Enabled = true;
			PluginTree.InsertPlugin(plugin);
			ModelLoaderInterface loader = (ModelLoaderInterface) PluginTree.BuildItem("/Tests/Loaders/MyLoader", this);
			Assert.IsNotNull(loader);
		}
		
		[Test]
		public void TestCreateInstance() {
			var asm = Assembly.GetExecutingAssembly();
			var obj = asm.CreateInstance("TickZoom.Loader.TestLoader");
			Assert.IsNotNull(obj);
		}
	
	}
	
	public class TestLoader : ModelLoaderInterface {
		
		public ModelInterface TopModel {
			get {
				throw new NotImplementedException();
			}
		}
		
		public System.Collections.Generic.IList<ModelInterface> Models {
			get {
				throw new NotImplementedException();
			}
		}
		
		public System.Collections.Generic.List<ModelProperty> Variables {
			get {
				throw new NotImplementedException();
			}
		}
		
		public string Name {
			get {
				return this.GetType().Name;
			}
		}
		
		public string Category {
			get {
				throw new NotImplementedException();
			}
		}
		
		public bool IsVisibleInGUI {
			get {
				return false;
			}
		}
		
		public bool QuietMode {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
		
		public Stream OptimizeOutput {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
		
		public void OnInitialize(ProjectProperties properties)
		{
			throw new NotImplementedException();
		}
		
		public void OnLoad(ProjectProperties properties)
		{
			throw new NotImplementedException();
		}
		
		public void OnClear()
		{
			throw new NotImplementedException();
		}
	}
}
	
