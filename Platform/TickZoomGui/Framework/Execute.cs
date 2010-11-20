using System.Collections.Generic;
using System.Threading;

namespace TickZoom.GUI.Framework
{
    using System;
    using System.Windows;

    public interface Task {
    	void Execute();
    	object Result { get; }
    }
	    
    public class SyncTask : Task {
    	private object result;
    	private Delegate execute;
    	private Action complete;
    	public SyncTask( Delegate execute, Action complete) {
    		this.execute = execute;
    		this.complete = complete;	    		
    	}
    	public void Execute() {
    		result = execute.DynamicInvoke();
    		complete();
    	}
    	
		public object Result {
			get { return result; }
		}
    }
    
    public static class Execute
    {
	    private static object tasksLocker = new object();
	    private static List<Task> tasks = new List<Task>();
	    public static void MessageLoop() {
	    	while( tasks.Count > 0) {
		    	lock( tasksLocker) {
		    		var task = tasks[0];
		    		task.Execute();
		    		tasks.Remove(task);
		    	}
	    	}
	    }
	    
	    public static void OnUIThread(Action action) {
	    	lock( tasksLocker) {	    		
	    		tasks.Add( new SyncTask( action, () => { } ));
	    	}
	    }
	    
	    public static T OnUIThread<T>(Delegate action) {
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
    }
}