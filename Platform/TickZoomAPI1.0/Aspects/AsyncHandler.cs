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

namespace TickZoom.Api
{
	public class DiagramAsyncSenderAttribute : Attribute {
		
	}
	public class DiagramAsyncReceiverAttribute : Attribute {
		
	}
	public class DiagramExcludeAttribute : Attribute {
		
	}
#if SkipPostSharp
	public class DiagramHelper {
		public static void Comment( string comment) {
			
		}
		public static void StateChange( object obj, object state) {
			
		}
		public static IList<DiagramAttribute> GetAspectsByCalls() {
			return new List<DiagramAttribute>();
		}
	}
	[AttributeUsage(AttributeTargets.All,AllowMultiple=true)]
	public class DiagramAttribute : Attribute {
		private bool attributeExclude;
		private string attributeTargetTypes;
		private int attributePriority;
		private string attributeTargetMembers;
		private string typeName;
		
		public string TypeName {
			get { return typeName; }
			set { typeName = value; }
		}
		private string methodSignature;
		
		public string MethodSignature {
			get { return methodSignature; }
			set { methodSignature = value; }
		}
		private int callCount;
		
		public int CallCount {
			get { return callCount; }
			set { callCount = value; }
		}
		
		public string AttributeTargetMembers {
			get { return attributeTargetMembers; }
			set { attributeTargetMembers = value; }
		}
		
		public int AttributePriority {
			get { return attributePriority; }
			set { attributePriority = value; }
		}
		
		public string AttributeTargetTypes {
			get { return attributeTargetTypes; }
			set { attributeTargetTypes = value; }
		}
		
		public bool AttributeExclude {
			get { return attributeExclude; }
			set { attributeExclude = value; }
		}
	}
#endif
}
