// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1252 $</version>
// </file>

using System;
using System.IO;
using NUnit.Framework;

namespace TickZoom.Loader.PluginTreeTests.Tests
{
	[TestFixture]
	public class PluginTreeLoadingTests
	{
		#region Plugin node tests
		[Test]
		public void TestEmptyPluginTreeLoading()
		{
			string pluginText = @"<Plugin/>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
		}
		
		[Test]
		public void TestPluginProperties()
		{
			string pluginText = @"
<Plugin name        = 'SharpDevelop Core'
       author      = 'Mike Krueger'
       copyright   = 'GPL'
       url         = 'http://www.icsharpcode.net'
       description = 'SharpDevelop core module'
       version     = '1.0.0'/>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(plugin.Properties["name"], "SharpDevelop Core");
			Assert.AreEqual(plugin.Properties["author"], "Mike Krueger");
			Assert.AreEqual(plugin.Properties["copyright"], "GPL");
			Assert.AreEqual(plugin.Properties["url"], "http://www.icsharpcode.net");
			Assert.AreEqual(plugin.Properties["description"], "SharpDevelop core module");
			Assert.AreEqual(plugin.Properties["version"], "1.0.0");
		}
		#endregion
		
		#region Runtime section tests
		[Test]
		public void TestEmtpyRuntimeSection()
		{
			string pluginText = @"<Plugin><Runtime/></Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
		}
		
		[Test]
		public void TestEmtpyRuntimeSection2()
		{
			string pluginText = @"<Plugin> <!-- Comment1 --> <Runtime>  <!-- Comment2 -->    </Runtime> <!-- Comment3 --> </Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
		}
		
		[Test]
		public void TestRuntimeSectionImport()
		{
			string pluginText = @"
<Plugin>
	<Runtime>
		<Import assembly = 'Test.dll'/>
	</Runtime>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(1, plugin.Runtimes.Count);
			Assert.AreEqual(plugin.Runtimes[0].Assembly, "Test.dll");
		}
		
		[Test]
		public void TestRuntimeSectionComplexImport()
		{
			string pluginText = @"
<Plugin>
	<Runtime>
		<Import assembly = '../bin/SharpDevelop.Base.dll'>
			<Doozer             name='MyDoozer'   class = 'ICSharpCode.Core.ClassDoozer'/>
			<ConditionEvaluator name='MyCompare'  class = 'ICSharpCode.Core.CompareCondition'/>
			<Doozer             name='Test'       class = 'ICSharpCode.Core.ClassDoozer2'/>
			<ConditionEvaluator name='Condition2' class = 'Condition2Class'/>
		</Import>
	</Runtime>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(1, plugin.Runtimes.Count);
			Assert.AreEqual(plugin.Runtimes[0].Assembly, "../bin/SharpDevelop.Base.dll");
			Assert.AreEqual(plugin.Runtimes[0].DefinedDoozers.Count, 2);
			Assert.AreEqual(plugin.Runtimes[0].DefinedDoozers[0].Name, "MyDoozer");
			Assert.AreEqual(plugin.Runtimes[0].DefinedDoozers[0].ClassName, "ICSharpCode.Core.ClassDoozer");
			
			Assert.AreEqual(plugin.Runtimes[0].DefinedDoozers[1].Name, "Test");
			Assert.AreEqual(plugin.Runtimes[0].DefinedDoozers[1].ClassName, "ICSharpCode.Core.ClassDoozer2");
			
			Assert.AreEqual(plugin.Runtimes[0].DefinedConditionEvaluators.Count, 2);
			Assert.AreEqual(plugin.Runtimes[0].DefinedConditionEvaluators[0].Name, "MyCompare");
			Assert.AreEqual(plugin.Runtimes[0].DefinedConditionEvaluators[0].ClassName, "ICSharpCode.Core.CompareCondition");
			
			Assert.AreEqual(plugin.Runtimes[0].DefinedConditionEvaluators[1].Name, "Condition2");
			Assert.AreEqual(plugin.Runtimes[0].DefinedConditionEvaluators[1].ClassName, "Condition2Class");
		}
		#endregion
		
		#region Path section tests
		[Test]
		public void TestEmptyPathSection()
		{
			string pluginText = @"
<Plugin>
	<Path name = '/Path1'/>
	<Path name = '/Path2'/>
	<Path name = '/Path1/SubPath'/>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(3, plugin.Paths.Count);
			Assert.IsNotNull(plugin.Paths["/Path1"]);
			Assert.IsNotNull(plugin.Paths["/Path2"]);
			Assert.IsNotNull(plugin.Paths["/Path1/SubPath"]);
		}
		
		[Test]
		public void TestSimpleCodon()
		{
			string pluginText = @"
<Plugin>
	<Path name = '/Path1'>
		<Simple id ='Simple' attr='a' attr2='b'/>
	</Path>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(1, plugin.Paths.Count);
			Assert.IsNotNull(plugin.Paths["/Path1"]);
			Assert.AreEqual(1, plugin.Paths["/Path1"].Codons.Count);
			Assert.AreEqual("Simple", plugin.Paths["/Path1"].Codons[0].Type);
			Assert.AreEqual("Simple", plugin.Paths["/Path1"].Codons[0].Id);
			Assert.AreEqual("a", plugin.Paths["/Path1"].Codons[0].Properties["attr"]);
			Assert.AreEqual("b", plugin.Paths["/Path1"].Codons[0].Properties["attr2"]);
		}
		
		[Test]
		public void TestSubCodons()
		{
			string pluginText = @"
<Plugin>
	<Path name = '/Path1'>
		<Sub id='Path2'>
			<Extension type='Codon2' id='Sub2'/>
		</Sub>
	</Path>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(2, plugin.Paths.Count);
			Assert.IsNotNull(plugin.Paths["/Path1"]);
			Assert.AreEqual(1, plugin.Paths["/Path1"].Codons.Count);
			Assert.AreEqual("Sub", plugin.Paths["/Path1"].Codons[0].Type);
			Assert.AreEqual("Path2", plugin.Paths["/Path1"].Codons[0].Id);
			
			Assert.IsNotNull(plugin.Paths["/Path1/Path2"]);
			Assert.AreEqual(1, plugin.Paths["/Path1/Path2"].Codons.Count);
			Assert.AreEqual("Codon2", plugin.Paths["/Path1/Path2"].Codons[0].Type);
			Assert.AreEqual("Sub2", plugin.Paths["/Path1/Path2"].Codons[0].Id);
		}
		
		[Test]
		public void TestSubCodonsWithCondition()
		{
			string pluginText = @"
<Plugin>
	<Path name = '/Path1'>
		<Condition name='Equal' string='a' equal='b'>
			<Sub id='Path2'>
				<Extension type='Codon2' id='Sub2'/>
			</Sub>
		</Condition>
	</Path>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(2, plugin.Paths.Count);
			Assert.IsNotNull(plugin.Paths["/Path1"]);
			Assert.AreEqual(1, plugin.Paths["/Path1"].Codons.Count);
			Assert.AreEqual("Sub", plugin.Paths["/Path1"].Codons[0].Type);
			Assert.AreEqual("Path2", plugin.Paths["/Path1"].Codons[0].Id);
			Assert.AreEqual(1, plugin.Paths["/Path1"].Codons[0].Conditions.Length);
			
			Assert.IsNotNull(plugin.Paths["/Path1/Path2"]);
			Assert.AreEqual(1, plugin.Paths["/Path1/Path2"].Codons.Count);
			Assert.AreEqual("Codon2", plugin.Paths["/Path1/Path2"].Codons[0].Type);
			Assert.AreEqual("Sub2", plugin.Paths["/Path1/Path2"].Codons[0].Id);
			Assert.AreEqual(0, plugin.Paths["/Path1/Path2"].Codons[0].Conditions.Length);
		}
		
		[Test]
		public void TestSimpleCondition()
		{
			string pluginText = @"
<Plugin>
	<Path name = '/Path1'>
		<Condition name='Equal' string='a' equal='b'>
			<Simple id ='Simple' attr='a' attr2='b'/>
		</Condition>
	</Path>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(1, plugin.Paths.Count, "Paths != 1");
			ExtensionPath path = plugin.Paths["/Path1"];
			Assert.IsNotNull(path);
			Assert.AreEqual(1, path.Codons.Count);
			Extension codon = path.Codons[0];
			Assert.AreEqual("Simple", codon.Type);
			Assert.AreEqual("Simple", codon.Id);
			Assert.AreEqual("a",      codon["attr"]);
			Assert.AreEqual("b",      codon["attr2"]);
			
			// Test for condition.
			Assert.AreEqual(1, codon.Conditions.Length);
			Condition condition = codon.Conditions[0] as Condition;
			Assert.IsNotNull(condition);
			Assert.AreEqual("Equal", condition.Name);
			Assert.AreEqual("a", condition["string"]);
			Assert.AreEqual("b", condition["equal"]);
			
		}
		
		[Test]
		public void TestStackedCondition()
		{
			string pluginText = @"
<Plugin>
	<Path name = '/Path1'>
		<Condition name='Equal' string='a' equal='b'>
			<Condition name='StackedCondition' string='1' equal='2'>
				<Simple id ='Simple' attr='a' attr2='b'/>
			</Condition>
			<Extension factory='Simple' id ='Simple2' attr='a' attr2='b'/>
		</Condition>
			<Extension factory='Simple' id ='Simple3' attr='a' attr2='b'/>
	</Path>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(1, plugin.Paths.Count);
			ExtensionPath path = plugin.Paths["/Path1"];
			Assert.IsNotNull(path);
			
			Assert.AreEqual(3, path.Codons.Count);
			Extension codon = path.Codons[0];
			Assert.AreEqual("Simple", codon.Type);
			Assert.AreEqual("Simple", codon.Id);
			Assert.AreEqual("a",      codon["attr"]);
			Assert.AreEqual("b",      codon["attr2"]);
			
			// Test for condition
			Assert.AreEqual(2, codon.Conditions.Length);
			Condition condition = codon.Conditions[1] as Condition;
			Assert.IsNotNull(condition);
			Assert.AreEqual("Equal", condition.Name);
			Assert.AreEqual("a", condition["string"]);
			Assert.AreEqual("b", condition["equal"]);
			
			condition = codon.Conditions[0] as Condition;
			Assert.IsNotNull(condition);
			Assert.AreEqual("StackedCondition", condition.Name);
			Assert.AreEqual("1", condition["string"]);
			Assert.AreEqual("2", condition["equal"]);
			
			codon = path.Codons[1];
			Assert.AreEqual(1, codon.Conditions.Length);
			condition = codon.Conditions[0] as Condition;
			Assert.IsNotNull(condition);
			Assert.AreEqual("Equal", condition.Name);
			Assert.AreEqual("a", condition["string"]);
			Assert.AreEqual("b", condition["equal"]);
			
			codon = path.Codons[2];
			Assert.AreEqual(0, codon.Conditions.Length);
			
		}
		
		[Test]
		public void TestComplexCondition()
		{
			string pluginText = @"
<Plugin>
	<Path name = '/Path1'>
		<ComplexCondition>
			<And>
				<Not><Condition name='Equal' string='a' equal='b'/></Not>
				<Or>
					<Condition name='Equal' string='a' equal='b'/>
					<Condition name='Equal' string='a' equal='b'/>
					<Condition name='Equal' string='a' equal='b'/>
				</Or>
			</And>
			<Simple id='Simple' attr='a' attr2='b'/>
		</ComplexCondition>
	</Path>
</Plugin>";
			Plugin plugin = Plugin.Load(new StringReader(pluginText));
			Assert.AreEqual(1, plugin.Paths.Count);
			ExtensionPath path = plugin.Paths["/Path1"];
			Assert.IsNotNull(path);
			Assert.AreEqual(1, path.Codons.Count);
			Extension codon = path.Codons[0];
			Assert.AreEqual("Simple", codon.Type);
			Assert.AreEqual("Simple", codon.Id);
			Assert.AreEqual("a",      codon["attr"]);
			Assert.AreEqual("b",      codon["attr2"]);
			
			// Test for condition.
			Assert.AreEqual(1, codon.Conditions.Length);
		}
		
		#endregion
	}
}
