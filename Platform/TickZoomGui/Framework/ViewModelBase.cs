namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;

    public class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public bool IsValid
        {
            get { return string.IsNullOrEmpty(Error); }
        }

        public string Error
        {
            get
            {
                return "";
            }
        }

        public string this[string propertyName]
        {
            get
            {
                var result = GetType().GetProperty(propertyName).GetValue(this, null);
                return result.ToString();
            }
        }

        public void NotifyOfPropertyChange(string propertyName)
        {
        	Execute.OnUIThread(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
        }

        public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;

            NotifyOfPropertyChange(memberExpression.Member.Name);
        }
    }
}