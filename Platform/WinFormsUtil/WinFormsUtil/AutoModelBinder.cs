using System.Text;
using TickZoom.Presentation.Framework;

namespace TickZoom.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;

    using TickZoom.Api;

    public static class AutoModelBinder
    {
        #region Fields

        private static readonly Log log = Factory.SysLog.GetLogger(typeof(AutoModelBinder));
        private static readonly bool debug = log.IsDebugEnabled;
        private static readonly bool trace = log.IsTraceEnabled;

        #endregion Fields

        #region Methods

        public static void Bind(object viewModel, object view)
        {
            var viewModelType = viewModel.GetType();
            var properties = new List<PropertyInfo>(viewModelType.GetProperties());
            var methods = viewModelType.GetMethods();

            BindCommands(viewModel, view, methods, properties);
            BindProperties(viewModel, view, properties);
        }

        private static void BindCommands(object viewModel, object view, IEnumerable<MethodInfo> methods, List<PropertyInfo> properties)
        {
            foreach(var method in methods)
            {
                if( method.IsSpecialName ||
                   method.ReturnType != typeof(void) ||
                   method.GetParameters().Length > 0 ||
                   !method.IsPublic) {
                    continue;
                }

                var foundControl = GetControl( view, method.Name);
                if(foundControl == null) {
                	if( debug) log.Debug("No button found for " + viewModel.GetType().Name + "." + method.Name + "()");
                    continue;
                }

                var canMethodName = "Can" + method.Name;
                var foundProperty = properties.FirstOrDefault(x => x.Name == canMethodName);
                if( foundProperty == null) {
                	if( debug) log.Debug( canMethodName + "() was not found on class " + viewModel.GetType().Name);
                } else {
                    properties.Remove(foundProperty);
                }

                var command = new ReflectiveCommand(viewModel, method, foundProperty);
                if( TrySetCommand(foundControl, command)) {
                	if(debug) log.Debug( view.GetType().Name + "." + foundControl.Name + " => " + viewModel.GetType().Name + "." + method.Name);
                } else {
                    log.Error( "Failed binding " + view.GetType().Name + "." + foundControl.Name + " to " + viewModel.GetType().Name + "." + foundProperty.Name);
                }
            }
        }

        private static void BindProperties(object viewModel, object view, IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
            	if( typeof(Delegate).IsAssignableFrom(property.PropertyType)) {
	                var foundMethod = GetMethod( view, property.Name);
	                if( foundMethod == null) {
	                	if( debug) log.Debug("Unabled to find " + view.GetType().Name + "." + property.Name + "() to assign to " + viewModel.GetType().Name + "." + property.Name);
	                } else {
	                    var action = Delegate.CreateDelegate(property.PropertyType,view,property.Name);
	                    property.SetValue(viewModel, action, null);
	                    if( debug) log.Debug( view.GetType().Name + "." + property.Name + "() => " + viewModel.GetType().Name + "." + property.Name);
	                }
	                continue;
            	}

                var foundControl = GetControl( view, property.Name);
                if(foundControl == null) {
                	if( debug) log.Debug("No control found for " + viewModel.GetType().Name + "." + property.Name);
                    continue;
                }

	            if( foundControl.DataBindings.Count > 0) {
                	if( debug) log.Debug( view.GetType().Name + "." + foundControl.Name + " was already bound.");
                	continue;
	            }
                
                if( foundControl is TextBox) {
                    TryBind(foundControl,"Text",viewModel,property.Name);
                } else if( foundControl is Label) {
                    TryBind(foundControl,"Text",viewModel,property.Name);
                } else if( foundControl is ComboBox) {
                    if( property.PropertyType.IsEnum) {
	                    TryBind(foundControl,"Text",viewModel,property.Name);
                        var comboBox = foundControl as ComboBox;
                        comboBox.DataSource = Enum.GetValues(property.PropertyType);
                        if( debug) log.Debug( "DataSource => enum values of " + viewModel + "." + property.Name);
                    } else if( property.PropertyType == typeof(string)) {
	                    TryBind(foundControl,"Text",viewModel,property.Name);
                        var comboBox = foundControl as ComboBox;
                        var valuesPropertyName = property.Name + "Values";
                        var foundProperty = properties.FirstOrDefault(x => x.Name == valuesPropertyName);
                        if( foundProperty == null) {
                        	if( debug) log.Debug( "Values property not found for " + viewModel + "." + property.Name);
                        } else {
                        	comboBox.DataSource = foundProperty.GetValue(viewModel,null);
                        	if( debug) log.Debug( "DataSource => " + viewModel + "." + foundProperty.Name);
                        }
                    } else {
                		if( debug) log.Debug( "DataSource was not set on control for " + viewModel + "." + property.Name);
                    }
                } else if( foundControl is DateTimePicker) {
                    TryBind(foundControl,"Value",viewModel,property.Name);
                } else if( foundControl is ProgressBar) {
                    TryBind(foundControl,"Value",viewModel,property.Name);
                } else if( foundControl is CheckBox) {
                    TryBind(foundControl,"Checked",viewModel,property.Name);
                } else {
                    log.Error("Unknown control type for " + view.GetType().Name +"."+ foundControl.Name + " of type " + foundControl.GetType().Name);
                    continue;
                }

            }
        }

        private static Control GetControl( object view, string name)
        {
            var propertyInfo = view.GetType().GetProperty(name);
            object propertyValue = null;
            if( propertyInfo != null) {
                propertyValue = propertyInfo.GetValue( view, null);
            }
            return propertyValue as Control;
        }

        private static MethodInfo GetMethod( object view, string name)
        {
            return view.GetType().GetMethod(name);
        }

        private static void TryBind(Control foundControl, string controlPropertyName, object viewModel, string propertyName)
        {
        	if( typeof(CheckBox).IsAssignableFrom(foundControl.GetType())) {
	            foundControl.DataBindings.Add(controlPropertyName, viewModel, propertyName, true, DataSourceUpdateMode.OnPropertyChanged);
        	} else {
	            foundControl.DataBindings.Add(controlPropertyName, viewModel, propertyName);
        	}
        	if( debug) log.Debug( foundControl.Name + "." + controlPropertyName + " => " + viewModel.GetType().Name + "." + propertyName);
        }

        private static bool TrySetCommand(object control, CommandInterface command)
        {
            return TrySetCommandBinding<ButtonBase>(control, command);
        }

        private static bool TrySetCommandBinding<T>(object control, CommandInterface command)
            where T : Control
        {
            var commandSource = control as T;
            commandSource.DataBindings.Add("Enabled", command, "CanExecute");
            commandSource.Click += delegate { command.Execute(); };
            return true;
        }

        #endregion Methods
    }
}