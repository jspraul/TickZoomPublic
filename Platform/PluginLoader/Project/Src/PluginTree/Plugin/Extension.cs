// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 2059 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;

namespace TickZoom.Loader
{
	/// <summary>
	/// Represents a node in the add in tree that can produce an item.
	/// </summary>
	public class Extension
	{
		Plugin       plugin;
		string      type;
		Properties  properties;
		ICondition[] conditions;
		
		public string Type {
			get {
				return type;
			}
		}
		
		public Plugin Plugin {
			get {
				return plugin;
			}
		}
		
		public string Id {
			get {
				return properties["id"];
			}
		}
		
		public string InsertAfter {
			get {
				if (!properties.Contains("insertafter")) {
					return "";
				}
				return properties["insertafter"];
			}
			set {
				properties["insertafter"] = value;
			}
		}
		
		public string InsertBefore {
			get {
				if (!properties.Contains("insertbefore")) {
					return "";
				}
				return properties["insertbefore"];
			}
			set {
				properties["insertbefore"] = value;
			}
		}
		
		public string this[string key] {
			get {
				return properties[key];
			}
		}
		
		public Properties Properties {
			get {
				return properties;
			}
		}
		
		public ICondition[] Conditions {
			get {
				return conditions;
			}
		}
		
		public Extension(Plugin plugin, string type, Properties properties, ICondition[] conditions)
		{
			this.plugin      = plugin;
			this.type       = type;
			this.properties = properties;
			this.conditions = conditions;
		}
		
		public ConditionFailedAction GetFailedAction(object caller)
		{
			return Condition.GetFailedAction(conditions, caller);
		}
		
//
//		public void BinarySerialize(BinaryWriter writer)
//		{
//			writer.Write(PluginTree.GetNameOffset(name));
//			writer.Write(PluginTree.GetPluginOffset(plugin));
//			properties.BinarySerialize(writer);
//		}
//
		public object BuildItem(object owner, ArrayList subItems)
		{
			IDoozer doozer;
			if (!PluginTree.Doozers.TryGetValue(Type, out doozer))
				throw new CoreException("Doozer " + Type + " not found!");
			
			if (!doozer.HandleConditions && conditions.Length > 0) {
				ConditionFailedAction action = GetFailedAction(owner);
				if (action != ConditionFailedAction.Nothing) {
					return null;
				}
			}
			return doozer.BuildItem(owner, this, subItems);
		}
		
		public override string ToString()
		{
			return String.Format("[Extension: name = {0}, plugin={1}]",
			                     type,
			                     plugin.FileName);
		}
	}
}
