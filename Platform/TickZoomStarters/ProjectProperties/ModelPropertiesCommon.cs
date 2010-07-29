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
using System.IO;
using System.Xml;

using TickZoom.Api;

namespace TickZoom.Properties
{
	public class ModelPropertiesCommon : ModelProperties
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Dictionary<string, ModelProperty> properties = new Dictionary<string, ModelProperty>();
		Dictionary<string, ModelProperties> models = new Dictionary<string, ModelProperties>();
		ModelType modelType = ModelType.None;
		string name;
		string type;
		
		public ModelProperties Clone() {
			ModelPropertiesCommon model = new ModelPropertiesCommon();
			model.ModelType = modelType;
			model.Name = name;
			model.Type = type;
			foreach(KeyValuePair<string, ModelProperty> kvp in properties) {
				model.AddProperty(kvp.Key,kvp.Value.Clone());
			}
			foreach(KeyValuePair<string, ModelProperties> kvp in models) {
				model.AddModel(kvp.Key,kvp.Value.Clone());
			}
			return model;
		}
		
		public void AddProperty(string name, string value)
		{
			properties.Add(name, new ModelPropertyCommon(name,value));
		}

		public void AddProperty(string name, string value, string startStr, string endStr, string incrementStr, string optimizeStr)
		{
			double start = Convert.ToDouble(startStr);
			double end = Convert.ToDouble(endStr);
			double increment = Convert.ToDouble(incrementStr);
			bool optimize = Convert.ToBoolean(optimizeStr);
			
			properties.Add(name, new ModelPropertyCommon(name,value,start,end,increment,optimize));
		}
		
		public void AddProperty(string name, ModelProperty property)
		{
			properties.Add(name, property);
		}
		
		public void AddModel(string name, ModelProperties model)
		{
			models.Add(name, model);
		}
		
		public ModelProperties GetModel(string key) {
			return models[key];
		}

		public ModelProperty GetProperty(string key) {
			return properties[key];
		}
		
		public string[] GetPropertyKeys()
		{
			string[] keyArray = new string[properties.Keys.Count];
			properties.Keys.CopyTo(keyArray, 0);
			return keyArray;
		}

		public string[] GetModelKeys()
		{
			string[] keyArray = new string[models.Keys.Count];
			models.Keys.CopyTo(keyArray, 0);
			return keyArray;
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public string Type {
			get { return type; }
			set { type = value; }
		}
		
		public ModelType ModelType {
			get { return modelType; }
			set { modelType = value; }
		}
	}
}
