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
using System.Runtime.CompilerServices;
using System.Text;

#if !SkipPostSharp
namespace TickZoom.Api
{
	public class DiagramHelper
	{
		private static readonly Log Logger = Factory.Log.GetLogger("Diagram");
		public static readonly bool Debug = Logger.IsDebugEnabled;
		private static readonly bool Trace = Logger.IsTraceEnabled;
		private static List<DiagramAttribute> diagramAspects = new List<DiagramAttribute>();
		private static Dictionary<DiagramObject,DiagramInstance> instances;
		private static object locker = new object();
		private static Dictionary<Type,int> instanceCounters;
		
		public static void AddAspect(DiagramAttribute aspect) {
			lock( locker) {
				diagramAspects.Add(aspect);
			}
		}
		
		public static void StateChange(object _object, object obj) {
			if( Debug) {
				bool isNew;
				DiagramInstance instance = GetInstance(_object, out isNew);
				if( instance.Trace) {
					instance.Logger.Trace( instance.Name + " >>> " + obj);
				}
			}
		}
		
		public static void Comment(string message) {
			if( Debug) {
				Logger.Debug( ";\n;" + message + "\n;");
			}
		}
		
		public static void Line(object _object) {
			if( Debug) {
				bool isNew;
				DiagramInstance instance = GetInstance(_object, out isNew);
				if( instance.Trace) {
					MethodBase caller = new StackFrame(1).GetMethod();
					instance.Logger.Trace( "# " + _object);
				}
			}
		}

		public static DiagramInstance GetInstance(object _object, out bool isNew) {
			DiagramInstance callee;
			isNew = true;
			if( !Instances.TryGetValue(new DiagramObject(_object),out callee)) {
	    		MethodBase method = new StackFrame(1).GetMethod();
	    		Type type;
	    		string typeName;
	    		string fullName;
	    		if( _object is Type) {
	    			type = (Type) _object;
	    			typeName = "(" + GetTypeName(type) + ")";
	    			fullName = type.FullName;
	    		} else {
	    			type = _object.GetType();
	    			typeName = GetTypeName(type);
	    			fullName = type.FullName;
	    		}
	    		callee = new DiagramInstance(typeName,fullName);
	    		if( !(_object is Type)) {
		    		int counter;
		    		lock(locker) {
			    		if( !InstanceCounters.TryGetValue(type,out counter)) {
			    			counter = 0;
			    			InstanceCounters[type] = counter;
			    		}
			    		InstanceCounters[type] = ++counter;
		    		}
		    		callee.Name += counter;
	    		}
	    		Instances[new DiagramObject(_object)] = callee;
    		} else {
				isNew = false;
    		}
			return callee;
		}
		
		public static string GetTypeName(Type type) {
			Type[] generics = type.GetGenericArguments();
			if( generics.Length>0) {
				StringBuilder builder = new StringBuilder();
				builder.Append(StripTilda(type.Name));
				builder.Append("<");
				for( int j=0; j<generics.Length; j++) {
					if( j!=0) builder.Append(",");
					Type generic = generics[j];
					builder.Append(GetTypeName(generic));
				}
				builder.Append(">");
				return builder.ToString();
			} else {
				return type.Name;
			}
		}
		
		private static string StripTilda(string typeName) {
			return typeName.Substring(0,typeName.IndexOf('`'));
		}
		
		public static Dictionary<DiagramObject,DiagramInstance> Instances {
			get { if( instances == null) {
	    			lock(locker) {
	    				if( instances == null) {
							instances = new Dictionary<DiagramObject,DiagramInstance>();
	    				}
	    			}
				}
				return instances;
			}
		}
		
		public static Dictionary<Type, int> InstanceCounters {
	    	get { if( instanceCounters == null) {
	    			lock( locker) {
	    				if( instanceCounters == null) {
			    			instanceCounters = new Dictionary<Type, int>();
	    				}
	    			}
	    		}
	    		return instanceCounters;
	    	}
		}
		
		public static IList<DiagramAttribute> GetAspectsByCalls() {
			diagramAspects.Sort( delegate( DiagramAttribute a, DiagramAttribute b) {
			                    	return b.CallCount - a.CallCount;
			                    });
			return diagramAspects;
		}
	}
}
#endif