namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;

    public class SyncTask : Task
    {
        #region Fields

        private Action complete;
        private Delegate execute;
        private object result;

        #endregion Fields

        #region Constructors

        public SyncTask( Delegate execute, Action complete)
        {
            this.execute = execute;
            if( this.execute == null) {
            	throw new NullReferenceException("Execute argument must be a valid delegate.");
            }
            this.complete = complete;
        }

        #endregion Constructors

        #region Properties

        public object Result
        {
            get { return result; }
        }

        #endregion Properties

        #region Methods

        public void Execute()
        {
            result = execute.DynamicInvoke();
            complete();
        }

        #endregion Methods
    }
}