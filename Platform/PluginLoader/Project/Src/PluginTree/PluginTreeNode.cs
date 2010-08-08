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
		List<Extension> codons = new List<Extension>();
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
		/// A list of child <see cref="Codon"/>s.
		/// </summary>
		public List<Extension> Codons {
			get {
				return codons;
			}
		}
		
//
//		public void BinarySerialize(BinaryWriter writer)
//		{
//			if (!isSorted) {
//				(new SortCodons(this)).Execute();
//				isSorted = true;
//			}
//			writer.Write((ushort)codons.Count);
//			foreach (Codon codon in codons) {
//				codon.BinarySerialize(writer);
//			}
//
//			writer.Write((ushort)childNodes.Count);
//			foreach (KeyValuePair<string, PluginTreeNode> child in childNodes) {
//				writer.Write(PluginTree.GetNameOffset(child.Key));
//				child.Value.BinarySerialize(writer);
//			}
//		}
		
		/// <summary>
		/// Supports sorting codons using InsertBefore/InsertAfter
		/// </summary>
		sealed class TopologicalSort
		{
			List<Extension> codons;
			bool[] visited;
			List<Extension> sortedCodons;
			Dictionary<string, int> indexOfName;
			
			public TopologicalSort(List<Extension> codons)
			{
				this.codons = codons;
				visited = new bool[codons.Count];
				sortedCodons = new List<Extension>(codons.Count);
				indexOfName = new Dictionary<string, int>(codons.Count);
				// initialize visited to false and fill the indexOfName dictionary
				for (int i = 0; i < codons.Count; ++i) {
					visited[i] = false;
					indexOfName[codons[i].Id] = i;
				}
			}
			
			void InsertEdges()
			{
				// add the InsertBefore to the corresponding InsertAfter
				for (int i = 0; i < codons.Count; ++i) {
					string before = codons[i].InsertBefore;
					if (before != null && before != "") {
						if (indexOfName.ContainsKey(before)) {
							string after = codons[indexOfName[before]].InsertAfter;
							if (after == null || after == "") {
								codons[indexOfName[before]].InsertAfter = codons[i].Id;
							} else {
								codons[indexOfName[before]].InsertAfter = after + ',' + codons[i].Id;
							}
						} else {
							LoggingService.WarnFormatted("Codon ({0}) specified in the insertbefore of the {1} codon does not exist!", before, codons[i]);
						}
					}
				}
			}
			
			public List<Extension> Execute()
			{
				InsertEdges();
				
				// Visit all codons
				for (int i = 0; i < codons.Count; ++i) {
					Visit(i);
				}
				return sortedCodons;
			}
			
			void Visit(int codonIndex)
			{
				if (visited[codonIndex]) {
					return;
				}
				string[] after = codons[codonIndex].InsertAfter.Split(new char[] {','});
				foreach (string s in after) {
					if (s == null || s.Length == 0) {
						continue;
					}
					if (indexOfName.ContainsKey(s)) {
						Visit(indexOfName[s]);
					} else {
						LoggingService.WarnFormatted("Codon ({0}) specified in the insertafter of the {1} codon does not exist!", codons[codonIndex].InsertAfter, codons[codonIndex]);
					}
				}
				sortedCodons.Add(codons[codonIndex]);
				visited[codonIndex] = true;
			}
		}
		
		/// <summary>
		/// Builds the child items in this path. Ensures that all items have the type T.
		/// </summary>
		/// <param name="caller">The owner used to create the objects.</param>
		public List<T> BuildChildItems<T>(object caller)
		{
			List<T> items = new List<T>(codons.Count);
			if (!isSorted) {
				codons = (new TopologicalSort(codons)).Execute();
				isSorted = true;
			}
			foreach (Extension codon in codons) {
				ArrayList subItems = null;
				if (childNodes.ContainsKey(codon.Id)) {
					subItems = childNodes[codon.Id].BuildChildItems(caller);
				}
				object result = codon.BuildItem(caller, subItems);
				if (result == null)
					continue;
				IBuildItemsModifier mod = result as IBuildItemsModifier;
				if (mod != null) {
					mod.Apply(items);
				} else if (result is T) {
					items.Add((T)result);
				} else {
					throw new InvalidCastException("The PluginTreeNode <" + codon.Type + " id='" + codon.Id
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
			ArrayList items = new ArrayList(codons.Count);
			if (!isSorted) {
				codons = (new TopologicalSort(codons)).Execute();
				isSorted = true;
			}
			foreach (Extension codon in codons) {
				ArrayList subItems = null;
				if (childNodes.ContainsKey(codon.Id)) {
					subItems = childNodes[codon.Id].BuildChildItems(caller);
				}
				object result = codon.BuildItem(caller, subItems);
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
			foreach (Extension codon in codons) {
				if (codon.Id == childItemID) {
					return codon.BuildItem(caller, subItems);
				}
			}
			throw new TreePathNotFoundException("The extension '" + childItemID + "' was not found at the path.");
		}
	}
}
