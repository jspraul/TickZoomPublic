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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace TickZoom.TZData
{
	public class Register : Command {
		string currentDir = System.Environment.CurrentDirectory;
		string directory;
		
		public Register() {
			Assembly assembly = Assembly.GetEntryAssembly();
			if( assembly != null) {
				string location = assembly.Location;
				directory = Path.GetDirectoryName(location);
			}
		}
		
		public void Run(string[] args) {
			AssociateFile(".tck","TZData","TickZoom Tick Data",currentDir+@"\tzdata.exe",null,0);
			AddToPath();
		}
		
		public void AddToPath() {
			Console.WriteLine("Adding to PATH environment variable.");
			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Environment",true);
			string variable = "TickZoom";
			string tickZoom = (string) key.GetValue(variable);
			string path = (string) key.GetValue("Path");
			
			if( path == null) {
				key.SetValue("Path",directory);
				key.SetValue(variable,directory);
				BroadCastPathChange();
			} else if(!path.Contains(directory)) {
				if( !string.IsNullOrEmpty(tickZoom) && path.Contains(tickZoom)) {
					path.Replace(tickZoom,directory);
				} else {
					path += ";" + directory;
				}
				key.SetValue("Path",path);
				key.SetValue(variable,directory);
				BroadCastPathChange();
			}
		}
		
		[DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg,
		UIntPtr wParam, string lParam, SendMessageTimeoutFlags fuFlags,
		uint uTimeout, out UIntPtr lpdwResult);

		public enum SendMessageTimeoutFlags:uint
		{
			SMTO_NORMAL= 0x0000
			, SMTO_BLOCK = 0x0001
			, SMTO_ABORTIFHUNG = 0x0002
			, SMTO_NOTIMEOUTIFNOTHUNG = 0x0008
		}

		private static void BroadCastPathChange()
		{
			//Notify all windows that User Environment variables are changed
			IntPtr HWND_BROADCAST = (IntPtr) 0xffff;
			const UInt32 WM_SETTINGCHANGE = 0x001A;
			UIntPtr result;
			IntPtr settingResult = SendMessageTimeout(HWND_BROADCAST,
				WM_SETTINGCHANGE,
				(UIntPtr)0,
				"Environment",
				SendMessageTimeoutFlags.SMTO_NORMAL,
				10000,
				out result);
		}	

		public void AssociateFile(string extension, string progId, string description, string executeable, string iconFile, int iconIdx)
		{
			Console.WriteLine("Adding file association for .tck files.");
		    try
		    {                   
				if (extension[0] != '.')
				{
				    extension = "."+extension;
				}
				
				RegistryKey extKey = Registry.ClassesRoot.OpenSubKey(extension, true);
			    if (extKey == null)
			    { 
					extKey = Registry.ClassesRoot.CreateSubKey(extension);
			    }
		        using (extKey)
		        {
		            extKey.SetValue(string.Empty, progId);
		        }
				
				RegistryKey progIdKey = Registry.ClassesRoot.OpenSubKey(progId,true);
			    if (progIdKey == null)
			    {
			         progIdKey = Registry.ClassesRoot.CreateSubKey(progId);
			    }
		        using (progIdKey)
		        {
		            progIdKey.SetValue(string.Empty, description);
		            if( iconFile != null) {
			            using (RegistryKey defaultIcon = progIdKey.CreateSubKey("DefaultIcon"))
			            {
			                 defaultIcon.SetValue(string.Empty, String.Format("\"{0}\",{1}", iconFile, iconIdx));
			            }
		            }
					RegistryKey command = progIdKey.OpenSubKey("shell\\open\\command",true);
					if( command == null) {
						command = progIdKey.CreateSubKey("shell\\open\\command");
					}
		            using (command)
		            {
		                 command.SetValue(string.Empty, String.Format("\"{0}\" \"open\" \"%1\"", executeable));
		            }
		        }
		    }
		    catch(Exception ex)
		    {
		     	Console.WriteLine( ex.Message);
		    }
		}
		
		public string[] Usage() {
			List<string> lines = new List<string>();
			string name = Assembly.GetEntryAssembly().GetName().Name;
			lines.Add(name + " register");
			return lines.ToArray();
		}
		
		public string Directory {
			get { return directory; }
			set { directory = value; }
		}
	}
}
