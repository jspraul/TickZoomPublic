// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections;

namespace TickZoom.Loader
{
	/// <summary>
	/// This doozer lazy-loads another doozer when it has to build an item.
	/// It is used internally to wrap doozers specified in addins.
	/// </summary>
	public class LazyLoadDoozer : IDoozer
	{
		Plugin plugin;
		string name;
		string className;
		
		public string Name {
			get {
				return name;
			}
		}
		
		public string ClassName {
			get {
				return className;
			}
		}
		
		public LazyLoadDoozer(Plugin plugin, Properties properties)
		{
			this.plugin      = plugin;
			this.name       = properties["name"];
			this.className  = properties["class"];
			
		}
		
		/// <summary>
		/// Gets if the doozer handles codon conditions on its own.
		/// If this property return false, the item is excluded when the condition is not met.
		/// </summary>
		public bool HandleConditions {
			get {
				IDoozer doozer = (IDoozer)plugin.CreateObject(className);
				if (doozer == null) {
					return false;
				}
				PluginTree.Doozers[name] = doozer;
				return doozer.HandleConditions;
			}
		}
		
		public object BuildItem(object caller, Extension codon, ArrayList subItems)
		{
			IDoozer doozer = (IDoozer)plugin.CreateObject(className);
			if (doozer == null) {
				return null;
			}
			PluginTree.Doozers[name] = doozer;
			return doozer.BuildItem(caller, codon, subItems);
		}
		
		public override string ToString()
		{
			return String.Format("[LazyLoadDoozer: className = {0}, name = {1}]",
			                     className,
			                     name);
		}
		
	}
}
