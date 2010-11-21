namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    public static class Execute
    {
        #region Fields

        private static long busycount = 0;
        private static long loopcount = 0;
        private static List<Task> tasks = new List<Task>();
        private static List<Loop> loops = new List<Loop>();
        private static object tasksLocker = new object();

        #endregion Fields

        #region Methods

        public static bool AppIsIdle()
        {
            peek_message msg;
              return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
        }

        public static void MessageLoop(object sender, EventArgs e)
        {
            while( AppIsIdle()) {
                Interlocked.Increment( ref loopcount);
                if( tasks.Count > 0) {
                    lock( tasksLocker) {
                        var task = tasks[0];
                        task.Execute();
                        tasks.Remove(task);
                        Interlocked.Increment( ref busycount);
                    }
                }
                for( int i=0; i<loops.Count; i++) {
                	var loop = loops[i];
                	if( loop.Execute()) {
                        Interlocked.Increment( ref busycount);
                	}
                }
                if( loopcount > 1000) {
                    TrySleep();
                }
            }
            Interlocked.Increment( ref busycount);
        }

        public static void OnUIThread(Func<bool> function)
        {
            lock( tasksLocker) {
                loops.Add( new Loop( function));
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

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        public static extern bool PeekMessage(
            out peek_message msg,
            IntPtr hWnd,
            uint messageFilterMin,
            uint messageFilterMax,
            uint flags
            );

        public static void TrySleep()
        {
            if( busycount == 0) {
                Thread.Sleep(1);
            }
            Interlocked.Exchange(ref loopcount, 0);
            Interlocked.Exchange(ref busycount, 0);
        }

        #endregion Methods

        #region Nested Types

        [StructLayout(LayoutKind.Sequential)]
        public struct peek_message
        {
            public IntPtr hWnd;
            public Message msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        #endregion Nested Types
    }
}