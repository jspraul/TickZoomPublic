namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;

    public static class ViewModelBinder
    {
        #region Methods

        public static void Bind(object viewModel, object view)
        {
            var viewType = viewModel.GetType();
            var properties = viewType.GetProperties();
            var methods = viewType.GetMethods();

            BindCommands(viewModel, view, methods, properties);
            BindProperties(viewModel, view, properties);
        }

        private static void BindCommands(object viewModel, object view, IEnumerable<MethodInfo> methods, IEnumerable<PropertyInfo> properties)
        {
            foreach(var method in methods)
            {
                var foundControl = GetControl( view, method.Name);
                if(foundControl == null)
                    continue;

                var foundProperty = properties
                    .FirstOrDefault(x => x.Name == "Can" + method.Name);

                var command = new ReflectiveCommand(viewModel, method, foundProperty);
                TrySetCommand(foundControl, command);
            }
        }

        private static void BindProperties(object viewModel, object view, IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                var foundControl = GetControl( view, property.Name);
                if(foundControl == null)
                    continue;

                string controlPropertyName;
                if( foundControl is TextBox) {
                    controlPropertyName = "Text";
                } else {
                    throw new ApplicationException("Unknow control type.");
                }

                if( foundControl.DataBindings.Count == 0) {
                    foundControl.DataBindings.Add(controlPropertyName, viewModel, property.Name);
                }

            //                var textBox = foundControl as TextBox;
            //                if(textBox != null)
            //                {
            //                    textBox.TextChanged += delegate { textBox.DataBindings..UpdateSource(); };
            //                    continue;
            //                }

            //                var itemsControl = foundControl as ItemsControl;
            //                if(itemsControl != null && string.IsNullOrEmpty(itemsControl.DisplayMemberPath) && itemsControl.ItemTemplate == null)
            //                {
            //                    itemsControl.ItemTemplate = _defaultTemplate;
            //                    continue;
            //                }
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

        private static void TrySetCommand(object control, Command command)
        {
            TrySetCommandBinding<ButtonBase>(control, command);
        }

        private static bool TrySetCommandBinding<T>(object control, Command command)
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