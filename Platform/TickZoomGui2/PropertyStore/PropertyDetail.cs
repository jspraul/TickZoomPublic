using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;

namespace TickZoom
{
	public class PropertyDetail
	{
		private Attribute[] attributes;
		private string category;
		private object defaultValue;
		private string description;
		private string editor;
		private string name;
		private Type type;
		private string typeConverter;
		private bool isReadOnly = false;
	
		public PropertyDetail(string name, Type type) : this(name, type, null, null, null) { }
	
		public PropertyDetail(string name, Type type, string category) : this(name, type, category, null, null) { }
	
		public PropertyDetail(string name, Type type, string category, string description) :
			this(name, type, category, description, null) { }
	
		public PropertyDetail(string name, Type type, string category, string description, object defaultValue)
		{
			this.name = name;
			this.type = type;
			this.category = category;
			this.description = description;
			this.defaultValue = defaultValue;
			this.attributes = null;
		}
	
		public PropertyDetail(string name, Type type, string category, string description, object defaultValue,
			string editor, string typeConverter) : this(name, type, category, description, defaultValue)
		{
			this.editor = editor;
			this.typeConverter = typeConverter;
		}
	
		public PropertyDetail(string name, Type type, string category, string description, object defaultValue,
			Type editor, string typeConverter) :
			this(name, type, category, description, defaultValue, editor.AssemblyQualifiedName,
			typeConverter) { }
	
		public PropertyDetail(string name, Type type, string category, string description, object defaultValue,
			string editor, Type typeConverter) :
			this(name, type, category, description, defaultValue, editor, typeConverter.AssemblyQualifiedName) { }
	
		public PropertyDetail(string name, Type type, string category, string description, object defaultValue,
			Type editor, Type typeConverter) :
			this(name, type, category, description, defaultValue, editor.AssemblyQualifiedName,
			typeConverter.AssemblyQualifiedName) { }
	
		public Attribute[] Attributes
		{
			get { return attributes; }
			set { attributes = value; }
		}
	
		public string Category
		{
			get { return category; }
			set { category = value; }
		}
	
		/// <summary>
		/// Gets or sets the fully qualified name of the type converter
		/// type for this property.
		/// </summary>
		public string ConverterTypeName
		{
			get { return typeConverter; }
			set { typeConverter = value; }
		}
	
		public object DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}
	
		public string Description
		{
			get { return description; }
			set { description = value; }
		}
	
		public string EditorTypeName
		{
			get { return editor; }
			set { editor = value; }
		}
	
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
	
		public string TypeName
		{
			get { return type.AssemblyQualifiedName; }
			set { type = Type.GetType(value); }
		}
		
		public Type Type
		{
			get { return type; }
			set { type = value; }
		}
		
		public bool IsReadOnly {
			get { return isReadOnly; }
			set { isReadOnly = value; }
		}
	}
}
