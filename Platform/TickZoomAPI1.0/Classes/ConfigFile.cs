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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of SettingsFile.
	/// </summary>
	public class ConfigFile : System.Configuration.AppSettingsReader
	{
		private XmlNode node;
		private string _cfgFile;
		
		public ConfigFile() {
			var configFile = FindInPath();
			if( configFile == null) {
	  			string appDataPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData);
  				appDataPath = Path.Combine(appDataPath,"TickZoom1.1");
				UseFile( Path.Combine(appDataPath, "TickZoom.config"));
			} else {
				UseFile( configFile);
			}
		}
			
		public ConfigFile(string name) {
			UseFile(name);
		}
		
		public ConfigFile(string name, string defaultContents) {
			this.defaultContents = defaultContents;
			UseFile(name);
		}
		
		private void UseFile(string name) {
			_cfgFile = name;
			if( !Exists) {
				CreateFile();
			}
		}
		
		public string FindInPath() {
			var path = GetExecutablePath();
			do {
				var file = path + Path.DirectorySeparatorChar + "TickZoom.config";
				if( File.Exists( file)) {
					return file;
				}
				path = Path.GetDirectoryName(path);
			} while( path != null);
			return null;
		}
		
		public bool Exists {
			get {
				return File.Exists(_cfgFile);
			}
		}

		private string GetExecutablePath()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
		}


		
		public string GetValue( string property, string defaultValue) {
			var result = GetValue(property);
			if( result == null) {
				result = defaultValue;
			}
			return result;
		}

		public string GetValueOLD(string property)
		{
			XmlDocument doc = new XmlDocument();
			loadDoc(doc);
			string key = "//appSettings//add[@key='" + property + "']";
			// retrieve the selected node
			try
			{
				node =  doc.SelectSingleNode(key);
				if ( !(node is XmlElement))
				{
					return null;
				}
				var element = (XmlElement) node;
				return element.GetAttribute("value");
			}
			catch
			{
				return null;
			}
		}

		private string[] SplitPathProperty( ref string property) {
			var strings = property.Split( new char[] { '/', '\\' });
			if( strings.Length > 1) {
				property = strings[strings.Length-1];
				Array.Resize<string>(ref strings, strings.Length -1);
				return strings;
			} else {
				return new string[0];
			}
		}
		public string GetValue(string property)
		{
			var strings = SplitPathProperty( ref property);
			var path = "";
			if( strings.Length > 0) {
				path = string.Join("//",strings);
				path = "//" + path;
			}
			XmlDocument doc = new XmlDocument();
			loadDoc(doc);
			string key = "//appSettings" + path + "//add[@key='" + property + "']";
			// retrieve the selected node
			try
			{
				node =  doc.SelectSingleNode(key);
				if ( !(node is XmlElement))
				{
					return null;
				}
				var element = (XmlElement) node;
				return element.GetAttribute("value");
			}
			catch
			{
				return null;
			}
		}

		public new object GetValue (string property, System.Type sType)
		{
			string retVal = GetValue(property);
			if (sType == typeof(string))
				return Convert.ToString(retVal);
			else
				if (sType == typeof(bool))
			{
				if (retVal.Equals("True") || retVal.Equals("False"))
					return Convert.ToBoolean(retVal);
				else
					return false;
			}
			else
				if (sType == typeof(int))
				return Convert.ToInt32(retVal);
			else
				if (sType == typeof(double))
				return Convert.ToDouble(retVal);
			else
				if (sType == typeof(DateTime))
				return Convert.ToDateTime(retVal);
			else
				if (sType == typeof(ushort))
				return Convert.ToUInt16(retVal);
			else
				return Convert.ToString(retVal);
		}

		public bool SetValue2(string property, string val)
		{
			XmlDocument doc = new XmlDocument();
			loadDoc(doc);
			try
			{
				// retrieve the target node
				string key = "//appSettings//add[@key='" + property + "']";
				string sNode = "//appSettings";
				node =  doc.SelectSingleNode(sNode);
				if( node == null ) {
					throw new ApplicationException("Can't find appSettings configuration section in " + _cfgFile);
				}
				// Set element that contains the key
				XmlElement targetElem= (XmlElement) node.SelectSingleNode(key);
				if (targetElem!=null)
				{
					// set new value
					targetElem.SetAttribute("value", val);
				}
					// create new element with key/value pair and add it
				else
				{
					
					sNode = key.Substring(key.LastIndexOf("//")+2);
					
					XmlElement entry = doc.CreateElement(sNode.Substring(0, sNode.IndexOf("[@")).Trim());
					sNode =  sNode.Substring(sNode.IndexOf("'")+1);
					
					entry.SetAttribute("key", sNode.Substring(0, sNode.IndexOf("'")) );
					
					entry.SetAttribute("value", val);
					node.AppendChild(entry);
				}
				saveDoc(doc, this._cfgFile);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool SetValue(string property, string value)
		{
			var strings = SplitPathProperty(ref property);		
			XmlDocument doc = new XmlDocument();
			loadDoc(doc);
			try
			{
				// retrieve the target node
				string key = "add[@key='" + property + "']";
				string sNode = "//appSettings";
				node =  doc.SelectSingleNode(sNode);
				if( node == null ) {
					throw new ApplicationException("Can't find appSettings configuration section in " + _cfgFile);
				}
				foreach( var pathElement in strings) {
					var next = node.SelectSingleNode( pathElement);
					if( next == null) {
						var entry = doc.CreateElement( pathElement);
						node.AppendChild( entry);
						next = entry;
					}
					node = next;
				}
				// Set element that contains the key
				XmlElement targetElem= (XmlElement) node.SelectSingleNode(key);
				if (targetElem!=null)
				{
					// set new value
					targetElem.SetAttribute("value", value);
				}
					// create new element with key/value pair and add it
				else
				{
					XmlElement entry = doc.CreateElement("add");
					entry.SetAttribute("key", property );
					entry.SetAttribute("value", value);
					node.AppendChild(entry);
				}
				saveDoc(doc, this._cfgFile);
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		private void saveDoc (XmlDocument doc, string docPath)
		{
			// save document
			// choose to ignore if web.config since it may cause server sessions interruptions
			if(  this._cfgFile.Equals("web.config") )
				return;
			else
				try
				{
					XmlTextWriter writer = new XmlTextWriter( docPath , null );
					writer.Formatting = Formatting.Indented;
					doc.WriteTo( writer );
					writer.Flush();
					writer.Close();
					return;
				}
				catch
				{}
		}

		public bool removeElement (string key)
		{
			XmlDocument doc = new XmlDocument();
			loadDoc(doc);
			try
			{
				string sNode = key.Substring(0, key.LastIndexOf("//"));
				// retrieve the appSettings node
				node =  doc.SelectSingleNode(sNode);
				if( node == null )
					return false;
				// XPath select setting "add" element that contains this key to remove
				XmlNode nd = node.SelectSingleNode(key);
				node.RemoveChild(nd);
				saveDoc(doc, this._cfgFile);
				return true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		private void loadDoc ( XmlDocument doc )
		{
			using( StreamReader streamReader = new StreamReader(_cfgFile)) {
				doc.Load( streamReader);
			}
		}
		
		private void CreateFile() {
			Directory.CreateDirectory(Path.GetDirectoryName(_cfgFile));
			string contents = BeautifyXML(defaultContents);
	        using (StreamWriter sw = new StreamWriter(_cfgFile)) 
	        {
	            // Add some text to the file.
	            sw.Write( contents);
	        }
		}
		
		private static string BeautifyXML(string xml)
		{
			using( StringReader reader = new StringReader(xml)) {
				XmlDocument doc = new XmlDocument();
				doc.Load( reader);
			    StringBuilder sb = new StringBuilder();
			    XmlWriterSettings settings = new XmlWriterSettings();
			    settings.Indent = true;
			    settings.IndentChars = "  ";
			    settings.NewLineChars = "\r\n";
			    settings.NewLineHandling = NewLineHandling.Replace;
			    using( XmlWriter writer = XmlWriter.Create(sb, settings)) {
				    doc.Save(writer);
			    }
			    return sb.ToString();
			}
		}
		
		public override string ToString()
		{
			return _cfgFile;
		}
		
		private string defaultContents = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <appSettings>
  </appSettings>
</configuration>";
	}
}