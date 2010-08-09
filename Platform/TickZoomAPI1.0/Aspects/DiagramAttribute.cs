#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

#if !SkipPostSharp
using PostSharp.Extensibility;
using PostSharp.Laos;
[assembly: DisablePostSharpMessage("PS0131")]

namespace TickZoom.Api
{
	public class MethodState {
		public DiagramInstance InstanceX;
		public MethodBase Method;
		public bool IsAsync;
		public int CallCounter;
		public readonly Log Logger;
		public readonly bool Debug;
		public bool Trace;
		public MethodState( MethodBase method, string fullName) {
			this.Method = method;
			Logger = Factory.SysLog.GetLogger("Diagram."+fullName);
			Debug = Logger.IsDebugEnabled;
			Trace = Logger.IsTraceEnabled;
		}
		public override bool Equals(object obj)
		{
			MethodState other = (MethodState) obj;
			return InstanceX.Equals(other.InstanceX) && Method.Equals(other.Method);
		}
		public override int GetHashCode()
		{
			return InstanceX.GetHashCode() ^ Method.GetHashCode();
		}
	}
#region Attibutes
	[MulticastAttributeUsage( MulticastTargets.Method | MulticastTargets.InstanceConstructor, 
                          AllowMultiple=true,
                          TargetMemberAttributes = 
                            MulticastAttributes.NonAbstract | 
                            MulticastAttributes.Managed)]
#endregion
	[Serializable]
	public class DiagramAttribute : OnMethodBoundaryAspect
	{
#region Fields
		private static Dictionary<DiagramMethodObject,MethodState> instances = new Dictionary<DiagramMethodObject,MethodState>();
		private static Dictionary<Task,Stack<MethodState>> stacks;
		[ThreadStatic]
		private static readonly bool verifyStack = false;
		private string methodSignature;
		private bool isConstructor = false;
		private int callCount = 0;
		private object staticObject = new object();
		private Type objectType;
		private MethodBase method;
		private bool isSilent;
		private bool isExcluded;
		private bool isAsyncReceiver;
		private bool isAsyncSender;
		
#endregion		
		public DiagramAttribute() {
		}
		
		public override void CompileTimeInitialize(System.Reflection.MethodBase method)
		{
			base.CompileTimeInitialize(method);
			objectType = method.DeclaringType;
			this.method = method;
			if( method.Name == ".ctor" ) {
			 	isConstructor = true;
			}
			if( method.Name == "ToString" ) {
			 	isExcluded = true;
			}
			object[] attributes = method.GetCustomAttributes(true);
			foreach( var attribute in attributes) {
				if( attribute is DiagramAsyncReceiverAttribute) {
					isAsyncReceiver = true;
				}
				if( attribute is DiagramAsyncSenderAttribute) {
					isAsyncSender = true;
				}
				if( attribute is DiagramExcludeAttribute) {
					isSilent = true;
				}
			}
			if( method.IsPrivate) {
				isSilent = true;
			}
		}
		
		private class InstanceTagProperties {
			public bool IsInternalCall = false;
		}
		
		private MethodState GetMethodState( object _object, out bool isNew) {
			var key = new DiagramMethodObject(_object,method);
	    	MethodState callee;
	    	if( !instances.TryGetValue(key, out callee)) {
	    		Type type = _object is Type ? (Type) _object : _object.GetType();
    			string fullName = type.FullName + "." + method.Name;
	    		callee = new MethodState(method,fullName);
	    		instances[key] = callee;
	    		callee.InstanceX = DiagramHelper.GetInstance(_object,out isNew);
	    	} else {
		    	isNew = false;
	    	}
	    	return callee;
		}
		
		private object locker = new object();
	    public sealed override void OnEntry( MethodExecutionEventArgs eventArgs )
	    {
	    	if( DiagramHelper.Debug && !isExcluded) {
    			bool isConstructorChain = false;
	    		bool isNew;
	    		object _object = eventArgs.Instance == null ? objectType : eventArgs.Instance;
	    		var callee = GetMethodState(_object,out isNew);
    			if( !callee.Debug) return;
				callCount++;
    			if( callee.Trace) {
					callee.CallCounter++;
	    			if( callee.CallCounter > 10) {
	    				callee.Trace = false;
	    			}
    			}
	    		callee.IsAsync = isAsyncSender;
	    		if( !isNew && isConstructor) {
    				isConstructorChain = true;
	    		}
	    		if( Stack.Count>0 && callee.Equals(Stack.Peek()) ) {
		    		Stack.Push(callee);
	    			return;
	    		}
		    	MethodState caller;
	    		if( Stack.Count > 0) {
			    	caller = Stack.Peek();
			    	if( verifyStack) {
			    		MethodBase callingMethod = new StackFrame(3).GetMethod();
			    		string callerType = DiagramHelper.GetTypeName(callingMethod.DeclaringType);
			    		if( caller.InstanceX.Name.StartsWith(callerType)) {
			    			callee.Logger.Trace("-- Matches Type--");
			    		} else {
			    			callee.Logger.Error("ERROR: stack instance, " + caller.InstanceX.Name + " mismatches type, " + callerType + "." + "\n" + Environment.StackTrace);
			    		}
			    	}
    			} else {
		    		MethodBase callingMethod = new StackFrame(3).GetMethod();
		    		bool otherNew;
		    		caller = GetMethodState(callingMethod.DeclaringType,out otherNew);
		    		Stack.Push(caller);
    			}
		    	if( isAsyncReceiver) {
		    		AsyncHandler async = eventArgs.Instance as AsyncHandler;
		    		if( async == null) {
		    			throw new ApplicationException("The method " + method.Name + " has the Asynchronous attribute applied but the class " + objectType + " does not implement the " + typeof(AsyncHandler).Name + " interface.");
		    		}
		    		bool otherNew;
		    		callee = GetMethodState(async.Instance,out otherNew);
	    			if( callCount > 10) {
	    				callee.Trace = false;
	    			}
					AsyncSend(caller, callee, isConstructorChain);
		    	} else if( isAsyncSender) {
		    		callee.IsAsync = true;
		    	} else if( !isSilent && !caller.IsAsync) {
					Call(caller, callee, isConstructorChain);
		    	}
				Stack.Push(callee);
	    	}
	    }
	    
		public void Call(MethodState caller, MethodState callee, bool isConstructorChain) {
			if( isConstructor) {
				if( !isConstructorChain) {
					if( callee.Trace) callee.Logger.Trace(caller.InstanceX.Name + " (!) " + callee.InstanceX.Name);
				}
			} else {
				if( callee.Trace) callee.Logger.Trace(caller.InstanceX.Name + " ==> " + callee.InstanceX.Name + " " + MethodSignature);
			}
		}	   
	    
		public void AsyncSend(MethodState caller, MethodState callee, bool isConstructorChain) {
			if( callee.Trace) callee.Logger.Trace(caller.InstanceX.Name + " >-> " + callee.InstanceX.Name + " " + MethodSignature);
		}	   
	    
	    private void Exception( MethodState caller, MethodState callee, string message) {
	    	if( callee.Trace) {
	    		callee.Logger.Trace(callee.InstanceX.Name + " >-> " + caller.InstanceX.Name + " " + message);
	    	}
	    }
	    
	    public sealed override void OnException( MethodExecutionEventArgs eventArgs )
	    {
	    	if( DiagramHelper.Debug && !isExcluded) {
	    		bool isNew;
	    		object _object = eventArgs.Instance == null ? objectType : eventArgs.Instance;
	    		MethodState callee = GetMethodState(_object,out isNew);
	    		if( !callee.Debug) return;
		    	if( Stack.Count > 0) {
	    			var enumerator = Stack.GetEnumerator();
	    			if( enumerator.MoveNext() && enumerator.MoveNext()) {
	    				MethodState caller = enumerator.Current;
		    			var ex = eventArgs.Exception;
		    			Exception(caller, callee, "Exception: " + ex.GetType().Name);
	    			}
		    	}
	    	}
	    }
	    
	    public sealed override void OnExit( MethodExecutionEventArgs eventArgs )
	    {
	    	if( DiagramHelper.Debug && !isExcluded) {
	    		bool isNew;
	    		object _object = eventArgs.Instance == null ? objectType : eventArgs.Instance;
	    		MethodState callee = GetMethodState(_object,out isNew);
	    		if( !callee.Debug) return;
		    	if( Stack.Count > 0) {
			    	callee = Stack.Pop();
			    	if( Stack.Count > 0) {
				    	MethodState caller = Stack.Peek();
				    	if( caller.Equals(callee)) {
				    		return;
				    	}
				    	if( !isSilent && !isConstructor && !isAsyncReceiver && !caller.IsAsync) {
			 				Return(caller,callee);
				    	}
			    	}
		    	}
	    	}
	    }
	    
		public void Return(MethodState caller, MethodState callee) {
			if( callee.Trace) {
				callee.Logger.Trace(callee.InstanceX.Name + " <== " + callee.InstanceX.Name + " " + MethodSignature);
			}
		}
	    
	    private string GetSignature(MethodBase method) {
			ParameterInfo[] parameters = method.GetParameters();
			StringBuilder builder = new StringBuilder();
			builder.Append(method.Name);
			builder.Append("(");
			for( int i=0; i<parameters.Length; i++) {
				if( i!=0) builder.Append(",");
				ParameterInfo parameter = parameters[i];
				Type type = parameter.ParameterType;
				builder.Append(DiagramHelper.GetTypeName(type));
			}
			builder.Append(")");
			return builder.ToString();
		}	    

		private static object stackLocker = new object();
		public static Stack<MethodState> Stack {
			get {
				if( stacks == null) {
					lock( stackLocker) {
						if( stacks == null) {
							stacks = new Dictionary<Task,Stack<MethodState>>();
						}
					}
				}			
				Stack<MethodState> stack;
				Task task = Factory.Parallel.CurrentTask;
				if( !stacks.TryGetValue(task,out stack)) {
					lock( stackLocker) {
						if( !stacks.TryGetValue(task,out stack)) {
							stack = new Stack<MethodState>();
							stacks.Add( task, stack);
						}
					}
				}
				return stack;
			}
		}
		
		public string TypeName {
			get {
				return objectType.Name;
			}
		}
		
		public string MethodSignature {
			get { if( methodSignature == null) {
	    			lock( locker) {
	    				if( methodSignature == null) {
							methodSignature = GetSignature(method);
	    				}
	    			}
				}
				return methodSignature;
			}
		}
		
		public int CallCount {
			get { return callCount; }
		}
	}
}
#endif