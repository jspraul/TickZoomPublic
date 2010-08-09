#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.Update
{
	/// <summary>
	/// Description of Factory.
	/// </summary>
	public class FactoryLoaderDefault : FactoryLoader
	{
		private static int errorCount = 0;
		private static object locker = new object();
		private static bool IsResolverSetup = false;
		private static readonly Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
		private bool debugFlag = false;

		public FactoryLoaderDefault()
		{
			Assembly.GetExecutingAssembly().GetName().Version = new Version();
			string path = GetShadowCopyFolder();
			string[] args = Environment.GetCommandLineArgs();
			string debugString = Factory.Settings["DebugFactory"];
			if (!string.IsNullOrEmpty(debugString)) {
				debugFlag = debugString.ToLower() == "true";
			}
			if (Directory.Exists(path)) {
				long startTime = Environment.TickCount;
				bool isDeleted = false;
				while (!isDeleted && Environment.TickCount - startTime < 2000) {
					try {
						Directory.Delete(path, true);
						isDeleted = true;
					} catch (Exception ex) {
						LogMsg("warning: error removing shadow copy folder: " + GetShadowCopyFolder() + ": " + ex.Message);
						Thread.Sleep(500);
					}
				}
			}
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
		}

		internal void LogMsg(string message)
		{
			if (debugFlag) {
				Console.WriteLine(message);
			}
		}

		public object Load(Type type, string assemblyName, params object[] args)
		{
			LogMsg("Attempting Load of " + type + " from " + assemblyName);
			errorCount = 0;
			string currentDirectoryPath = GetExecutablePath();

			LogMsg("Executable Path = " + currentDirectoryPath);

			object obj = null;
			try {
				if (obj == null && !string.IsNullOrEmpty(currentDirectoryPath)) {
					obj = Load(currentDirectoryPath, type, assemblyName, false, args);
				}
				if (obj == null && !string.IsNullOrEmpty(currentDirectoryPath)) {
					obj = Load(currentDirectoryPath, type, assemblyName, true, args);
				}
			} catch (Exception ex) {
				// if not found in main bin folder, look in the update folder
				LogMsg("Individual load failed: " + ex.Message);
			}
			if (obj == null) {
				string message = "Sorry, type " + type.Name + " was not found in any assembly named, " + assemblyName + ", in " + currentDirectoryPath;
				LogMsg(message);
				throw new Exception(message);
			}
			return obj;
		}



		private string GetExecutablePath()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
		}

		private string GetShadowCopyFolder()
		{
			return GetExecutablePath() + Path.DirectorySeparatorChar + "ShadowCopy" + Path.DirectorySeparatorChar + System.Diagnostics.Process.GetCurrentProcess().ProcessName + Path.DirectorySeparatorChar;
		}

		private object Load(string path, Type type, string partialName, bool runUpdate, params object[] args)
		{
			string internalName = partialName;

			LogMsg("Current version : " + currentVersion);

			List<string> allFiles = new List<string>();

			if (runUpdate) {
				LogMsg("Looking for AutoUpdate downloaded files.");
				string appDataFolder = Factory.Settings["AppDataFolder"];
				string autoUpdateFolder = appDataFolder + "\\AutoUpdate";

				Directory.CreateDirectory(autoUpdateFolder);
				allFiles.AddRange(Directory.GetFiles(autoUpdateFolder, partialName + "-" + currentVersion + ".dll", SearchOption.AllDirectories));
				allFiles.AddRange(Directory.GetFiles(autoUpdateFolder, partialName + "-" + currentVersion + ".exe", SearchOption.AllDirectories));
			}

			allFiles.AddRange(Directory.GetFiles(path, partialName + ".dll", SearchOption.AllDirectories));
			allFiles.AddRange(Directory.GetFiles(path, partialName + ".exe", SearchOption.AllDirectories));

			LogMsg("Files being searched:");
			for (int i = 0; i < allFiles.Count; i++) {
				LogMsg(allFiles[i]);
			}
			if (allFiles.Count == 0) {
				return null;
			}

			foreach (String fullPath in allFiles) {
				try {
					AssemblyName assemblyName = AssemblyName.GetAssemblyName(fullPath);
					LogMsg("Considering " + assemblyName);
					LogMsg("Checking for " + internalName + " and " + currentVersion);
					if (!assemblyName.Version.Equals(currentVersion)) {
						LogMsg("Version mismatch. Skipping");
						continue;
					}
					if (!assemblyName.Name.Equals(internalName)) {
						LogMsg("Internal name mismatch. Skipping");
						continue;
					}

					// Create shadow copy.
					LogMsg("Creating shadow copy for loading...");
					string fileName = Path.GetFileName(fullPath);
					string fileDirectory = Path.GetDirectoryName(fullPath);
					string loadPath = fullPath;
					if (fileDirectory != GetExecutablePath()) {
						try {
							string shadowPath = GetShadowCopyFolder() + fileName;
							File.Copy(fullPath, shadowPath, true);
							loadPath = shadowPath;
						} catch (IOException ex) {
							LogMsg("Unable to copy " + fileName + " into shadow copy folder. Actual error: '" + ex.GetType() + ": " + ex.Message + "'. Ignoring. Continuing.");
						}
					}
					Assembly assembly = Assembly.LoadFrom(loadPath);

					object obj = InstantiateObject(assembly, type, args);
					if (obj != null)
						return obj;
				} catch (ReflectionTypeLoadException ex) {
					string exceptions = System.Environment.NewLine;
					for (int i = 0; i < ex.LoaderExceptions.Length; i++) {
						exceptions += ex.LoaderExceptions[i].Message + System.Environment.NewLine;
					}
					LogMsg(partialName + " load failed for '" + fullPath + "'.\n" + "This is probably due to a mismatch version. Skipping this one.\n" + "Further detail:\n" + exceptions + ex.ToString());
				} catch (Exception ex) {
					LogMsg(partialName + " load unsuccessful for '" + fullPath + "'.\n" + "This is probably due to a mismatch version. Skipping this one.\n" + "Further detail: " + ex.Message);
				}
			}
			return null;
		}

		private object InstantiateObject(Assembly assembly, Type type, object[] args)
		{
			foreach (Type t in assembly.GetTypes()) {
				if (t.IsClass && !t.IsAbstract && !t.IsInterface) {
					if (t.GetInterface(type.FullName) != null) {
						LogMsg("Found the interface: " + type.Name);
						try {
							LogMsg("Creating an instance.");
							return Activator.CreateInstance(t, args);
						} catch (MissingMethodException) {
							errorCount++;
							throw new ApplicationException("'" + t.Name + "' failed to load due to missing default constructor");
						} catch (Exception ex) {
							LogMsg(ex.ToString());
						}
					}
				}
			}
			return null;
		}

		private Version GetCurrentVersion(string[] files, string partialName)
		{

			Version currentVersion = new Version(0, 0, 0, 0);
			foreach (String filename in files) {
				if (!filename.Contains(partialName))
					continue;
				try {
					AssemblyName assemblyName = AssemblyName.GetAssemblyName(filename);
					currentVersion = assemblyName.Version;
					break;
				} catch (Exception ex) {
					throw new Exception(partialName + " check version failed for '" + filename + "': " + ex.Message);
				}
			}
			return currentVersion;
		}

		private string GetHighestModel(string[] files, string partialName, string engineModel, Version maxVersion)
		{

			string value = null;
			foreach (String filename in files) {
				if (!filename.Contains(partialName))
					continue;
				try {
					AssemblyName assemblyName = AssemblyName.GetAssemblyName(filename);
					if (assemblyName.Version == maxVersion && assemblyName.Name.Contains(engineModel)) {
						value = assemblyName.Name;
						break;
					}
				} catch (Exception ex) {
					LogMsg("WARNING: " + partialName + " check version failed for '" + filename + "': " + ex.Message);
				}
			}
			return value;
		}

		public void SetupResolver()
		{
			if (!IsResolverSetup) {
				lock (locker) {
					if (!IsResolverSetup) {
						LogMsg("Setup ResolveEventHandler");
						AppDomain currentDomain = AppDomain.CurrentDomain;
						currentDomain.AssemblyResolve += new ResolveEventHandler(TZResolveEventHandler);
						IsResolverSetup = true;
					}
				}
			}
		}

		private Assembly TZResolveEventHandler(object sender, ResolveEventArgs args)
		{
			LogMsg("===============================");
			LogMsg("WARN: ResolveEventHandle called");
			LogMsg("===============================");
			//This handler is called only when the common language runtime tries to bind to the assembly and fails.

			//Retrieve the list of referenced assemblies in an array of AssemblyName.
			Assembly MyAssembly, objExecutingAssemblies;
			string strTempAssmbPath = "";

			objExecutingAssemblies = Assembly.GetExecutingAssembly();
			AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

			//Loop through the array of referenced assembly names.
			foreach (AssemblyName strAssmbName in arrReferencedAssmbNames) {
				//Check for the assembly names that have raised the "AssemblyResolve" event.
				if (strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == args.Name.Substring(0, args.Name.IndexOf(","))) {
					//Build the path of the assembly from where it has to be loaded.				
					strTempAssmbPath = args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
					break;
				}

			}

			//Load the assembly from the specified path. 					
			MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

			//Return the loaded assembly.
			return MyAssembly;
		}
		
	    public bool AutoUpdate(BackgroundWorker bw) {
	    	bool retVal = false;
			AutoUpdate updater = new AutoUpdate();
			updater.BackgroundWorker = bw;
			
			if( updater.UpdateAll() ) {
				retVal = true;
			}
   			return retVal;
		}
		
	}
}
