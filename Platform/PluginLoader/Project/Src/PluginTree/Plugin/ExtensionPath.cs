// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Xml;

namespace TickZoom.Loader
{
	/// <summary>
	/// Description of Path.
	/// </summary>
	public class ExtensionPath
	{
		string      name;
		Plugin       plugin;
		List<Extension> codons = new List<Extension>();
		
		public Plugin Plugin {
			get {
				return plugin;
			}
		}
		
		public string Name {
			get {
				return name;
			}
		}
		public List<Extension> Codons {
			get {
				return codons;
			}
		}
		
		public ExtensionPath(string name, Plugin plugin)
		{
			this.plugin = plugin;
			this.name = name;
		}
		
		public static void SetUp(ExtensionPath extensionPath, XmlReader reader, string endElement)
		{
			Stack<ICondition> conditionStack = new Stack<ICondition>();
			while (reader.Read()) {
				switch (reader.NodeType) {
					case XmlNodeType.EndElement:
						if (reader.LocalName == "Condition" || reader.LocalName == "ComplexCondition") {
							conditionStack.Pop();
						} else if (reader.LocalName == endElement) {
							return;
						}
						break;
					case XmlNodeType.Element:
						string elementName = reader.LocalName;
						if (elementName == "Condition") {
							conditionStack.Push(Condition.Read(reader));
						} else if (elementName == "ComplexCondition") {
							conditionStack.Push(Condition.ReadComplexCondition(reader));
						} else {
							Properties properties = Properties.ReadFromAttributes(reader);
							string extensionType = "Class";
							switch( elementName) {
								case "Extension":
									string type = properties["type"];
									if( !string.IsNullOrEmpty(type)) {
										extensionType = type;
									}
									break;
								case "Sub":
								case "Simple":
									extensionType = elementName;
									break;
								default:
									throw new ApplicationException("Unknown element '" + elementName + "'");
							}
							Extension newCodon = new Extension(extensionPath.Plugin, extensionType, properties, conditionStack.ToArray());
							extensionPath.codons.Add(newCodon);
							if (!reader.IsEmptyElement) {
								ExtensionPath subPath = extensionPath.Plugin.GetExtensionPath(extensionPath.Name + "/" + newCodon.Id);
								//foreach (ICondition condition in extensionPath.conditionStack) {
								//	subPath.conditionStack.Push(condition);
								//}
								SetUp(subPath, reader, elementName);
								//foreach (ICondition condition in extensionPath.conditionStack) {
								//	subPath.conditionStack.Pop();
								//}
							}
						}
						break;
				}
			}
		}
	}
}
