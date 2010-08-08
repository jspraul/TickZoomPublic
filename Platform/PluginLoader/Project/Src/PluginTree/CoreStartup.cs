// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 3681 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;

namespace TickZoom.Loader
{
	/// <summary>
	/// Class that helps starting up TickZoom.Loader.
	/// </summary>
	/// <remarks>
	/// Initializing TickZoom.Loader requires initializing several static classes
	/// and the <see cref="PluginTree"/>. <see cref="CoreStartup"/> does this work
	/// for you, provided you use it like this:
	/// 1. Create a new CoreStartup instance
	/// 2. (Optional) Set the values of the properties.
	/// 3. Call <see cref="StartCoreServices()"/>.
	/// 4. Add "preinstalled" Plugins using <see cref="AddPluginsFromDirectory"/>
	///    and <see cref="AddPluginFile"/>.
	/// 5. (Optional) Call <see cref="ConfigureExternalPlugins"/> to support
	///    disabling Plugins and installing external Plugins
	/// 6. (Optional) Call <see cref="ConfigureUserPlugins"/> to support installing
	///    user Plugins.
	/// 7. Call <see cref="RunInitialization"/>.
	/// </remarks>
	public sealed class CoreStartup
	{
		List<string> pluginFiles = new List<string>();
		List<string> disabledPlugins = new List<string>();
		bool externalPluginsConfigured;
		string propertiesName;
		string configDirectory;
		string dataDirectory;
		string applicationName;
		
		/// <summary>
		/// Sets the name used for the properties (only name, without path or extension).
		/// Must be set before StartCoreServices() is called.
		/// </summary>
		public string PropertiesName {
			get {
				return propertiesName;
			}
			set {
				if (value == null || value.Length == 0)
					throw new ArgumentNullException("value");
				propertiesName = value;
			}
		}
		
		/// <summary>
		/// Sets the directory name used for the property service.
		/// Must be set before StartCoreServices() is called.
		/// Use null to use the default path "%ApplicationData%\%ApplicationName%",
		/// where %ApplicationData% is the system setting for
		/// "c:\documents and settings\username\application data"
		/// and %ApplicationName% is the application name you used in the
		/// CoreStartup constructor call.
		/// </summary>
		public string ConfigDirectory {
			get {
				return configDirectory;
			}
			set {
				configDirectory = value;
			}
		}
		
		/// <summary>
		/// Sets the data directory used to load resources.
		/// Must be set before StartCoreServices() is called.
		/// Use null to use the default path "ApplicationRootPath\data".
		/// </summary>
		public string DataDirectory {
			get {
				return dataDirectory;
			}
			set {
				dataDirectory = value;
			}
		}
		
		/// <summary>
		/// Creates a new CoreStartup instance.
		/// </summary>
		/// <param name="applicationName">
		/// The name of your application.
		/// This is used as default title for message boxes,
		/// default name for the configuration directory etc.
		/// </param>
		public CoreStartup(string applicationName)
		{
			if (applicationName == null)
				throw new ArgumentNullException("applicationName");
			this.applicationName = applicationName;
			propertiesName = applicationName + "Properties";
			MessageService.DefaultMessageBoxTitle = applicationName;
			MessageService.ProductName = applicationName;
		}
		
		/// <summary>
		/// Find Plugins by searching all .addin files recursively in <paramref name="pluginDir"/>.
		/// The found Plugins are added to the list of Plugin files to load.
		/// </summary>
		public void AddPluginsFromDirectory(string pluginDir)
		{
			if (pluginDir == null)
				throw new ArgumentNullException("pluginDir");
			pluginFiles.AddRange(FileUtility.SearchDirectory(pluginDir, "*.addin"));
		}
		
		/// <summary>
		/// Add the specified .addin file to the list of Plugin files to load.
		/// </summary>
		public void AddPluginFile(string pluginFile)
		{
			if (pluginFile == null)
				throw new ArgumentNullException("pluginFile");
			pluginFiles.Add(pluginFile);
		}
		
		/// <summary>
		/// Use the specified configuration file to store information about
		/// disabled Plugins and external Plugins.
		/// You have to call this method to support the <see cref="PluginManager"/>.
		/// </summary>
		/// <param name="pluginConfigurationFile">
		/// The name of the file used to store the list of disabled Plugins
		/// and the list of installed external Plugins.
		/// A good value for this parameter would be
		/// <c>Path.Combine(<see cref="PropertyService.ConfigDirectory"/>, "Plugins.xml")</c>.
		/// </param>
		public void ConfigureExternalPlugins(string pluginConfigurationFile)
		{
			externalPluginsConfigured = true;
			PluginManager.ConfigurationFileName = pluginConfigurationFile;
			PluginManager.LoadPluginConfiguration(pluginFiles, disabledPlugins);
		}
		
		/// <summary>
		/// Configures user Plugin support.
		/// </summary>
		/// <param name="pluginInstallTemp">
		/// The Plugin installation temporary directory.
		/// ConfigureUserPlugins will install the Plugins from this directory and
		/// store the parameter value in <see cref="PluginManager.PluginInstallTemp"/>.
		/// </param>
		/// <param name="userPluginPath">
		/// The path where user Plugins are installed to.
		/// Plugins from this directory will be loaded.
		/// </param>
		public void ConfigureUserPlugins(string pluginInstallTemp, string userPluginPath)
		{
			if (!externalPluginsConfigured) {
				throw new InvalidOperationException("ConfigureExternalPlugins must be called before ConfigureUserPlugins");
			}
			PluginManager.PluginInstallTemp = pluginInstallTemp;
			PluginManager.UserPluginPath = userPluginPath;
			if (Directory.Exists(pluginInstallTemp)) {
				PluginManager.InstallPlugins(disabledPlugins);
			}
			if (Directory.Exists(userPluginPath)) {
				AddPluginsFromDirectory(userPluginPath);
			}
		}
		
		/// <summary>
		/// Initializes the Plugin system.
		/// This loads the Plugins that were added to the list,
		/// then it executes the <see cref="ICommand">commands</see>
		/// in <c>/Workspace/Autostart</c>.
		/// </summary>
		public void RunInitialization()
		{
			PluginTree.Load(pluginFiles, disabledPlugins);
			
			// run workspace autostart commands
			LoggingService.Info("Running autostart commands...");
			foreach (ICommand command in PluginTree.BuildItems<ICommand>("/Workspace/Autostart", null, false)) {
				try {
					command.Run();
				} catch (Exception ex) {
					// allow startup to continue if some commands fail
					MessageService.ShowError(ex);
				}
			}
		}
		
		/// <summary>
		/// Starts the core services.
		/// This initializes the PropertyService and ResourceService.
		/// </summary>
		public void StartCoreServices()
		{
			if (configDirectory == null)
				configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				                               applicationName);
			PropertyService.InitializeService(configDirectory,
			                                  dataDirectory ?? Path.Combine(FileUtility.ApplicationRootPath, "data"),
			                                  propertiesName);
			PropertyService.Load();
			ResourceService.InitializeService(FileUtility.Combine(PropertyService.DataDirectory, "resources"));
			StringParser.Properties["AppName"] = applicationName;
		}
	}
}
