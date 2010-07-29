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
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Xml;

using TickZoom.Api;

namespace TickZoom.Properties
{
	/// <summary>
	/// Description of ProjectProperties.
	/// </summary>
	public class ProjectPropertiesCommon : TickZoom.Api.ProjectProperties
	{
		TickZoom.Api.Log log = TickZoom.Api.Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			
		ChartProperties chartProperties = new ChartProperties();
		EngineProperties engineProperties = new EngineProperties();
		TickZoom.Api.ModelProperties modelProperties = new ModelPropertiesCommon();
		StarterProperties starterProperties;
		public ProjectPropertiesCommon()
		{
			starterProperties = new StarterProperties(chartProperties,engineProperties);
		}
		
		public static TickZoom.Api.ProjectProperties Create(TextReader projectXML) {
			ProjectPropertiesCommon project = new ProjectPropertiesCommon();
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = true;
			
			using (XmlReader reader = XmlReader.Create(projectXML))
			{
				try {
					// Read nodes one at a time  
					while (reader.Read())  
					{  
					    // Print out info on node  
					    switch( reader.NodeType) {
					    	case XmlNodeType.Element:
					    		project.HandleProject(reader);
					    		projectXML.Close();
					    		return project;
					    }
					}  				
				} catch( Exception ex) {
					project.Error( reader, ex.ToString());
					projectXML.Close();
					return null;
				}
			}
			projectXML.Close();
			return null;
		}
		
		private void HandleProject(XmlReader reader) {
			log.Debug("Handle Starter properties");
			log.Indent();
			while( reader.Read()) {
			    // Print out info on node  
			    switch( reader.NodeType) {
			    	case XmlNodeType.Element:
			    		if( "property".Equals(reader.Name) ) {
			    			HandleProperty(this, reader.GetAttribute("name"),reader.GetAttribute("value"));
			    		} else if( "propertyset".Equals(reader.Name)) {
			    			string name = reader.GetAttribute("name");
			    			if( "starter".Equals(name)) {
					    		HandleObject(Starter, reader);
				    		} else if( "chart".Equals(name)) {
					    		HandleObject(Chart, reader);
				    		} else if( "data".Equals(name)) {
					    		HandleObject(Starter.SymbolProperties, reader);
				    		} else if( "engine".Equals(name)) {
					    		HandleObject(Engine, reader);
			    			}
			    		}
			    		if( "strategy".Equals(reader.Name)) {
		    				ModelPropertiesCommon properties = new ModelPropertiesCommon();
			    			HandleModel(properties,reader,TickZoom.Api.ModelType.Strategy);
			    			Model = properties;
		    			}
			    		break;
			    	case XmlNodeType.EndElement:
			    		if( "projectproperties".Equals(reader.Name)) {
			    			log.Outdent();
				    		return;
			    		} else {
			    			Error(reader,"End of " + reader.Name + " tag in xml was unexpected");
			    		}
			    		break;
			    }
			}
   			Error(reader,"End of file unexpected");
		}
		
		private void HandleObject(object obj, XmlReader reader) {
			string tagName = reader.Name;
			log.Debug("Handle " + obj.GetType().Name);
			if( reader.IsEmptyElement) { return; }			
			log.Indent();
			while( reader.Read()) {
			    // Print out info on node  
			    switch( reader.NodeType) {
			    	case XmlNodeType.Element:
			    		if( "property".Equals(reader.Name) ) {
			    			HandleProperty(obj, reader.GetAttribute("name"), reader.GetAttribute("value"));
			    		} 
			    		if( "method".Equals(reader.Name) ) {
			    			HandleMethod(obj, reader.GetAttribute("name"), reader.GetAttribute("value"));
			    		} 
			    		break;
			    	case XmlNodeType.EndElement:
			    		if( tagName.Equals(reader.Name)) {
			    			log.Outdent();
				    		return;
			    		} else {
			    			Error(reader,"End of " + reader.Name + " tag in xml was unexpected");
			    		}
			    		break;
			    }
			}
			Error(reader,"Unexpected end of file");
		}
		
		private void Error( XmlReader reader, string msg) {
			IXmlLineInfo lineInfo = reader as IXmlLineInfo;
			string lineStr = "";
			if( lineInfo != null) {
				lineStr += " on line " + lineInfo.LineNumber + " at position " + lineInfo.LinePosition;
			}
			log.Debug(msg + lineStr);
			throw new ApplicationException(msg + lineStr);
		}
		
		private void HandleModel(ModelPropertiesCommon properties, XmlReader reader, TickZoom.Api.ModelType modelType) {
			string tagName = reader.Name;
			properties.ModelType = modelType;
			properties.Name = reader.GetAttribute("name");
			properties.Type = reader.GetAttribute("type");
			log.Debug("Handle " + properties.ModelType + " " + reader.GetAttribute("name"));
			if( reader.IsEmptyElement) { return; }
			log.Indent();
			while( reader.Read()) {
			    // Print out info on node  
			    switch( reader.NodeType) {
			    	case XmlNodeType.Element:
			    		if( "property".Equals(reader.Name) ) {
			    			string name = reader.GetAttribute("name");
			    			string value = reader.GetAttribute("value");
			    			string start = reader.GetAttribute("start");
			    			string end = reader.GetAttribute("end");
			    			string increment = reader.GetAttribute("increment");
			    			string optimize = reader.GetAttribute("optimize");
			    			if( "true".ToLower().Equals(optimize)) {
			    				properties.AddProperty(name,value,start,end,increment,optimize);
			    			} else {
			    				properties.AddProperty(name,value);
			    			}
							log.Debug("Property " + name + " = " + value);
			    		} else if( "strategy".Equals(reader.Name)) {
			    			ModelPropertiesCommon newProperties = new ModelPropertiesCommon();
			    			String name = reader.GetAttribute("name");
				    		HandleModel(newProperties,reader,TickZoom.Api.ModelType.Strategy);
				    		properties.AddModel(name,newProperties);
			    		} else if( "indicator".Equals(reader.Name)) {
			    			ModelPropertiesCommon newProperties = new ModelPropertiesCommon();
			    			String name = reader.GetAttribute("name");
				    		HandleModel(newProperties,reader,TickZoom.Api.ModelType.Indicator);
				    		properties.AddModel(name,newProperties);
			    		} else if( "propertyset".Equals(reader.Name)) {
			    			ModelPropertiesCommon newProperties = new ModelPropertiesCommon();
			    			String name = reader.GetAttribute("name");
				    		HandleModel(newProperties,reader,TickZoom.Api.ModelType.Model);
				    		properties.AddModel(name,newProperties);
			    		} else {
			    			Error(reader,"unexpected tag " + reader.Name );
			    		}
			    		break;
			    	case XmlNodeType.EndElement:
			    		if( tagName.Equals(reader.Name)) {
			    			log.Outdent();
				    		return;
			    		} else {
			    			Error(reader,"End of " + tagName + " was expected instead of end of " + reader.Name);
			    		}
			    		break;
			    } 
			}
			Error(reader,"Unexpected end of file");
			return;
		}
		
		private void HandleMethod( object obj, string name, string str) {
			MethodInfo method = obj.GetType().GetMethod(name, new Type[] { typeof(string) } );
			if( method == null) {
				throw new ApplicationException( obj + " does not have the method: " + name);
			}
			method.Invoke(obj,new object[] { str } );
			log.Debug("Method " + method.Name + "(" + str + ")");
		}
		
		private void HandleProperty( object obj, string name, string str) {
			PropertyInfo property = obj.GetType().GetProperty(name);
			if( property == null) {
				throw new ApplicationException( obj + " does not have the property: " + name);
			}
			Type propertyType = property.PropertyType;
			object value = TickZoom.Api.Converters.Convert(propertyType,str);
			property.SetValue(obj,value,null);
			log.Debug("Property " + property.Name + " = " + value);
		}
		
		public TickZoom.Api.StarterProperties Starter {
			get { return starterProperties; }
			set { value.CopyProperties(this); }
		}
		
		public TickZoom.Api.ChartProperties Chart {
			get { return chartProperties; }
			set { value.CopyProperties(this); }
		}
		
		public TickZoom.Api.EngineProperties Engine {
			get { return engineProperties; }
			set { value.CopyProperties(this); }
		}
		
		public TickZoom.Api.ModelProperties Model {
			get { return modelProperties; }
			set { modelProperties = value.Clone(); }
		}
	}
}