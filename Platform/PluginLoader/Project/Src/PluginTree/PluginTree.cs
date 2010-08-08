// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 4498 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace TickZoom.Loader
{
	/// <summary>
	/// Static class containing the PluginTree. Contains methods for accessing tree nodes and building items.
	/// </summary>
	public static class PluginTree
	{
		static List<Plugin>   plugins   = new List<Plugin>();
		static PluginTreeNode rootNode = new PluginTreeNode();
		
		static Dictionary<string, IDoozer> doozers = new Dictionary<string, IDoozer>();
		static Dictionary<string, IConditionEvaluator> conditionEvaluators = new Dictionary<string, IConditionEvaluator>();
		
		static PluginTree()
		{
			doozers.Add("Class", new ClassDoozer());
			doozers.Add("FileFilter", new FileFilterDoozer());
			doozers.Add("String", new StringDoozer());
			doozers.Add("Icon", new IconDoozer());
			doozers.Add("MenuItem", new MenuItemDoozer());
			doozers.Add("ToolbarItem", new ToolbarItemDoozer());
			doozers.Add("Include", new IncludeDoozer());
			
			conditionEvaluators.Add("Compare", new CompareConditionEvaluator());
			conditionEvaluators.Add("Ownerstate", new OwnerStateConditionEvaluator());
			
			ApplicationStateInfoService.RegisterStateGetter("Installed 3rd party Plugins", GetInstalledThirdPartyPluginsListAsString);
		}
		
		static object GetInstalledThirdPartyPluginsListAsString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (Plugin plugin in Plugins) {
				// Skip preinstalled Plugins (show only third party Plugins)
				if (FileUtility.IsBaseDirectory(FileUtility.ApplicationRootPath, plugin.FileName)) {
					string hidden = plugin.Properties["pluginManagerHidden"];
					if (string.Equals(hidden, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(hidden, "preinstalled", StringComparison.OrdinalIgnoreCase))
						continue;
				}
				if (sb.Length > 0) sb.Append(", ");
				sb.Append("[");
				sb.Append(plugin.Name);
				if (plugin.Version != null) {
					sb.Append(' ');
					sb.Append(plugin.Version.ToString());
				}
				if (!plugin.Enabled) {
					sb.Append(", Enabled=");
					sb.Append(plugin.Enabled);
				}
				if (plugin.Action != PluginAction.Enable) {
					sb.Append(", Action=");
					sb.Append(plugin.Action.ToString());
				}
				sb.Append("]");
			}
			return sb.ToString();
		}
		
		/// <summary>
		/// Gets the list of loaded Plugins.
		/// </summary>
		public static IList<Plugin> Plugins {
			get {
				return plugins.AsReadOnly();
			}
		}
		
		/// <summary>
		/// Gets a dictionary of registered doozers.
		/// </summary>
		public static Dictionary<string, IDoozer> Doozers {
			get {
				return doozers;
			}
		}
		
		/// <summary>
		/// Gets a dictionary of registered condition evaluators.
		/// </summary>
		public static Dictionary<string, IConditionEvaluator> ConditionEvaluators {
			get {
				return conditionEvaluators;
			}
		}
		
		/// <summary>
		/// Checks whether the specified path exists in the Plugin tree.
		/// </summary>
		public static bool ExistsTreeNode(string path)
		{
			if (path == null || path.Length == 0) {
				return true;
			}
			
			string[] splittedPath = path.Split('/');
			PluginTreeNode curPath = rootNode;
			int i = 0;
			while (i < splittedPath.Length) {
				// curPath = curPath.ChildNodes[splittedPath[i]] - check if child path exists
				if (!curPath.ChildNodes.TryGetValue(splittedPath[i], out curPath)) {
					return false;
				}
				++i;
			}
			return true;
		}
		
		/// <summary>
		/// Gets the <see cref="PluginTreeNode"/> representing the specified path.
		/// This method throws a <see cref="TreePathNotFoundException"/> when the
		/// path does not exist.
		/// </summary>
		public static PluginTreeNode GetTreeNode(string path)
		{
			return GetTreeNode(path, true);
		}
		
		/// <summary>
		/// Gets the <see cref="PluginTreeNode"/> representing the specified path.
		/// </summary>
		/// <param name="path">The path of the Plugin tree node</param>
		/// <param name="throwOnNotFound">
		/// If set to <c>true</c>, this method throws a
		/// <see cref="TreePathNotFoundException"/> when the path does not exist.
		/// If set to <c>false</c>, <c>null</c> is returned for non-existing paths.
		/// </param>
		public static PluginTreeNode GetTreeNode(string path, bool throwOnNotFound)
		{
			if (path == null || path.Length == 0) {
				return rootNode;
			}
			string[] splittedPath = path.Split('/');
			PluginTreeNode curPath = rootNode;
			int i = 0;
			while (i < splittedPath.Length) {
				if (!curPath.ChildNodes.TryGetValue(splittedPath[i], out curPath)) {
					if (throwOnNotFound)
						throw new TreePathNotFoundException(path);
					else
						return null;
				}
				// curPath = curPath.ChildNodes[splittedPath[i]]; already done by TryGetValue
				++i;
			}
			return curPath;
		}
		
		/// <summary>
		/// Builds a single item in the addin tree.
		/// </summary>
		/// <param name="path">A path to the item in the addin tree.</param>
		/// <param name="caller">The owner used to create the objects.</param>
		/// <exception cref="TreePathNotFoundException">The path does not
		/// exist or does not point to an item.</exception>
		public static object BuildItem(string path, object caller)
		{
			int pos = path.LastIndexOf('/');
			string parent = path.Substring(0, pos);
			string child = path.Substring(pos + 1);
			PluginTreeNode node = GetTreeNode(parent);
			return node.BuildChildItem(child, caller, new ArrayList(BuildItems<object>(parent, caller, false)));
		}
		
		/// <summary>
		/// Builds the items in the path. Ensures that all items have the type T.
		/// Throws a <see cref="TreePathNotFoundException"/> if the path is not found.
		/// </summary>
		/// <param name="path">A path in the addin tree.</param>
		/// <param name="caller">The owner used to create the objects.</param>
		public static List<T> BuildItems<T>(string path, object caller)
		{
			return BuildItems<T>(path, caller, true);
		}
		
		/// <summary>
		/// Builds the items in the path. Ensures that all items have the type T.
		/// </summary>
		/// <param name="path">A path in the addin tree.</param>
		/// <param name="caller">The owner used to create the objects.</param>
		/// <param name="throwOnNotFound">If true, throws a <see cref="TreePathNotFoundException"/>
		/// if the path is not found. If false, an empty ArrayList is returned when the
		/// path is not found.</param>
		public static List<T> BuildItems<T>(string path, object caller, bool throwOnNotFound)
		{
			PluginTreeNode node = GetTreeNode(path, throwOnNotFound);
			if (node == null)
				return new List<T>();
			else
				return node.BuildChildItems<T>(caller);
		}
		
		static PluginTreeNode CreatePath(PluginTreeNode localRoot, string path)
		{
			if (path == null || path.Length == 0) {
				return localRoot;
			}
			string[] splittedPath = path.Split('/');
			PluginTreeNode curPath = localRoot;
			int i = 0;
			while (i < splittedPath.Length) {
				if (!curPath.ChildNodes.ContainsKey(splittedPath[i])) {
					curPath.ChildNodes[splittedPath[i]] = new PluginTreeNode();
				}
				curPath = curPath.ChildNodes[splittedPath[i]];
				++i;
			}
			
			return curPath;
		}
		
		static void AddExtensionPath(ExtensionPath path)
		{
			PluginTreeNode treePath = CreatePath(rootNode, path.Name);
			foreach (Extension codon in path.Codons) {
				treePath.Codons.Add(codon);
			}
		}
		
		/// <summary>
		/// The specified Plugin is added to the <see cref="Plugins"/> collection.
		/// If the Plugin is enabled, its doozers, condition evaluators and extension
		/// paths are added to the PluginTree and its resources are added to the
		/// <see cref="ResourceService"/>.
		/// </summary>
		public static void InsertPlugin(Plugin plugin)
		{
			if (plugin.Enabled) {
				foreach (ExtensionPath path in plugin.Paths.Values) {
					AddExtensionPath(path);
				}
				
				foreach (Runtime runtime in plugin.Runtimes) {
					if (runtime.IsActive) {
						foreach (LazyLoadDoozer doozer in runtime.DefinedDoozers) {
							if (PluginTree.Doozers.ContainsKey(doozer.Name)) {
								throw new PluginLoadException("Duplicate doozer: " + doozer.Name);
							}
							PluginTree.Doozers.Add(doozer.Name, doozer);
						}
						foreach (LazyConditionEvaluator condition in runtime.DefinedConditionEvaluators) {
							if (PluginTree.ConditionEvaluators.ContainsKey(condition.Name)) {
								throw new PluginLoadException("Duplicate condition evaluator: " + condition.Name);
							}
							PluginTree.ConditionEvaluators.Add(condition.Name, condition);
						}
					}
				}
				
				string pluginRoot = Path.GetDirectoryName(plugin.FileName);
				foreach(string bitmapResource in plugin.BitmapResources)
				{
					string path = Path.Combine(pluginRoot, bitmapResource);
					ResourceManager resourceManager = ResourceManager.CreateFileBasedResourceManager(Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path), null);
					ResourceService.RegisterNeutralImages(resourceManager);
				}
				
				foreach(string stringResource in plugin.StringResources)
				{
					string path = Path.Combine(pluginRoot, stringResource);
					ResourceManager resourceManager = ResourceManager.CreateFileBasedResourceManager(Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path), null);
					ResourceService.RegisterNeutralStrings(resourceManager);
				}
			}
			plugins.Add(plugin);
		}
		
		/// <summary>
		/// The specified Plugin is removed to the <see cref="Plugins"/> collection.
		/// This is only possible for disabled Plugins, enabled Plugins require
		/// a restart of the application to be removed.
		/// </summary>
		/// <exception cref="ArgumentException">Occurs when trying to remove an enabled Plugin.</exception>
		public static void RemovePlugin(Plugin plugin)
		{
			if (plugin.Enabled) {
				throw new ArgumentException("Cannot remove enabled Plugins at runtime.");
			}
			plugins.Remove(plugin);
		}
		
		// As long as the show form takes 10 times of loading the xml representation I'm not implementing
		// binary serialization.
//		static Dictionary<string, ushort> nameLookupTable = new Dictionary<string, ushort>();
//		static Dictionary<Plugin, ushort> pluginLookupTable = new Dictionary<Plugin, ushort>();
//
//		public static ushort GetPluginOffset(Plugin plugin)
//		{
//			return pluginLookupTable[plugin];
//		}
//
//		public static ushort GetNameOffset(string name)
//		{
//			if (!nameLookupTable.ContainsKey(name)) {
//				nameLookupTable[name] = (ushort)nameLookupTable.Count;
//			}
//			return nameLookupTable[name];
//		}
//
//		public static void BinarySerialize(string fileName)
//		{
//			for (int i = 0; i < plugins.Count; ++i) {
//				pluginLookupTable[plugins] = (ushort)i;
//			}
//			using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(fileName))) {
//				rootNode.BinarySerialize(writer);
//				writer.Write((ushort)plugins.Count);
//				for (int i = 0; i < plugins.Count; ++i) {
//					plugins[i].BinarySerialize(writer);
//				}
//				writer.Write((ushort)nameLookupTable.Count);
//				foreach (string name in nameLookupTable.Keys) {
//					writer.Write(name);
//				}
//			}
//		}
		
		// used by Load(): disables an addin and removes it from the dictionaries.
		static void DisableAddin(Plugin plugin, Dictionary<string, Version> dict, Dictionary<string, Plugin> pluginDict)
		{
			plugin.Enabled = false;
			plugin.Action = PluginAction.DependencyError;
			foreach (string name in plugin.Manifest.Identities.Keys) {
				dict.Remove(name);
				pluginDict.Remove(name);
			}
		}
		
		/// <summary>
		/// Loads a list of .addin files, ensuring that dependencies are satisfied.
		/// This method is normally called by <see cref="CoreStartup.RunInitialization"/>.
		/// </summary>
		/// <param name="pluginFiles">
		/// The list of .addin file names to load.
		/// </param>
		/// <param name="disabledPlugins">
		/// The list of disabled Plugin identity names.
		/// </param>
		public static void Load(List<string> pluginFiles, List<string> disabledPlugins)
		{
			List<Plugin> list = new List<Plugin>();
			Dictionary<string, Version> dict = new Dictionary<string, Version>();
			Dictionary<string, Plugin> pluginDict = new Dictionary<string, Plugin>();
			foreach (string fileName in pluginFiles) {
				Plugin plugin;
				try {
					plugin = Plugin.Load(fileName);
				} catch (PluginLoadException ex) {
					LoggingService.Error(ex);
					if (ex.InnerException != null) {
						MessageService.ShowError("Error loading Plugin " + fileName + ":\n"
						                         + ex.InnerException.Message);
					} else {
						MessageService.ShowError("Error loading Plugin " + fileName + ":\n"
						                         + ex.Message);
					}
					plugin = new Plugin();
					plugin.pluginFileName = fileName;
					plugin.CustomErrorMessage = ex.Message;
				}
				if (plugin.Action == PluginAction.CustomError) {
					list.Add(plugin);
					continue;
				}
				plugin.Enabled = true;
				if (disabledPlugins != null && disabledPlugins.Count > 0) {
					foreach (string name in plugin.Manifest.Identities.Keys) {
						if (disabledPlugins.Contains(name)) {
							plugin.Enabled = false;
							break;
						}
					}
				}
				if (plugin.Enabled) {
					foreach (KeyValuePair<string, Version> pair in plugin.Manifest.Identities) {
						if (dict.ContainsKey(pair.Key)) {
							MessageService.ShowError("Name '" + pair.Key + "' is used by " +
							                         "'" + pluginDict[pair.Key].FileName + "' and '" + fileName + "'");
							plugin.Enabled = false;
							plugin.Action = PluginAction.InstalledTwice;
							break;
						} else {
							dict.Add(pair.Key, pair.Value);
							pluginDict.Add(pair.Key, plugin);
						}
					}
				}
				list.Add(plugin);
			}
		checkDependencies:
			for (int i = 0; i < list.Count; i++) {
				Plugin plugin = list[i];
				if (!plugin.Enabled) continue;
				
				Version versionFound;
				
				foreach (PluginReference reference in plugin.Manifest.Conflicts) {
					if (reference.Check(dict, out versionFound)) {
						MessageService.ShowError(plugin.Name + " conflicts with " + reference.ToString()
						                         + " and has been disabled.");
						DisableAddin(plugin, dict, pluginDict);
						goto checkDependencies; // after removing one addin, others could break
					}
				}
				foreach (PluginReference reference in plugin.Manifest.Dependencies) {
					if (!reference.Check(dict, out versionFound)) {
						if (versionFound != null) {
							MessageService.ShowError(plugin.Name + " has not been loaded because it requires "
							                         + reference.ToString() + ", but version "
							                         + versionFound.ToString() + " is installed.");
						} else {
							MessageService.ShowError(plugin.Name + " has not been loaded because it requires "
							                         + reference.ToString() + ".");
						}
						DisableAddin(plugin, dict, pluginDict);
						goto checkDependencies; // after removing one addin, others could break
					}
				}
			}
			foreach (Plugin plugin in list) {
				try {
					InsertPlugin(plugin);
				} catch (PluginLoadException ex) {
					LoggingService.Error(ex);
					MessageService.ShowError("Error loading Plugin " + plugin.FileName + ":\n"
					                         + ex.Message);
				}
			}
		}
	}
}
