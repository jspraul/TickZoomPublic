namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    public interface CommandInterface : INotifyPropertyChanged
    {
        #region Properties

        bool CanExecute
        {
            get;
        }

        #endregion Properties

        #region Methods

        void Execute();

        #endregion Methods
    }
}