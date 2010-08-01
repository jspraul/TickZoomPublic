// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;

namespace TickZoom.Loader
{
	/// <summary>
	/// Condition evaluator that lazy-loads another condition evaluator and executes it.
	/// </summary>
	public class LazyConditionEvaluator : IConditionEvaluator
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
		
		public LazyConditionEvaluator(Plugin plugin, Properties properties)
		{
			this.plugin      = plugin;
			this.name       = properties["name"];
			this.className  = properties["class"];
		}
		
		public bool IsValid(object caller, Condition condition)
		{
			IConditionEvaluator evaluator = (IConditionEvaluator)plugin.CreateObject(className);
			if (evaluator == null) {
				return false;
			}
			PluginTree.ConditionEvaluators[name] = evaluator;
			return evaluator.IsValid(caller, condition);
		}
		
		public override string ToString()
		{
			return String.Format("[LazyLoadConditionEvaluator: className = {0}, name = {1}]",
			                     className,
			                     name);
		}
		
	}
}
