namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    public abstract class Command : ViewModelBase, CommandInterface
    {
        #region Properties

        public abstract bool CanExecute
        {
            get;
        }

        #endregion Properties

        #region Methods

        public abstract void Execute();

        #endregion Methods
    }
}