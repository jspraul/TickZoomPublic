using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TickZoom.Api;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("TickZOOM")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("TickZOOM Services, LLC")]
[assembly: AssemblyProduct("TickZOOM")]
[assembly: AssemblyCopyright("Copyright © TickZOOM Servicess 2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: Diagram( AttributePriority=1, AttributeTargetTypes = "TickZoom.*")]
[assembly: Diagram( AttributePriority=2, AttributeExclude=true, 
                   AttributeTargetTypes = "TickZoom.*",
                   AttributeTargetMembers = "regex:(set_.*|get_.*)")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("94c164ce-02a9-4966-948f-004d35760ba1")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.21.21890")]
[assembly: AssemblyFileVersion("1.0.0.0")]
