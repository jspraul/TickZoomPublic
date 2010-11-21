namespace TickZoom.Presentation.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;

    public class AutoBindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        
        private Action<Action> callbackAction = (action) => {action();};
        
		public Action<Action> CallbackAction {
			get { return callbackAction; }
			set { callbackAction = value; }
		}

        public void NotifyOfPropertyChange(string propertyName)
        {
        	CallbackAction(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
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