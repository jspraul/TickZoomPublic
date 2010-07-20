using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace TickZoom
{
    public partial class DummyPropertyWindow : ToolWindow
    {
        public DummyPropertyWindow()
        {
            InitializeComponent();
        }
        
        public object SelectedObject {
        	get { return propertyGrid.SelectedObject; }
        	set { propertyGrid.SelectedObject = value; }
        }
		
		void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			
		}
		
		void PropertyValueChanged(object sender, EventArgs e)
		{
			this.SelectedObject = this.SelectedObject;
		}
		
		void PropertyGridPropertySortChanged(object sender, EventArgs e)
		{
			if( propertyGrid.PropertySort == PropertySort.CategorizedAlphabetical) {
				propertyGrid.PropertySort = PropertySort.Categorized;
			}
		}
    }
}