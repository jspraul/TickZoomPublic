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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Description of Starter.
	/// </summary>
	public abstract class ModelLoaderCommon : ModelLoaderInterface
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		IList<Model> models = new List<Model>();
		List<ModelProperty> variables = new List<ModelProperty>();
		List<OptimizeRange> rules = new List<OptimizeRange>();
		bool isVisibleInGUI = true;
		bool quietMode = false;
		ModelInterface topModel;
		Stream optimizeOutput;
		
		public ModelLoaderCommon()
		{ 
			name = this.GetType().Name;
		}
		
		public virtual void OnInitialize(ProjectProperties model) {
		}
		
		public abstract void OnLoad(ProjectProperties model);
		
		[Obsolete("Fallen into disuse.",true)]
		public IList<ModelInterface> Models {
			get { return null; }
		}

		public void OnClear() {
			models.Clear();
		}
		
		public ModelInterface CreateModel( string type, string name) {
			Model model = CreateModel(type);
			model.Name = name;
			return model;
		}
		 
		public Strategy CreateStrategy( string type, string name) {
			Model model = CreateStrategy(type);
			model.Name = name;
			return model as Strategy;
		}
		
		public Strategy CreateStrategy( string type) {
			Model model = CreateModel( type);
			if( model is Strategy) {
				models.Add( model as Strategy);
				return model as Strategy;
			} else {
				if( model == null) {
					throw new ApplicationException("Name passed to CreateStrategy() was not found: " + type);
				} else {
					throw new ApplicationException("Name passed to CreateStrategy() was of type " + model.GetType().Name + " instead of a strategy. Perhaps, try CreatePortfolio() instead.");
				}
			}
		}

		public Portfolio CreatePortfolio( string type, string name) {
			Model model = CreatePortfolio(type);
			model.Name = name;
			return model as Portfolio;
		}
		
		public Portfolio CreatePortfolio( string type) {
			Model model = CreateModel( type);
			if( model is Portfolio) {
				models.Add( model as Portfolio);
				return model as Portfolio;
			} else {
				throw new ApplicationException("Name passed to CreatePortfolio() was " + model.GetType().Name + " instead of a portfolio. Perhaps, try CreateStrategy() instead.");
			}
		}
		
		public Model CreateModel( string type) {
			Model model = null;
			for( int i = 0; i< models.Count; i++) {
				if( models[i].Name == type) {
					if( models[i].GetType().Name != type) {
						throw new ApplicationException("Model already exists with name " + name + " with different type");
					}
					model = models[i];
				}
			}
			if( model == null) {
				try { 
					model = Plugins.Instance.GetModel(type) as Model;
					models.Add(model);
				} catch( Exception ex) {
					log.Error( "Please make sure " + type + " exists and has a default constructor. ", ex);
				}
			}
			return model;
		}
		
		public Strategy GetStrategy( string name) {
			ModelInterface model = GetModelInternal( name);
			if( model is Strategy) {
				return (Strategy) model;
			} else {
				throw new InvalidCastException( "'" + name + "' is not a strategy. Perhaps, try GetPortfolio() method instead.");
			}
		}
		
		public Portfolio GetPortfolio( string name) {
			ModelInterface model = GetModelInternal( name);
			if( model is Portfolio) {
				return (Portfolio) model;
			} else {
				throw new InvalidCastException( "'" + name + "' is not a portfolio. Perhaps, try GetStrategy() method instead.");
			}
		}
		
		[Obsolete("Please use GetStrategy() instead",true)]
		public ModelInterface GetModel( string name) {
			return GetModelInternal(name);
		}
		
		private ModelInterface GetModelInternal( string name) {
			ModelInterface model;
			for( int i = 0; i< models.Count; i++) {
				if( models[i].Name == name) {
					return models[i];
				}
			}
			model = CreateModel( name);
			if( model != null) {
				return model;
			}
			throw new ApplicationException( "Strategy name '"+name+"' was not found in GetStrategy()");
		}
		
		[Obsolete("Please use the new AddVariable method with the defaultValue argument. This value will be replaced for the optimize variable when the last isActive argument is false.",true)]
		public void AddVariable(string name, double start, double end, double increment, bool isActive) {
			throw new NotImplementedException();
		}
		
		public void AddVariable(string name, double start, double end, double increment, double defaultValue, bool isActive) {
			ModelProperty property = Factory.Starter.ModelProperty(name,defaultValue.ToString(),start,end,increment,isActive);
			AddVariable(property);
		}
		
		public void AddDependency( string current, string previous) {
			Model previousStrategy = null;
			for( int i = 0; i< models.Count; i++) {
				if( models[i].Name == previous) {
					previousStrategy = models[i];
				}
			}
			if( previousStrategy == null) {
				previousStrategy = CreateStrategy( previous);
			}
			if( previousStrategy == null) {
				throw new ApplicationException( "Parent Strategy '" + previous + "' was not found for AddDependency()");
			}
			AddDependency( current, previousStrategy);
		}
		
		public void AddDependency( string current, Model previousStrategy) {
			Model currentStrategy = null;
			for( int i = 0; i< models.Count; i++) {
				string name = models[i].Name;
				if( name == current) {
					currentStrategy = models[i];
				}
			}
			if( currentStrategy == null) {
				currentStrategy = CreateModel( current);
			}
			if( currentStrategy == null) {
				throw new ApplicationException( "Child Strategy '" + current + "' was not found for AddDependency()");
			}
			AddDependency(currentStrategy,previousStrategy);
		}
		
		public void AddDependency( ModelInterface currentStrategy, ModelInterface previousStrategy) {
			currentStrategy.Chain.Dependencies.Add(previousStrategy.Chain.Root);
		}
		                          
		public void Chain( string current, string previous) {
			ModelInterface currentStrategy = null;
			ModelInterface previousStrategy = null;
			for( int i = 0; i< models.Count; i++) {
				if( models[i].Name == current) {
					currentStrategy = models[i];
				}
				if( models[i].Name == previous) {
					previousStrategy = models[i];
				}
			}
			if( currentStrategy == null) {
				throw new ApplicationException( "Child Strategy '" + current + "' was not found for AddDependency()");
			}
			if( previousStrategy == null) {
				throw new ApplicationException( "Parent Strategy '" + previous + "' was not found for AddDependency()");
			}
			currentStrategy.Chain.Dependencies.Add(previousStrategy.Chain);
		}
		
		public void AddVariable( ModelProperty property) {
			variables.Add(property);
		}

		public List<ModelProperty> Variables {
			get { return variables; }
		}
		
		protected string name;
		public string Name {
			get { return category + ": " + name; } 
		}
		
		protected string category;
		public string Category {
			get { return category; }
		}
		
		public bool IsVisibleInGUI {
			get { return isVisibleInGUI; }
			set { isVisibleInGUI = value; }
		}
		
		public bool QuietMode {
			get { return quietMode; }
			set { quietMode = value; }
		}
		
		public ModelInterface TopModel {
			get { return topModel; }
			set { topModel = value; }
		}
		
		public Stream OptimizeOutput {
			get { return optimizeOutput; }
			set { optimizeOutput = value; }
		}
	}
}
