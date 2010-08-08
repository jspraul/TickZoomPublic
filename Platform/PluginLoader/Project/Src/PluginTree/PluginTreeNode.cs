// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 3287 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;

namespace TickZoom.Loader
{
	/// <summary>
	/// Represents an extension path in the <see cref="PluginTree"/>.
	/// </summary>
	public sealed class PluginTreeNode
	{
		Dictionary<string, PluginTreeNode> childNodes = new Dictionary<string, PluginTreeNode>();
		List<Extension> extensions = new List<Extension>();
		bool isSorted = false;
		
		/// <summary>
		/// A dictionary containing the child paths.
		/// </summary>
		public Dictionary<string, PluginTreeNode> ChildNodes {
			get {
				return childNodes;
			}
		}
		
		/// <summary>
		/// A list of child <see cref="Extension"/>s.
		/// </summary>
		public List<Extension> Extensions {
			get {
				return extensions;
			}
		}
		
//
//		public void BinarySerialize(BinaryWriter writer)
//		{
//			if (!isSorted) {
//				(new SortExtensions(this)).Execute();
//				isSorted = true;
//			}
//			writer.Write((ushort)extensions.Count);
//			foreach (Extension extension in extensions) {
//				extension.BinarySerialize(writer);
//			}
//
//			writer.Write((ushort)childNodes.Count);
//			foreach (KeyValuePair<string, PluginTreeNode> child in childNodes) {
//				writer.Write(PluginTree.GetNameOffset(child.Key));
//				child.Value.BinarySerialize(writer);
//			}
//		}
		
		/// <summary>
		/// Supports sorting extensions using InsertBefore/InsertAfter
		/// </summary>
		sealed class TopologicalSort
		{
			List<Extension> extensions;
			bool[] visited;
			List<Extension> sortedExtensions;
			Dictionary<string, int> indexOfName;
			
			public TopologicalSort(List<Extension> extensions)
			{
				this.extensions = extensions;
				visited = new bool[extensions.Count];
				sortedExtensions = new List<Extension>(extensions.Count);
				indexOfName = new Dictionary<string, int>(extensions.Count);
				// initialize visited to false and fill the indexOfName dictionary
				for (int i = 0; i < extensions.Count; ++i) {
					visited[i] = false;
					indexOfName[extensions[i].Id] = i;
				}
			}
			
			void InsertEdges()
			{
				// add the InsertBefore to the corresponding InsertAfter
				for (int i = 0; i < extensions.Count; ++i) {
					string before = extensions[i].InsertBefore;
					if (before != null && before != "") {
						if (indexOfName.ContainsKey(before)) {
							string after = extensions[indexOfName[before]].InsertAfter;
							if (after == null || after == "") {
								extensions[indexOfName[before]].InsertAfter = extensions[i].Id;
							} else {
								extensions[indexOfName[before]].InsertAfter = after + ',' + extensions[i].Id;
							}
						} else {
							LoggingService.WarnFormatted("Extension ({0}) specified in the insertbefore of the {1} extension does not exist!", before, extensions[i]);
						}
					}
				}
			}
			
			public List<Extension> Execute()
			{
				InsertEdges();
				
				// Visit all extensions
				for (int i = 0; i < extensions.Count; ++i) {
					Visit(i);
				}
				return sortedExtensions;
			}
			
			void Visit(int extensionIndex)
			{
				if (visited[extensionIndex]) {
					return;
				}
				string[] after = extensions[extensionIndex].InsertAfter.Split(new char[] {','});
				foreach (string s in after) {
					if (s == null || s.Length == 0) {
						continue;
					}
					if (indexOfName.ContainsKey(s)) {
						Visit(indexOfName[s]);
					} else {
						LoggingService.WarnFormatted("Extension ({0}) specified in the insertafter of the {1} extension does not exist!", extensions[extensionIndex].InsertAfter, extensions[extensionIndex]);
					}
				}
				sortedExtensions.Add(extensions[extensionIndex]);
				visited[extensionIndex] = true;
			}
		}
		
		/// <summary>
		/// Builds the child items in this path. Ensures that all items have the type T.
		/// </summary>
		/// <param name="caller">The owner used to create the objects.</param>
		public List<T> BuildChildItems<T>(object caller)
		{
			List<T> items = new List<T>(extensions.Count);
			if (!isSorted) {
				extensions = (new TopologicalSort(extensions)).Execute();
				isSorted = true;
			}
			foreach (Extension extension in extensions) {
				ArrayList subItems = null;
				if (childNodes.ContainsKey(extension.Id)) {
					subItems = childNodes[extension.Id].BuildChildItems(caller);
				}
				object result = extension.BuildItem(caller, subItems);
				if (result == null)
					continue;
				IBuildItemsModifier mod = result as IBuildItemsModifier;
				if (mod != null) {
					mod.Apply(items);
				} else if (result is T) {
					items.Add((T)result);
				} else {
					throw new InvalidCastException("The PluginTreeNode <" + extension.Type + " id='" + extension.Id
					                               + "' returned an instance of " + result.GetType().FullName
					                               + " but the type " + typeof(T).FullName + " is expected.");
				}
			}
			return items;
		}
		
		/// <summary>
		/// Builds the child items in this path.
		/// </summary>
		/// <param name="caller">The owner used to create the objects.</param>
		public ArrayList BuildChildItems(object caller)
		{
			ArrayList items = new ArrayList(extensions.Count);
			if (!isSorted) {
				extensions = (new TopologicalSort(extensions)).Execute();
				isSorted = true;
			}
			foreach (Extension extension in extensions) {
				ArrayList subItems = null;
				if (childNodes.ContainsKey(extension.Id)) {
					subItems = childNodes[extension.Id].BuildChildItems(caller);
				}
				object result = extension.BuildItem(caller, subItems);
				if (result == null)
					continue;
				IBuildItemsModifier mod = result as IBuildItemsModifier;
				if (mod != null) {
					mod.Apply(items);
				} else {
					items.Add(result);
				}
			}
			return items;
		}
		
		/// <summary>
		/// Builds a specific child items in this path.
		/// </summary>
		/// <param name="childItemID">
		/// The ID of the child item to build.
		/// </param>
		/// <param name="caller">The owner used to create the objects.</param>
		/// <param name="subItems">The subitems to pass to the doozer</param>
		/// <exception cref="TreePathNotFoundException">
		/// Occurs when <paramref name="childItemID"/> does not exist in this path.
		/// </exception>
		public object BuildChildItem(string childItemID, object caller, ArrayList subItems)
		{
			foreach (Extension extension in extensions) {
				if (extension.Id == childItemID) {
					return extension.BuildItem(caller, subItems);
				}
			}
			throw new TreePathNotFoundException("The extension '" + childItemID + "' was not found at the path.");
		}
	}
}
