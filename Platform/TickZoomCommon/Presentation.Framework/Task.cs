namespace TickZoom.Presentation.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;


    
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