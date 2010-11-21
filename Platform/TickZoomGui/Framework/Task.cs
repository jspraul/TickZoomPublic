namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;


    
    public interface Task
    {
        #region Properties

        object Result
        {
            get;
        }

        #endregion Properties

        #region Methods

        void Execute();

        #endregion Methods
    }
}