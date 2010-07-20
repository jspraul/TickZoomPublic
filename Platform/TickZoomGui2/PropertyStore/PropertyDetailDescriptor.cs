
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;

namespace TickZoom
{
	public class PropertyDetailDescriptor : PropertyDescriptor
	{
		private PropertyStore store;
		private PropertyDetail info;
	
		public PropertyDetailDescriptor(PropertyDetail item, PropertyStore store, string name, Attribute[] attrs) :
			base(name, attrs)
		{
			this.store = store;
			this.info = item;
		}
	
		public override Type ComponentType
		{
			get { return info.GetType(); }
		}
	
		public override bool IsReadOnly
		{
			get { return info.IsReadOnly; }
		}
	
		public override Type PropertyType
		{
			get { return Type.GetType(info.TypeName); }
		}
	
		public override bool CanResetValue(object component)
		{
			if(info.DefaultValue == null)
				return false;
			else
				return !this.GetValue(component).Equals(info.DefaultValue);
		}
	
		public override object GetValue(object component)
		{
			// Have the property bag raise an event to get the current value
			// of the property.
			return store.GetValue(info);
		}
	
		public override void ResetValue(object component)
		{
			SetValue(component, info.DefaultValue);
		}
	
		public override void SetValue(object component, object value)
		{
			// Have the property bag raise an event to set the current value
			// of the property.
	
			store.SetValue(info,value);
		}
	
		public override bool ShouldSerializeValue(object component)
		{
			object val = this.GetValue(component);
	
			if(info.DefaultValue == null && val == null)
				return false;
			else
				return !val.Equals(info.DefaultValue);
		}
	}
}
