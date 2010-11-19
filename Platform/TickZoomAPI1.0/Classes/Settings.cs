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
using System.Configuration;
using System.IO;

namespace TickZoom.Api
{
	public class Settings {
		private const string defaultDataFolder = "TickZoomHome";
		
		Dictionary<string,string> obsoleteSettings = new Dictionary<string,string>();
		public Settings() {
			obsoleteSettings.Add("ServiceAddress",null);
			obsoleteSettings.Add("ServicePort",null);
		}
		public string this[string name]
		{
			get { 
				if( name == "AppDataFolder") { return GetInstallationProperty(name); }
				if( name == "PriceDataFolder") { return GetInstallationProperty(name); }
				if( obsoleteSettings.ContainsKey(name)) {
					throw new ApplicationException("Used obsolete setting: " + name);
				}
				string retVal = ConfigurationManager.AppSettings[name];
				return retVal;
			}
		}
		
		private string GetInstallationProperty(string property) {
			// ConfigFile gets either global config or a local
			// installation config if found anywhere in the ancestry
			// of the working directore.
			ConfigFile config = new ConfigFile();
			string retVal = config.GetValue(property);
			if( retVal != null) {
				if( Directory.Exists(retVal)) {
					return retVal;
				} else {
					retVal = null;
				}
			}
			bool setValue = false;
			if( retVal == null) {
				setValue = true;
				retVal = ScanForFolder();
			}
			if( retVal == null) {
				retVal = ScanForLargestDrive();
			}
			if( retVal == null) {
				throw new ApplicationException("Failed to find a drive to put TickZoomData");
			}
			if( setValue) {
				config.SetValue(property,retVal);
			}
			return retVal;
		}
		
		private string ScanForFolder() {
			foreach( DriveInfo drive in DriveInfo.GetDrives()) {
				var path = drive.Name + defaultDataFolder;
				if( Directory.Exists(path)) {
					return path;
				}
			}
			return null;
		}
		
		private string ScanForLargestDrive() {
			long maxSpace = 0;
			DriveInfo maxDrive = null;
			foreach( DriveInfo drive in DriveInfo.GetDrives()) {
				try {
					if( drive.AvailableFreeSpace > maxSpace) {
						maxDrive = drive;
						maxSpace = drive.AvailableFreeSpace;
					}
				} catch( IOException) {
					// Ignore. Try another drive.
				}
			}
			if( maxDrive != null) {
				return maxDrive + defaultDataFolder;
			} else {
				return null;
			}
		}
		
		public string DefaultDataFolder {
			get { return defaultDataFolder; }
		}
	}
}
