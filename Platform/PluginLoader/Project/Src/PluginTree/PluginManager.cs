// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 2318 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TickZoom.Loader
{
	/// <summary>
	/// Manages all actions performed on <see cref="Plugin"/>s.
	/// An PluginManager GUI can use the methods here to install/update/uninstall
	/// <see cref="Plugin"/>s.
	/// 
	/// There are three types of Plugins:
	/// - Preinstalled Plugins (added by host application) -> can only be disabled
	/// - External Plugins -> can be added, disabled and removed
	/// 	Removing external Plugins only removes the reference to the .addin file
	///     but does not delete the Plugin.
	/// - User Plugins -> are installed to UserPluginPath, can be installed, disabled
	///     and uninstalled
	/// </summary>
	public static class PluginManager
	{
		static string configurationFileName;
		static string pluginInstallTemp;
		static string userPluginPath;
		
		/// <summary>
		/// Gets or sets the user addin path.
		/// This is the path where user Plugins are installed to.
		/// This property is normally initialized by <see cref="CoreStartup.ConfigureUserPlugins"/>.
		/// </summary>
		public static string UserPluginPath {
			get {
				return userPluginPath;
			}
			set {
				userPluginPath = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the addin install temporary directory.
		/// This is a directory used to store Plugins that should be installed on
		/// the next start of the application.
		/// This property is normally initialized by <see cref="CoreStartup.ConfigureUserPlugins"/>.
		/// </summary>
		public static string PluginInstallTemp {
			get {
				return pluginInstallTemp;
			}
			set {
				pluginInstallTemp = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the full name of the configuration file.
		/// In this file, the PluginManager stores the list of disabled Plugins
		/// and the list of installed external Plugins.
		/// This property is normally initialized by <see cref="CoreStartup.ConfigureExternalPlugins"/>.
		/// </summary>
		public static string ConfigurationFileName {
			get {
				return configurationFileName;
			}
			set {
				configurationFileName = value;
			}
		}
		
		/// <summary>
		/// Installs the Plugins from PluginInstallTemp to the UserPluginPath.
		/// In case of installation errors, a error message is displayed to the user
		/// and the affected Plugin is added to the disabled list.
		/// This method is normally called by <see cref="CoreStartup.ConfigureUserPlugins"/>
		/// </summary>
		public static void InstallPlugins(List<string> disabled)
		{
			if (!Directory.Exists(pluginInstallTemp))
				return;
			LoggingService.Info("PluginManager.InstallPlugins started");
			if (!Directory.Exists(userPluginPath))
				Directory.CreateDirectory(userPluginPath);
			string removeFile = Path.Combine(pluginInstallTemp, "remove.txt");
			bool allOK = true;
			List<string> notRemoved = new List<string>();
			if (File.Exists(removeFile)) {
				using (StreamReader r = new StreamReader(removeFile)) {
					string pluginName;
					while ((pluginName = r.ReadLine()) != null) {
						pluginName = pluginName.Trim();
						if (pluginName.Length == 0)
							continue;
						string targetDir = Path.Combine(userPluginPath, pluginName);
						if (!UninstallPlugin(disabled, pluginName, targetDir)) {
							notRemoved.Add(pluginName);
							allOK = false;
						}
					}
				}
				if (notRemoved.Count == 0) {
					LoggingService.Info("Deleting remove.txt");
					File.Delete(removeFile);
				} else {
					LoggingService.Info("Rewriting remove.txt");
					using (StreamWriter w = new StreamWriter(removeFile)) {
						notRemoved.ForEach(w.WriteLine);
					}
				}
			}
			foreach (string sourceDir in Directory.GetDirectories(pluginInstallTemp)) {
				string pluginName = Path.GetFileName(sourceDir);
				string targetDir = Path.Combine(userPluginPath, pluginName);
				if (notRemoved.Contains(pluginName)) {
					LoggingService.Info("Skipping installation of " + pluginName + " because deinstallation failed.");
					continue;
				}
				if (UninstallPlugin(disabled, pluginName, targetDir)) {
					LoggingService.Info("Installing " + pluginName + "...");
					Directory.Move(sourceDir, targetDir);
				} else {
					allOK = false;
				}
			}
			if (allOK) {
				try {
					Directory.Delete(pluginInstallTemp, false);
				} catch (Exception ex) {
					LoggingService.Warn("Error removing install temp", ex);
				}
			}
			LoggingService.Info("PluginManager.InstallPlugins finished");
		}
		
		static bool UninstallPlugin(List<string> disabled, string pluginName, string targetDir)
		{
			if (Directory.Exists(targetDir)) {
				LoggingService.Info("Removing " + pluginName + "...");
				try {
					Directory.Delete(targetDir, true);
				} catch (Exception ex) {
					disabled.Add(pluginName);
					MessageService.ShowError("Error removing " + pluginName + ":\n" +
					                         ex.Message + "\nThe Plugin will be " +
					                         "removed on the next start of " + MessageService.ProductName +
					                         " and is disabled for now.");
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Uninstalls the user addin on next start.
		/// <see cref="RemoveUserPluginOnNextStart"/> schedules the Plugin for
		/// deinstallation, you can unschedule it using
		/// <see cref="AbortRemoveUserPluginOnNextStart"/>
		/// </summary>
		/// <param name="identity">The identity of the addin to remove.</param>
		public static void RemoveUserPluginOnNextStart(string identity)
		{
			List<string> removeEntries = new List<string>();
			string removeFile = Path.Combine(pluginInstallTemp, "remove.txt");
			if (File.Exists(removeFile)) {
				using (StreamReader r = new StreamReader(removeFile)) {
					string pluginName;
					while ((pluginName = r.ReadLine()) != null) {
						pluginName = pluginName.Trim();
						if (pluginName.Length > 0)
							removeEntries.Add(pluginName);
					}
				}
				if (removeEntries.Contains(identity))
					return;
			}
			removeEntries.Add(identity);
			if (!Directory.Exists(pluginInstallTemp))
				Directory.CreateDirectory(pluginInstallTemp);
			using (StreamWriter w = new StreamWriter(removeFile)) {
				removeEntries.ForEach(w.WriteLine);
			}
		}
		
		/// <summary>
		/// Prevents a user Plugin from being uninstalled.
		/// <see cref="RemoveUserPluginOnNextStart"/> schedules the Plugin for
		/// deinstallation, you can unschedule it using
		/// <see cref="AbortRemoveUserPluginOnNextStart"/>
		/// </summary>
		/// <param name="identity">The identity of which to abort the removal.</param>
		public static void AbortRemoveUserPluginOnNextStart(string identity)
		{
			string removeFile = Path.Combine(pluginInstallTemp, "remove.txt");
			if (!File.Exists(removeFile)) {
				return;
			}
			List<string> removeEntries = new List<string>();
			using (StreamReader r = new StreamReader(removeFile)) {
				string pluginName;
				while ((pluginName = r.ReadLine()) != null) {
					pluginName = pluginName.Trim();
					if (pluginName.Length > 0)
						removeEntries.Add(pluginName);
				}
			}
			if (removeEntries.Remove(identity)) {
				using (StreamWriter w = new StreamWriter(removeFile)) {
					removeEntries.ForEach(w.WriteLine);
				}
			}
		}
		
		/// <summary>
		/// Adds the specified external Plugins to the list of registered external
		/// Plugins.
		/// </summary>
		/// <param name="plugins">
		/// The list of Plugins to add. (use <see cref="Plugin"/> instances
		/// created by <see cref="Plugin.Load(TextReader)"/>).
		/// </param>
		public static void AddExternalPlugins(IList<Plugin> plugins)
		{
			List<string> pluginFiles = new List<string>();
			List<string> disabled = new List<string>();
			LoadPluginConfiguration(pluginFiles, disabled);
			
			foreach (Plugin plugin in plugins) {
				if (!pluginFiles.Contains(plugin.FileName))
					pluginFiles.Add(plugin.FileName);
				plugin.Enabled = false;
				plugin.Action = PluginAction.Install;
				PluginTree.InsertPlugin(plugin);
			}
			
			SavePluginConfiguration(pluginFiles, disabled);
		}
		
		/// <summary>
		/// Removes the specified external Plugins from the list of registered external
		/// Plugins.
		/// </summary>
		/// The list of Plugins to remove.
		/// (use external Plugins from the <see cref="PluginTree.Plugins"/> collection).
		public static void RemoveExternalPlugins(IList<Plugin> plugins)
		{
			List<string> pluginFiles = new List<string>();
			List<string> disabled = new List<string>();
			LoadPluginConfiguration(pluginFiles, disabled);
			
			foreach (Plugin plugin in plugins) {
				foreach (string identity in plugin.Manifest.Identities.Keys) {
					disabled.Remove(identity);
				}
				pluginFiles.Remove(plugin.FileName);
				plugin.Action = PluginAction.Uninstall;
				if (!plugin.Enabled) {
					PluginTree.RemovePlugin(plugin);
				}
			}
			
			SavePluginConfiguration(pluginFiles, disabled);
		}
		
		/// <summary>
		/// Marks the specified Plugins as enabled (will take effect after
		/// next application restart).
		/// </summary>
		public static void Enable(IList<Plugin> plugins)
		{
			List<string> pluginFiles = new List<string>();
			List<string> disabled = new List<string>();
			LoadPluginConfiguration(pluginFiles, disabled);
			
			foreach (Plugin plugin in plugins) {
				foreach (string identity in plugin.Manifest.Identities.Keys) {
					disabled.Remove(identity);
				}
				if (plugin.Action == PluginAction.Uninstall) {
					if (FileUtility.IsBaseDirectory(userPluginPath, plugin.FileName)) {
						foreach (string identity in plugin.Manifest.Identities.Keys) {
							AbortRemoveUserPluginOnNextStart(identity);
						}
					} else {
						if (!pluginFiles.Contains(plugin.FileName))
							pluginFiles.Add(plugin.FileName);
					}
				}
				plugin.Action = PluginAction.Enable;
			}
			
			SavePluginConfiguration(pluginFiles, disabled);
		}
		
		/// <summary>
		/// Marks the specified Plugins as disabled (will take effect after
		/// next application restart).
		/// </summary>
		public static void Disable(IList<Plugin> plugins)
		{
			List<string> pluginFiles = new List<string>();
			List<string> disabled = new List<string>();
			LoadPluginConfiguration(pluginFiles, disabled);
			
			foreach (Plugin plugin in plugins) {
				string identity = plugin.Manifest.PrimaryIdentity;
				if (identity == null)
					throw new ArgumentException("The Plugin cannot be disabled because it has no identity.");
				
				if (!disabled.Contains(identity))
					disabled.Add(identity);
				plugin.Action = PluginAction.Disable;
			}
			
			SavePluginConfiguration(pluginFiles, disabled);
		}
		
		/// <summary>
		/// Loads a configuration file.
		/// The 'file' from XML elements in the form "&lt;Plugin file='full path to .addin file'&gt;" will
		/// be added to <paramref name="pluginFiles"/>, the 'addin' element from
		/// "&lt;Disable addin='addin identity'&gt;" will be added to <paramref name="disabledPlugins"/>,
		/// all other XML elements are ignored.
		/// </summary>
		/// <param name="pluginFiles">File names of external Plugins are added to this collection.</param>
		/// <param name="disabledPlugins">Identities of disabled addins are added to this collection.</param>
		public static void LoadPluginConfiguration(List<string> pluginFiles, List<string> disabledPlugins)
		{
			if (!File.Exists(configurationFileName))
				return;
			using (XmlTextReader reader = new XmlTextReader(configurationFileName)) {
				while (reader.Read()) {
					if (reader.NodeType == XmlNodeType.Element) {
						if (reader.Name == "Plugin") {
							string fileName = reader.GetAttribute("file");
							if (fileName != null && fileName.Length > 0) {
								pluginFiles.Add(fileName);
							}
						} else if (reader.Name == "Disable") {
							string plugin = reader.GetAttribute("addin");
							if (plugin != null && plugin.Length > 0) {
								disabledPlugins.Add(plugin);
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Saves the Plugin configuration in the format expected by
		/// <see cref="LoadPluginConfiguration"/>.
		/// </summary>
		/// <param name="pluginFiles">List of file names of external Plugins.</param>
		/// <param name="disabledPlugins">List of Identities of disabled addins.</param>
		public static void SavePluginConfiguration(List<string> pluginFiles, List<string> disabledPlugins)
		{
			using (XmlTextWriter writer = new XmlTextWriter(configurationFileName, Encoding.UTF8)) {
				writer.Formatting = Formatting.Indented;
				writer.WriteStartDocument();
				writer.WriteStartElement("PluginConfiguration");
				foreach (string file in pluginFiles) {
					writer.WriteStartElement("Plugin");
					writer.WriteAttributeString("file", file);
					writer.WriteEndElement();
				}
				foreach (string name in disabledPlugins) {
					writer.WriteStartElement("Disable");
					writer.WriteAttributeString("addin", name);
					writer.WriteEndElement();
				}
				writer.WriteEndDocument();
			}
		}
	}
}
