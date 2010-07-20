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
using System.Windows.Forms;
using TickZoom.Api;
using TickZoom.Starters;

namespace TickZoom
{
	/// <summary>
	/// Description of Portfolio.
	/// </summary>
	public class PortfolioNode : TreeNode
	{
		public PortfolioNode(string name,ModelProperties properties) : base(name) {
			Tag = GetModel(name,properties);
		}
		public PortfolioNode(string name) : base(name) {
			Tag = GetModel(name,null);
		}
		
		public void Add( string name) {
			PortfolioNode node = new PortfolioNode(name);
			Nodes.Add(node);
		}
		
		public void Add( ModelProperties properties) {
			PortfolioNode node = new PortfolioNode(properties.Type,properties);
			node.Name = properties.Name;
			Nodes.Add(node);
		}
		
		private void LoadIndicators(ModelInterface model) {
			for( int i=0; i<model.Chain.Dependencies.Count; i++) {
				IndicatorInterface indicator = model.Chain.Dependencies[i].Model as IndicatorInterface;
				if( indicator != null) {
					Add( indicator.GetType().Name);
				}
			}
			
		}
		
		private object GetModel(string name,ModelProperties properties) {
			ModelInterface model = Plugins.Instance.GetModel(name);
			PropertyTable propertyTable = new PropertyTable(model);
			Starter starter = new DesignStarter();
			starter.Run(model);
			starter.Wait();
			propertyTable.UpdateAfterInitialize();
			if( properties != null) {
				model.OnProperties(properties);
				propertyTable.UpdateAfterProjectFile();
				string[] keys = properties.GetModelKeys();
				for( int i=0; i<keys.Length; i++) {
					ModelProperties nestedProperties = properties.GetModel(keys[i]);
					if( nestedProperties.ModelType == ModelType.Indicator) {
						Add( nestedProperties);
					}
				}
			} else {
				LoadIndicators(model);
			}
			return propertyTable;
		}
		
	}
}
