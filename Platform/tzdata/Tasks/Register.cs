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
using Microsoft.Win32;

namespace TickZoom.TZData
{
	public class Register {
		string currentDir = System.Environment.CurrentDirectory;
		public Register(string[] args) {
			AssociateFile(".tck","TZData","TickZoom Tick Data",currentDir+@"\tzdata.exe",null,0);
		}
	
		public static void AssociateFile(string extension, string progId, string description, string executeable, string iconFile, int iconIdx)
		{
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
	}
}
