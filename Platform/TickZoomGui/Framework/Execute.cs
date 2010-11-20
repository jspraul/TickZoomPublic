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

    public static class Execute
    {
        #region Fields

        private static List<Task> tasks = new List<Task>();
        private static object tasksLocker = new object();

        #endregion Fields

        #region Methods

        public static void MessageLoop()
        {
            while( tasks.Count > 0) {
                lock( tasksLocker) {
                    var task = tasks[0];
                    task.Execute();
                    tasks.Remove(task);
                }
            }
        }

        public static void OnUIThread(Action action)
        {
            lock( tasksLocker) {
                tasks.Add( new SyncTask( action, () => { } ));
            }
        }

        public static T OnUIThread<T>(Delegate action)
        {
            var isComplete = false;
            var task = new SyncTask( action, () => isComplete = true );
            lock( tasksLocker) {
                tasks.Add( task);
            }
            while( !isComplete) {
                Thread.Sleep(1);
            }
            return (T) task.Result;
        }

        #endregion Methods
    }

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