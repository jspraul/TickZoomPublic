#region Using directives

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using TickZoom.Api;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Common")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Common")]
[assembly: AssemblyCopyright("Copyright 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: Diagram( AttributePriority=1, AttributeTargetTypes = "TickZoom.*")]
[assembly: Diagram( AttributePriority=2, AttributeExclude=true, 
                   AttributeTargetTypes = "TickZoom.*",
                   AttributeTargetMembers = "regex:(set_.*|get_.*)")]
//[assembly: Diagram( AttributePriority=2, AttributeExclude=true, 
//                   AttributeTargetTypes = "TickZoom.Transactions.*")]
//[assembly: Diagram( AttributePriority=2, AttributeExclude=true, 
//                   AttributeTargetTypes = "TickZoom.Interceptors.*")]
//[assembly: Diagram( AttributePriority=2, AttributeExclude=true, 
//                   AttributeTargetTypes = "TickZoom.Statistics.*")]
//[assembly: Diagram( AttributePriority=2, AttributeExclude=true, 
//                   AttributeTargetTypes = "TickZoom.Reports.*")]

// This sets the default COM visibility of types in the assembly to invisible.
// If you need to expose a type to COM, use [ComVisible(true)] on that type.
[assembly: ComVisible(false)]

// The assembly version has following format :
//
// Major.Minor.Build.Revision
//
// You can specify all the values or you can use the default the Revision and 
// Build Numbers by using the '*' as shown below:
[assembly: AssemblyVersion("1.1.3.3613")]
