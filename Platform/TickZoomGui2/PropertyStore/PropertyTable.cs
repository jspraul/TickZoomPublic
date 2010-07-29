using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Xml;

using TickZoom.Api;

namespace TickZoom
{
	
	public class OptimizeTypeEditor : UITypeEditor {
		public OptimizeTypeEditor() {
			
		}
	}
	/// <summary>
	/// An extension of PropertyBag that manages a table of property values, in
	/// addition to firing events when property values are requested or set.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class PropertyTable : PropertyStore
	{
		private enum State {
			Begin,
			AfterInitialize,
			FromProjectFile
		};
		State state = State.Begin;
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		Dictionary<string,object> values;
		string name;
		object value;

		/// <summary>
		/// Initializes a new instance of the PropertyTable class.
		/// </summary>
		public PropertyTable(object obj)
		{
			value = obj;
			values = new Dictionary<string,object>();
			if( typeof(ModelInterface).IsAssignableFrom(obj.GetType())) {
				name = ((ModelInterface) obj).Name;
			}
			ExpandBrowsableProperties(obj);
		}
	
		public object this[string key]
		{
			get { return values[key]; }
			set { values[key] = value; }
		}
	
		public void UpdateAfterInitialize() {
			state = State.AfterInitialize;
			ExpandBrowsableProperties(value);
			state = State.Begin;
		}
		
		public void UpdateAfterProjectFile() {
			state = State.FromProjectFile;
			ExpandBrowsableProperties(value);
			state = State.Begin;
		}
		
		public override object GetValue(PropertyDetail item)
		{
			return values[item.Name];
		}
	
		public override void SetValue(PropertyDetail item, object value)
		{
			values[item.Name] = value;
		}

		private void ExpandBrowsableProperties(object obj) {
			Type type = obj.GetType();
			PropertyInfo[] properties = type.GetProperties();
			for( int i=0; i<properties.Length; i++) {
				PropertyInfo property = properties[i];
				string name = property.Name;
				object[] attributes = property.GetCustomAttributes(true);
				bool browsable = true;
				string category = null;
				string description = null;
				string editor = null;
				string typeConverter = null;
				for( int j=0; j<attributes.Length; j++) {
					BrowsableAttribute browsableAttr = attributes[j] as BrowsableAttribute;
					if( browsableAttr != null) {
						browsable = browsableAttr.Browsable;
					}
					CategoryAttribute categoryAttr = attributes[j] as CategoryAttribute;
					if( categoryAttr != null) {
						category = categoryAttr.Category;
					}
					DescriptionAttribute descriptionAttr = attributes[j] as DescriptionAttribute;
					if( descriptionAttr != null) {
						description = descriptionAttr.Description;
					}
				}
				
				if( browsable) {
					Type propertyType = property.PropertyType;
					attributes = propertyType.GetCustomAttributes(true);
					for( int j=0; j<attributes.Length; j++) {
						EditorAttribute editorAttr = attributes[j] as EditorAttribute;
						if( editorAttr != null) {
							editor = editorAttr.EditorTypeName;
						}
						TypeConverterAttribute typeConverterAttr = attributes[j] as TypeConverterAttribute;
						if( typeConverterAttr != null) {
							typeConverter = typeConverterAttr.ConverterTypeName;
						}
					}
					// skip the indexer
					if( property.Name.Equals("Item")) continue;
					if( property.Name.Equals("Chars")) continue;
					// skip non-getter properties
					if( property.GetGetMethod() == null) continue;
					object value = null;
					try { 
						value = property.GetValue(obj,null);
					} catch( TargetInvocationException) {
						log.Debug("Unable to get value for " + property.Name + " on type = " + type.FullName + " using default type of " + propertyType.Name);
						value = 0;
					}
					switch( state) {
						case State.Begin:
							SetupProperty(obj, name, propertyType, value, category, description, editor, typeConverter);
							break;
						case State.AfterInitialize:
							SetupPropertyAfterInitialize(obj, name, propertyType, value, category, description, editor, typeConverter);
							break;
						case State.FromProjectFile:
							SetPropertyFromProjectFile(obj, name, propertyType, value, category, description, editor, typeConverter);
							break;
					}
				}
			}
		}
		
		private void SetupProperty(object obj, string name, Type propertyType, object value, string category, string description, string editor, string typeConverter) {
			if( propertyType.FullName.Equals("TickZoom.Api.Elapsed") ) {
				editor = "ElapsedPropertyEditor";
				typeConverter = typeof(ElapsedTypeConverter).AssemblyQualifiedName;
			}
			if( propertyType == typeof(TimeStamp)) {
				editor = "TimestampPropertyEditor";
				typeConverter = typeof(TimestampTypeConverter).AssemblyQualifiedName;
			}
			if( propertyType == typeof(Interval)) {
				editor = "IntervalPropertyEditor";
				typeConverter = typeof(IntervalTypeConverter).AssemblyQualifiedName;
			}
			if( propertyType == typeof(Interval)) {
				editor = "IntervalPropertyEditor";
				typeConverter = typeof(IntervalTypeConverter).AssemblyQualifiedName;
			}
			if( HasTypeConverter(obj,propertyType, typeConverter)) {
				PropertyDetail detail = new PropertyDetail(name,propertyType,category,description,value,editor,typeConverter);
				this.Properties.Add(detail);
				this.DefaultProperty = name;
				values[name] = value;
			} else {
				if( value!=null && !value.Equals(obj)) {
					PropertyTable table = new PropertyTable(value);
					PropertyDetail detail = new PropertyDetail(name,table.GetType(),category,description);
					this.Properties.Add(detail);
					values[name] = table;
				}
			}
		}
		
		public static bool HasTypeConverter(object obj, Type propertyType, string typeConverter) {
			return 
				   propertyType.IsPrimitive ||
			       propertyType.IsEnum ||
				   propertyType == typeof(string) ||
				   typeConverter != null ||
				   propertyType == typeof(Color) ||
				   propertyType == typeof(TimeStamp) ||
				   propertyType == typeof(Interval) ||
				   propertyType == typeof(Elapsed);
		}
		
		private Interval emptyInterval = Factory.Engine.DefineInterval(BarUnit.Default,0);
		
		private void HandleReadOnly( object value, PropertyDetail detail) {
			if( value == null || !value.Equals(detail.DefaultValue)) {
				detail.IsReadOnly = true;
				detail.Description = "READ ONLY: Properties that you set in OnInitialize() are read only to the GUI because project settings apply before and get overwritten by OnInitialize().";
			}
		}
		
		private void SetupPropertyAfterInitialize(object obj, string name, Type propertyType, object value, string category, string description, string editor, string typeConverter) {
			if( HasTypeConverter(obj,propertyType, typeConverter))	{
				PropertyDetail spec = Properties[Properties.IndexOf(name)];
				if( propertyType == typeof(Interval) ) {
					spec.DefaultValue = value;
				} else {
					HandleReadOnly(value,spec);
				}
				values[name] = value;
			} else {
				if( value!=null && value!=obj) {
					PropertyDetail spec = Properties[Properties.IndexOf(name)];
					PropertyTable table = (PropertyTable) values[name];
					table.UpdateAfterInitialize();
				}
			}
		}
		
		private void SetPropertyFromProjectFile(object obj, string name, Type propertyType, object value, string category, string description, string editor, string typeConverter) {
			if( HasTypeConverter(obj,propertyType, typeConverter))	{
				PropertyDetail spec = Properties[Properties.IndexOf(name)];
				values[name] = value;
			} else {
				if( value!=null && value!=obj) {
					PropertyDetail spec = Properties[Properties.IndexOf(name)];
					PropertyTable table = (PropertyTable) values[name];
					table.UpdateAfterProjectFile();
				}
			}
		}
		
		public override string ToString()
		{
			return name;
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public object Value {
			get { return value; }
		}
		
#region VoidContext
		public class VoidContext : ITypeDescriptorContext {
			public IContainer Container {
				get {
					throw new NotImplementedException();
				}
			}
			
			public object Instance {
				get {
					throw new NotImplementedException();
				}
			}
			
			public PropertyDescriptor PropertyDescriptor {
				get {
					throw new NotImplementedException();
				}
			}
			
			public bool OnComponentChanging()
			{
				throw new NotImplementedException();
			}
			
			public void OnComponentChanged()
			{
				throw new NotImplementedException();
			}
			
			public object GetService(Type serviceType)
			{
				throw new NotImplementedException();
			}
		}
#endregion

		public string Convert( string converterName, object obj) {
			Assembly assembly = Assembly.GetAssembly(this.GetType());
			CultureInfo cultureInfo = assembly.GetName().CultureInfo;
			Type type = Type.GetType(converterName);
			object converter = Activator.CreateInstance(type);
			MethodInfo methodInfo = converter.GetType().GetMethod("ConvertTo",new Type[] { typeof( VoidContext), typeof(CultureInfo), typeof(object), typeof(Type) } );
			string str = (string) methodInfo.Invoke(converter,new object[] { new VoidContext(), cultureInfo, obj, typeof(string) });
			return str;
		}
		
		public void Serialize( XmlWriter writer) {
			foreach( PropertyDetail detail in Properties) {
				PropertyTable properties = values[detail.Name] as PropertyTable;
				if( properties != null) {
					writer.WriteStartElement("propertyset");
					writer.WriteAttributeString("name",detail.Name.ToLower());
					log.Debug( "Reiteritive " + detail.Name.ToLower());
					properties.Serialize(writer);
					writer.WriteEndElement();
				} else {
					if( !detail.IsReadOnly && !values[detail.Name].Equals(detail.DefaultValue) ) {
						string value = "";
						if( detail.ConverterTypeName == null) {
							value = values[detail.Name].ToString();
						} else {
							value = Convert(detail.ConverterTypeName,values[detail.Name]);
						}
				    	writer.WriteStartElement("property");
				    	writer.WriteAttributeString("name",detail.Name);
				    	writer.WriteAttributeString("value",value);
	//			    	if( detail.ConverterTypeName != null && detail.ConverterTypeName.Length > 0) {
	//			    		writer.WriteAttributeString("converter",detail.ConverterTypeName);
	//			    	}
					    writer.WriteEndElement();
					}
				}
			}
		}
	}
}
