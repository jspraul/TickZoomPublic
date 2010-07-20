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
using System.Reflection;
using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of ProjectModelLoader.
	/// </summary>
	public class ProjectFileLoader : ModelLoaderCommon
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public ProjectFileLoader() {
			category = "TickZOOM";
			name = "Project File";
//			IsVisibleInGUI = false;
		}
		
		public override void OnInitialize(ProjectProperties properties) {
			FindVariables(properties.Model);
		}
		
		private void FindVariables(ModelProperties properties) {
			string[] propertyKeys = properties.GetPropertyKeys();
			for( int i=0; i<propertyKeys.Length; i++) {
				ModelProperty property = properties.GetProperty(propertyKeys[i]);
				AddVariable(property);
			}
			string[] keys = properties.GetModelKeys();
			for( int i=0; i<keys.Length; i++) {
				ModelProperties nestedProperties = properties.GetModel(keys[i]);
				FindVariables(nestedProperties);
			}
		}
		
		public override void OnLoad(ProjectProperties properties)
		{
			LoadModel( properties.Model);
		}
		
		private void LoadModel( ModelProperties properties) {
			if( !QuietMode) {
				log.Debug( properties.ModelType + " " + properties.Name + ", type = " + properties.Type);
			}
			ModelInterface model = CreateModel( properties.Type, properties.Name);
			model.OnProperties(properties);
			if( !QuietMode) {
				log.Indent();
			}
			string[] keys = properties.GetModelKeys();
			for( int i=0; i<keys.Length; i++) {
				ModelProperties nestedProperties = properties.GetModel(keys[i]);
				if( nestedProperties.ModelType == ModelType.Indicator) {
					
				} else {
					// type null for performance, positionsize, exitstrategy, etc.
					if( nestedProperties.Type == null)
					{
						HandlePropertySet( model, nestedProperties);
					} else {
						LoadModel(nestedProperties);
						Strategy nestedModel = TopModel as Strategy;
						nestedModel.OnProperties(nestedProperties);
						AddDependency( model, nestedModel);
					}
				}
			}
			if( !QuietMode) {
				log.Outdent();
			}
			TopModel = model;
		}
		
		private void HandlePropertySet( ModelInterface model, ModelProperties properties) {
			if( !QuietMode) {
				log.Debug( properties.Name);
				log.Indent();
			}
			
			if( "exitstrategy".Equals(properties.Name)) {
				Strategy strategy = (Strategy) model;
				strategy.ExitStrategy.OnProperties(properties);
			} else if( "performance".Equals(properties.Name)) {
				if( model is Portfolio) {
					Portfolio portfolio = (Portfolio) model;
					portfolio.Performance.OnProperties(properties);
				} else if( model is Strategy) {
					Strategy strategy = (Strategy) model;
					strategy.Performance.OnProperties(properties);
				} else {
					throw new ApplicationException("'" + model.Name + "' is neither a strategy nor a portfolio.");
				}
			} else {
				model.OnProperties(properties);
			}
			if( !QuietMode) {
				log.Outdent();
			}
		}
	}
}
