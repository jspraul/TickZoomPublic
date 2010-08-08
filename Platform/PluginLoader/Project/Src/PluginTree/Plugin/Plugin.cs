// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 3671 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TickZoom.Loader
{
	public sealed class Plugin
	{
		Properties    properties = new Properties();
		List<Runtime> runtimes   = new List<Runtime>();
		List<string> bitmapResources = new List<string>();
		List<string> stringResources = new List<string>();
		
		internal string pluginFileName = null;
		PluginManifest manifest = new PluginManifest();
		Dictionary<string, ExtensionPath> paths = new Dictionary<string, ExtensionPath>();
		PluginAction action = PluginAction.Disable;
		bool enabled;
		
		static bool hasShownErrorMessage = false;

		public object CreateObject(string className)
		{
			LoadDependencies();
			foreach (Runtime runtime in runtimes) {
				object o = runtime.CreateInstance(className);
				if (o != null) {
					return o;
				}
			}
			if (!hasShownErrorMessage) {
				hasShownErrorMessage = true;
				LoggingService.Error("Cannot create object: " + className + "\nFuture missing objects will not cause an error message.");
			}
			return null;
		}
		
		public void LoadRuntimeAssemblies()
		{
			LoadDependencies();
			foreach (Runtime runtime in runtimes) {
				runtime.Load();
			}
		}
		
		bool dependenciesLoaded;
		
		void LoadDependencies()
		{
			if (!dependenciesLoaded) {
				dependenciesLoaded = true;
				foreach (PluginReference r in manifest.Dependencies) {
					if (r.RequirePreload) {
						bool found = false;
						foreach (Plugin plugin in PluginTree.Plugins) {
							if (plugin.Manifest.Identities.ContainsKey(r.Name)) {
								found = true;
								plugin.LoadRuntimeAssemblies();
							}
						}
						if (!found) {
							throw new PluginLoadException("Cannot load run-time dependency for " + r.ToString());
						}
					}
				}
			}
		}
		
		public override string ToString()
		{
			return "[Plugin: " + Name + "]";
		}
		
		string customErrorMessage;
		
		/// <summary>
		/// Gets the message of a custom load error. Used only when PluginAction is set to CustomError.
		/// Settings this property to a non-null value causes Enabled to be set to false and
		/// Action to be set to PluginAction.CustomError.
		/// </summary>
		public string CustomErrorMessage {
			get {
				return customErrorMessage;
			}
			internal set {
				if (value != null) {
					Enabled = false;
					Action = PluginAction.CustomError;
				}
				customErrorMessage = value;
			}
		}
		
		/// <summary>
		/// Action to execute when the application is restarted.
		/// </summary>
		public PluginAction Action {
			get {
				return action;
			}
			set {
				action = value;
			}
		}
		
		public List<Runtime> Runtimes {
			get {
				return runtimes;
			}
		}
		
		public Version Version {
			get {
				return manifest.PrimaryVersion;
			}
		}
		
		public string FileName {
			get {
				return pluginFileName;
			}
		}
		
		public string Name {
			get {
				return properties["name"];
			}
		}
		
		public PluginManifest Manifest {
			get {
				return manifest;
			}
		}
		
		public Dictionary<string, ExtensionPath> Paths {
			get {
				return paths;
			}
		}
		
		public Properties Properties {
			get {
				return properties;
			}
		}
		
		public List<string> BitmapResources {
			get {
				return bitmapResources;
			}
			set {
				bitmapResources = value;
			}
		}
		
		public List<string> StringResources {
			get {
				return stringResources;
			}
			set {
				stringResources = value;
			}
		}
		
		public bool Enabled {
			get {
				return enabled;
			}
			set {
				enabled = value;
				this.Action = value ? PluginAction.Enable : PluginAction.Disable;
			}
		}
		
		internal Plugin()
		{
		}
		
		static void SetupPlugin(XmlReader reader, Plugin plugin, string hintPath)
		{
			while (reader.Read()) {
				if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement()) {
					switch (reader.LocalName) {
						case "StringResources":
						case "BitmapResources":
							if (reader.AttributeCount != 1) {
								throw new PluginLoadException("BitmapResources requires ONE attribute.");
							}
							
							string filename = StringParser.Parse(reader.GetAttribute("file"));
							
							if(reader.LocalName == "BitmapResources")
							{
								plugin.BitmapResources.Add(filename);
							}
							else
							{
								plugin.StringResources.Add(filename);
							}
							break;
						case "Runtime":
							if (!reader.IsEmptyElement) {
								Runtime.ReadSection(reader, plugin, hintPath);
							}
							break;
						case "Include":
							if (reader.AttributeCount != 1) {
								throw new PluginLoadException("Include requires ONE attribute.");
							}
							if (!reader.IsEmptyElement) {
								throw new PluginLoadException("Include nodes must be empty!");
							}
							if (hintPath == null) {
								throw new PluginLoadException("Cannot use include nodes when hintPath was not specified (e.g. when PluginManager reads a .addin file)!");
							}
							string fileName = Path.Combine(hintPath, reader.GetAttribute(0));
							XmlReaderSettings xrs = new XmlReaderSettings();
							xrs.ConformanceLevel = ConformanceLevel.Fragment;
							using (XmlReader includeReader = XmlTextReader.Create(fileName, xrs)) {
								SetupPlugin(includeReader, plugin, Path.GetDirectoryName(fileName));
							}
							break;
						case "Path":
							if (reader.AttributeCount != 1) {
								throw new PluginLoadException("Import node requires ONE attribute.");
							}
							string pathName = reader.GetAttribute(0);
							ExtensionPath extensionPath = plugin.GetExtensionPath(pathName);
							if (!reader.IsEmptyElement) {
								ExtensionPath.SetUp(extensionPath, reader, "Path");
							}
							break;
						case "Manifest":
							plugin.Manifest.ReadManifestSection(reader, hintPath);
							break;
						default:
							throw new PluginLoadException("Unknown root path node:" + reader.LocalName);
					}
				}
			}
		}
		
		public ExtensionPath GetExtensionPath(string pathName)
		{
			if (!paths.ContainsKey(pathName)) {
				return paths[pathName] = new ExtensionPath(pathName, this);
			}
			return paths[pathName];
		}
		
		public static Plugin Load(TextReader textReader)
		{
			return Load(textReader, null);
		}
		
		public static Plugin Load(TextReader textReader, string hintPath)
		{
			Plugin plugin = new Plugin();
			using (XmlTextReader reader = new XmlTextReader(textReader)) {
				while (reader.Read()){
					if (reader.IsStartElement()) {
						switch (reader.LocalName) {
							case "Plugin":
								plugin.properties = Properties.ReadFromAttributes(reader);
								SetupPlugin(reader, plugin, hintPath);
								break;
							default:
								throw new PluginLoadException("Expected element \"Plugin\" at line " + reader.LineNumber + ".");
						}
					}
				}
			}
			return plugin;
		}
		
		public static Plugin Load(string fileName)
		{
			try {
				using (TextReader textReader = File.OpenText(fileName)) {
					Plugin plugin = Load(textReader, Path.GetDirectoryName(fileName));
					plugin.pluginFileName = fileName;
					return plugin;
				}
			} catch (Exception e) {
				throw new PluginLoadException("Can't load " + fileName, e);
			}
		}
	}
}
