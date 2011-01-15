using TickZoom.Presentation.Framework;
namespace TickZoom.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    public class Execute
    {
        #region Fields

    	private volatile bool exit = false;
        private long busycount = 0;
        private long loopcount = 0;
        private List<Task> tasks = new List<Task>();
        private List<Loop> loops = new List<Loop>();
        private object tasksLocker = new object();
        private Thread messageLoopThread;
        private string startStack;
        
        public static Execute Create() {
        	var execute = new Execute();
        	execute.Initialize();
        	return execute;
        }
        
        public void Initialize() {
        	exit = false;
        	messageLoopThread = Thread.CurrentThread;
        	startStack = Environment.StackTrace;
        }
        
        public void Flush() {
        	while( tasks.Count > 0) {
        		Thread.Sleep(1);
        	}
        }

        public void Exit() {
        	exit = true;
        }
        #endregion Fields

        #region Methods

        public bool AppIsIdle()
        {
            peek_message msg;
              return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
        }

        public void MessageLoop(object sender, EventArgs e)
        {
        	if( messageLoopThread != Thread.CurrentThread) {
        		throw new ApplicationException("Must always call MessageLoop with same thread as Initialize().");
        	}
            while( !exit && AppIsIdle()) {
                Interlocked.Increment( ref loopcount);
                if( tasks.Count > 0) {
                    lock( tasksLocker) {
                        var task = tasks[0];
                        task.Execute();
                        tasks.Remove(task);
                        Interlocked.Increment( ref busycount);
                    }
                }
                for( int i=0; !exit && i<loops.Count; i++) {
                	var loop = loops[i];
                	if( loop.Execute()) {
                        Interlocked.Increment( ref busycount);
                	}
                }
                if( loopcount > 1000) {
                    TrySleep();
                }
            }
        	if( exit) {
        		Application.ExitThread();
        	}
            Interlocked.Increment( ref busycount);
        }

        public void OnUIThreadLoop(Func<bool> loopMethod)
        {
            lock( tasksLocker) {
                loops.Add( new Loop( loopMethod));
            }
        }
        
        public void OnUIThread(Action action)
        {
            lock( tasksLocker) {
                tasks.Add( new SyncTask( action, () => { } ));
            }
        }

        public void OnUIThreadSync(Action action)
        {
        	if( messageLoopThread == Thread.CurrentThread) {
        		action();
        	}
            var isComplete = false;
            var task = new SyncTask( action, () => isComplete = true );
            lock( tasksLocker) {
                tasks.Add( task);
            }
            while( !isComplete) {
                Thread.Sleep(1);
            }
        }
        
        public T OnUIThread<T>(Delegate action)
        {
        	if( messageLoopThread == Thread.CurrentThread) {
        		return (T) action.DynamicInvoke();
        	}
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

        public void TrySleep()
        {
        	if( busycount == 0) { // || loopcount * 100 / busycount < 10) {
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