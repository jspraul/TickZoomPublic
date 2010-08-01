// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 5352 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TickZoom.Loader
{
	/// <summary>
	/// Represents a versioned reference to an Plugin. Used by <see cref="PluginManifest"/>.
	/// </summary>
	public class PluginReference : ICloneable
	{
		string name;
		Version minimumVersion;
		Version maximumVersion;
		bool requirePreload;
		private static readonly Version entryVersion = Assembly.GetExecutingAssembly().GetName().Version;
		
		public Version MinimumVersion {
			get {
				return minimumVersion;
			}
		}
		
		public Version MaximumVersion {
			get {
				return maximumVersion;
			}
		}
		
		public bool RequirePreload {
			get { return requirePreload; }
		}
		
		
		public string Name {
			get {
				return name;
			}
			set {
				if (value == null) throw new ArgumentNullException("name");
				if (value.Length == 0) throw new ArgumentException("name cannot be an empty string", "name");
				name = value;
			}
		}
		
		/// <returns>Returns true when the reference is valid.</returns>
		public bool Check(Dictionary<string, Version> plugins, out Version versionFound)
		{
			if (plugins.TryGetValue(name, out versionFound)) {
				return CompareVersion(versionFound, minimumVersion) >= 0
					&& CompareVersion(versionFound, maximumVersion) <= 0;
			} else {
				return false;
			}
		}
		
		/// <summary>
		/// Compares two versions and ignores unspecified fields (unlike Version.CompareTo)
		/// </summary>
		/// <returns>-1 if a &lt; b, 0 if a == b, 1 if a &gt; b</returns>
		int CompareVersion(Version a, Version b)
		{
			if (a.Major != b.Major) {
				return a.Major > b.Major ? 1 : -1;
			}
			if (a.Minor != b.Minor) {
				return a.Minor > b.Minor ? 1 : -1;
			}
			if (a.Build < 0 || b.Build < 0)
				return 0;
			if (a.Build != b.Build) {
				return a.Build > b.Build ? 1 : -1;
			}
			if (a.Revision < 0 || b.Revision < 0)
				return 0;
			if (a.Revision != b.Revision) {
				return a.Revision > b.Revision ? 1 : -1;
			}
			return 0;
		}
		
		public static PluginReference Create(Properties properties, string hintPath)
		{
			PluginReference reference = new PluginReference(properties["addin"]);
			string version = properties["version"];
			if (version != null && version.Length > 0) {
				int pos = version.IndexOf('-');
				if (pos > 0) {
					reference.minimumVersion = ParseVersion(version.Substring(0, pos), hintPath);
					reference.maximumVersion = ParseVersion(version.Substring(pos + 1), hintPath);
				} else {
					reference.maximumVersion = reference.minimumVersion = ParseVersion(version, hintPath);
				}
				
				if (reference.Name == "TickZoom") {
					// HACK: SD 3.0 Plugins work with TickZoom 3.1
					// Because some 3.0 Plugins restrict themselves to SD 3.0, we extend the
					// supported SD range.
					if (reference.maximumVersion == new Version("3.0") || reference.maximumVersion == new Version("3.1")) {
						reference.maximumVersion = new Version(entryVersion.Major + "." + entryVersion.Minor);
					}
				}
			}
			reference.requirePreload = string.Equals(properties["requirePreload"], "true", StringComparison.OrdinalIgnoreCase);
			return reference;
		}
		
		internal static Version ParseVersion(string version, string hintPath)
		{
			if (version == null || version.Length == 0)
				return new Version(0,0,0,0);
			if (version.StartsWith("@")) {
				if (version == "@TickZoomCoreVersion") {
					return entryVersion;
				}
				if (hintPath != null) {
					string fileName = Path.Combine(hintPath, version.Substring(1));
					try {
						FileVersionInfo info = FileVersionInfo.GetVersionInfo(fileName);
						return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);
					} catch (FileNotFoundException ex) {
						throw new PluginLoadException("Cannot get version '" + version + "': " + ex.Message);
					}
				}
				return new Version(0,0,0,0);
			} else {
				return new Version(version);
			}
		}
		
		public PluginReference(string name) : this(name, new Version(0,0,0,0), new Version(int.MaxValue, int.MaxValue)) { }
		
		public PluginReference(string name, Version specificVersion) : this(name, specificVersion, specificVersion) { }
		
		public PluginReference(string name, Version minimumVersion, Version maximumVersion)
		{
			this.Name = name;
			if (minimumVersion == null) throw new ArgumentNullException("minimumVersion");
			if (maximumVersion == null) throw new ArgumentNullException("maximumVersion");
			
			this.minimumVersion = minimumVersion;
			this.maximumVersion = maximumVersion;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is PluginReference)) return false;
			PluginReference b = (PluginReference)obj;
			return name == b.name && minimumVersion == b.minimumVersion && maximumVersion == b.maximumVersion;
		}
		
		public override int GetHashCode()
		{
			return name.GetHashCode() ^ minimumVersion.GetHashCode() ^ maximumVersion.GetHashCode();
		}
		
		public override string ToString()
		{
			if (minimumVersion.ToString() == "0.0.0.0") {
				if (maximumVersion.Major == int.MaxValue) {
					return name;
				} else {
					return name + ", version <" + maximumVersion.ToString();
				}
			} else {
				if (maximumVersion.Major == int.MaxValue) {
					return name + ", version >" + minimumVersion.ToString();
				} else if (minimumVersion == maximumVersion) {
					return name + ", version " + minimumVersion.ToString();
				} else {
					return name + ", version " + minimumVersion.ToString() + "-" + maximumVersion.ToString();
				}
			}
		}
		
		public PluginReference Clone()
		{
			return new PluginReference(name, minimumVersion, maximumVersion);
		}
		
		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
