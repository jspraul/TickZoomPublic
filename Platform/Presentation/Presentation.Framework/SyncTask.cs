using System.Reflection;
using TickZoom.Api;

namespace TickZoom.Presentation.Framework
{
    using System;

    public class SyncTask : Task
    {
    	private Log log = Factory.SysLog.GetLogger(typeof(SyncTask));
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
        	try {
	        	result = execute.DynamicInvoke();
    	        complete();
        	} catch( TargetInvocationException ex) {
        		log.Error( ex.InnerException.Message, ex.InnerException);
        		throw ex.InnerException;
        	}
        }

        #endregion Methods
    }
}