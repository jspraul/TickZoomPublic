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
using System.Windows.Forms;
using System.Xml;

using TickZoom.Api;

namespace TickZoom
{
	/// <summary>
	/// Description of PortfolioControl.
	/// </summary>
	public partial class PortfolioControl : UserControl
	{
		bool isInitialized = false;
		Log log;
		ProjectProperties projectProperties;
		ProjectDoc projectDoc;
		
		public PortfolioControl()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}

		void PortfolioControlLoad(object sender, EventArgs e)
		{
			this.projectDoc = (ProjectDoc) this.Parent.Parent.Parent;
			if( !DesignMode && !isInitialized) {
				log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
				projectProperties = new ProjectProperties();
				treeView.LabelEdit = true;
				string appData = Factory.Settings["AppDataFolder"];
				TickZoom.Api.ProjectProperties loadProjectProperties = Factory.Starter.ProjectProperties(appData + @"\portfolio.xml");
				ReloadProjectModels(loadProjectProperties);
			}
		}
		
		private void ReloadProjectModels(TickZoom.Api.ProjectProperties loadProjectProperties) {
			ModelProperties model = loadProjectProperties.Model;
            TreeNode project = new TreeNode("New Portfolio Project");
           	PropertyTable properties = new PropertyTable(projectProperties);
           	project.Tag = properties;
	        PortfolioNode node = ReloadPortfolio(model);
           	loadProjectProperties.Chart.CopyProperties(projectProperties.Chart);
           	loadProjectProperties.Starter.CopyProperties(projectProperties.Starter);
           	loadProjectProperties.Engine.CopyProperties(projectProperties.Engine);
           	properties.UpdateAfterProjectFile();
            project.Nodes.Add(node);
            this.treeView.Nodes.Add(project);
			this.treeView.ExpandAll();
			this.treeView.SelectedNode = project;
			isInitialized = true;
		}

		private PortfolioNode ReloadPortfolio(ModelProperties properties) {
			PortfolioNode portfolio = new PortfolioNode(properties.Type,properties);
			portfolio.Name = properties.Name;
			string[] keys = properties.GetModelKeys();
			for( int i=0; i<keys.Length; i++) {
				ModelProperties modelProperties = properties.GetModel(keys[i]);
				if( modelProperties.ModelType == ModelType.Indicator) {
				} else {
					// type null for performance, positionsize, exitstrategy, etc.
					if( modelProperties.Type == null)
					{
//						HandlePropertySet( model, modelProperties);
					} else {
						PortfolioNode node = ReloadPortfolio(modelProperties);
						portfolio.Nodes.Add(node);
					}
				}
			}
			return portfolio;
		}

		private void NewProject() {
            TreeNode project = new TreeNode("New Portfolio Project");
            PortfolioNode portfolio = null;
            try {
            	project.Tag = new PropertyTable(projectProperties);
	            portfolio = new PortfolioNode("PortfolioCommon");
            } catch( Exception ex) {
            	log.Debug(ex.ToString());
            }
            project.Nodes.Add(portfolio);
//	        portfolio.Add("ExampleReversalStrategy");
            portfolio.Add("ExampleSMAStrategy");
            this.treeView.Nodes.Add(project);
			this.treeView.ExpandAll();
			this.treeView.SelectedNode = project;
			isInitialized = true;
		}
		
		void TreeViewClick(object sender, EventArgs e)
		{
			
		}
		
		void TreeViewNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if( e.Button == MouseButtons.Right) {
				// Show a menu.
			}
			if( e.Button == MouseButtons.Left) {
				
			}
		}
		
		void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
		{
			if( projectDoc.MainForm.PropertyWindow.Visible) {
				projectDoc.MainForm.PropertyWindow.SelectedObject = e.Node.Tag;
				projectDoc.MainForm.PropertyWindow.PropertyGrid.PropertySort = PropertySort.NoSort;
				projectDoc.MainForm.PropertyWindow.PropertyGrid.ExpandAllGridItems();
			}
			WriteProject();
		}
		
		public void WriteProject() {
			TreeNode node = treeView.Nodes[0];
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = ("    ");
			string appData = Factory.Settings["AppDataFolder"];
			using (XmlWriter writer = XmlWriter.Create(appData + @"\portfolio.xml", settings))
			{
				writer.WriteStartDocument();
				SerializeNode(writer, node);
				writer.WriteEndDocument();
			    writer.Flush();
			    writer.Close();
			}
		}
		
		public void SerializeNode(XmlWriter writer, TreeNode node) {
			object obj = node.Tag;
			string modelName = obj.GetType().FullName;
	    	PropertyTable properties = obj as PropertyTable;
	    	if( properties != null) {
	    		if( typeof(IndicatorInterface).IsAssignableFrom(properties.Value.GetType())) {
			    	writer.WriteStartElement("indicator");
	    		} else if( typeof(StrategyInterceptor).IsAssignableFrom(properties.Value.GetType())) {
			    	writer.WriteStartElement("strategy");
	    		} else if( typeof(ProjectProperties).IsAssignableFrom(properties.Value.GetType())) {
			    	writer.WriteStartElement("projectproperties");
	    		} else {
			    	writer.WriteStartElement("model");
	    		}
		    	writer.WriteAttributeString("name",properties.Name);
	    		writer.WriteAttributeString("type",properties.Value.GetType().Name);
	    		properties.Serialize(writer);
	    	} else {
	    		throw new ApplicationException( "Found unexpected type in tree view for xml: " + obj.GetType());
	    	}
			for( int i=0; i<node.Nodes.Count; i++) {
				SerializeNode( writer, node.Nodes[i]);
			}
			writer.WriteEndElement();
		}
		
		void TreeViewNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{ 
			e.Node.BeginEdit();
		}
		
		void TreeViewAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if( e.Label != null) {
				ModelInterface model = Plugins.Instance.GetModel( e.Label);
				e.Node.Tag = model;
				projectDoc.MainForm.PropertyWindow.SelectedObject = e.Node.Tag;
			}
		}
		
		public ProjectProperties ProjectProperties {
			get { return projectProperties; }
		}
	}
}
