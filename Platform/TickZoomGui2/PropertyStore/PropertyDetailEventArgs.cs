using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;

namespace TickZoom
{
	public class PropertyDetailEventArgs : EventArgs
	{
		private PropertyDetail property;
		private object val;
	
		public PropertyDetailEventArgs(PropertyDetail property, object val)
		{
			this.property = property;
			this.val = val;
		}
	
		public PropertyDetail Property
		{
			get { return property; }
		}
	
		public object Value
		{
			get { return val; }
			set { val = value; }
		}
	}
}
