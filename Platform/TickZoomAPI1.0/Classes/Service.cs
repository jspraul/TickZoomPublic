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
using System.Runtime.InteropServices;
using TickZoom.Api;

namespace TickZoom.API.Classes
{
[Flags]
public enum ServiceManagerRights
{
Connect = 0x0001,
CreateService = 0x0002,
EnumerateService = 0x0004,
Lock = 0x0008,
QueryLockStatus = 0x0010,
ModifyBootConfig = 0x0020,
StandardRightsRequired = 0xF0000,
AllAccess = (StandardRightsRequired | Connect | CreateService |
EnumerateService | Lock | QueryLockStatus | ModifyBootConfig)
}

[Flags]
public enum ServiceRights
{
QueryConfig = 0x1,
ChangeConfig = 0x2,
QueryStatus = 0x4,
EnumerateDependants = 0x8,
Start = 0x10,
Stop = 0x20,
PauseContinue = 0x40,
Interrogate = 0x80,
UserDefinedControl = 0x100,
Delete = 0x00010000,
StandardRightsRequired = 0xF0000,
AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig |
QueryStatus | EnumerateDependants | Start | Stop | PauseContinue |
Interrogate | UserDefinedControl)
}

public enum ServiceBootFlag
{
Start = 0x00000000,
SystemStart = 0x00000001,
AutoStart = 0x00000002,
DemandStart = 0x00000003,
Disabled = 0x00000004
}

public enum ServiceState
{
Unknown = -1, // The state cannot be (has not been) retrieved.
NotFound = 0, // The service is not known on the host server.
Stopped = 1,
StartPending = 2,
StopPending = 3,
Running = 4,
ContinuePending = 5,
PausePending = 6,
Paused = 7
}

public enum ServiceControl
{
Stop = 0x00000001,
Pause = 0x00000002,
Continue = 0x00000003,
Interrogate = 0x00000004,
Shutdown = 0x00000005,
ParamChange = 0x00000006,
NetBindAdd = 0x00000007,
NetBindRemove = 0x00000008,
NetBindEnable = 0x00000009,
NetBindDisable = 0x0000000A
}

public enum ServiceError
{
Ignore = 0x00000000,
Normal = 0x00000001,
Severe = 0x00000002,
Critical = 0x00000003
}

public class Service
{
private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

[StructLayout(LayoutKind.Sequential)]
private class SERVICE_STATUS
{
public int dwServiceType = 0;
public ServiceState dwCurrentState = 0;
public int dwControlsAccepted = 0;
public int dwWin32ExitCode = 0;
public int dwServiceSpecificExitCode = 0;
public int dwCheckPoint = 0;
public int dwWaitHint = 0;
}

[DllImport("advapi32.dll", EntryPoint= "OpenSCManagerA")]
private static extern IntPtr OpenSCManager(string lpMachineName, string
lpDatabaseName, ServiceManagerRights dwDesiredAccess);
[DllImport("advapi32.dll", EntryPoint= "OpenServiceA",
CharSet=CharSet.Ansi)]
private static extern IntPtr OpenService(IntPtr hSCManager, string
lpServiceName, ServiceRights dwDesiredAccess);
[DllImport("advapi32.dll", EntryPoint= "CreateServiceA")]
private static extern IntPtr CreateService(IntPtr hSCManager, string
lpServiceName, string lpDisplayName, ServiceRights dwDesiredAccess, int
dwServiceType, ServiceBootFlag dwStartType, ServiceError dwErrorControl,
string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string
lpDependencies, string lp, string lpPassword);
[DllImport("advapi32.dll")]
private static extern int CloseServiceHandle(IntPtr hSCObject);
[DllImport("advapi32.dll")]
private static extern int QueryServiceStatus(IntPtr hService,
SERVICE_STATUS lpServiceStatus);
[DllImport("advapi32.dll", SetLastError = true)]
private static extern int DeleteService(IntPtr hService);
[DllImport("advapi32.dll")]
private static extern int ControlService(IntPtr hService, ServiceControl
dwControl, SERVICE_STATUS lpServiceStatus);
[DllImport("advapi32.dll", EntryPoint= "StartServiceA")]
private static extern int StartService(IntPtr hService, int
dwNumServiceArgs, int lpServiceArgVectors);


private Service()
{
}

public static void Uninstall(string ServiceName)
{
IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
try
{
IntPtr service = OpenService(scman, ServiceName,
ServiceRights.StandardRightsRequired | ServiceRights.Stop |
ServiceRights.QueryStatus);
if(service == IntPtr.Zero)
{
throw new ApplicationException("Service not installed.");
}
try
{
StopService(service);
int ret = DeleteService(service);
if(ret == 0)
{
int error = Marshal.GetLastWin32Error();
throw new ApplicationException("Could not delete service " + error);
}
}
finally
{
CloseServiceHandle(service);
}
}
finally
{
CloseServiceHandle(scman);
}
}

public static bool ServiceIsInstalled(string ServiceName)
{
IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
try
{
IntPtr service = OpenService(scman, ServiceName,
ServiceRights.QueryStatus);
if(service == IntPtr.Zero) return false;
CloseServiceHandle(service);
return true;
}
finally
{
CloseServiceHandle(scman);
}
}

public static void InstallAndStart(string ServiceName, string DisplayName,
string FileName)
{
IntPtr scman = OpenSCManager(ServiceManagerRights.Connect |
ServiceManagerRights.CreateService);
try
{
IntPtr service = OpenService(scman, ServiceName,
ServiceRights.QueryStatus | ServiceRights.Start);
if(service == IntPtr.Zero)
{
service = CreateService(scman, ServiceName, DisplayName,
ServiceRights.QueryStatus | ServiceRights.Start, SERVICE_WIN32_OWN_PROCESS,
ServiceBootFlag.AutoStart, ServiceError.Normal, FileName, null, IntPtr.Zero,
null, null, null);
}
if(service == IntPtr.Zero)
{
throw new ApplicationException("Failed to install service.");
}
try
{
StartService(service);
}
finally
{
CloseServiceHandle(service);
}
}
finally
{
CloseServiceHandle(scman);
}
}

public static void StartService(string Name)
{
IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
try
{
IntPtr hService = OpenService(scman, Name, ServiceRights.QueryStatus |
ServiceRights.Start);
if(hService == IntPtr.Zero)
{
throw new ApplicationException("Could not open service.");
}
try
{
StartService(hService);
}
finally
{
CloseServiceHandle(hService);
}
}
finally
{
CloseServiceHandle(scman);
}
}

public static void StopService(string Name)
{
IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
try
{
IntPtr hService = OpenService(scman, Name, ServiceRights.QueryStatus |
ServiceRights.Stop);
if(hService == IntPtr.Zero)
{
throw new ApplicationException("Could not open service.");
}
try
{
StopService(hService);
}
finally
{
CloseServiceHandle(hService);
}
}
finally
{
CloseServiceHandle(scman);
}
}

private static void StartService(IntPtr hService)
{
SERVICE_STATUS status = new SERVICE_STATUS();
StartService(hService, 0, 0);
WaitForServiceStatus(hService, ServiceState.StartPending,
ServiceState.Running);
}

private static void StopService(IntPtr hService)
{
SERVICE_STATUS status = new SERVICE_STATUS();
ControlService(hService, ServiceControl.Stop, status);
WaitForServiceStatus(hService, ServiceState.StopPending,
ServiceState.Stopped);
}

public static ServiceState GetServiceStatus(string ServiceName)
{
IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
try
{
IntPtr hService = OpenService(scman, ServiceName,
ServiceRights.QueryStatus);
if(hService == IntPtr.Zero)
{
return ServiceState.NotFound;
}
try
{
return GetServiceStatus(hService);
}
finally
{
CloseServiceHandle(scman);
}
}
finally
{
CloseServiceHandle(scman);
}
}

private static ServiceState GetServiceStatus(IntPtr hService)
{
SERVICE_STATUS ssStatus = new SERVICE_STATUS();
if(QueryServiceStatus(hService, ssStatus) == 0)
{
throw new ApplicationException("Failed to query service status.");
}
return ssStatus.dwCurrentState;
}

private static bool WaitForServiceStatus(IntPtr hService, ServiceState
WaitStatus, ServiceState DesiredStatus)
{
SERVICE_STATUS ssStatus = new SERVICE_STATUS();
long dwOldCheckPoint;
long dwStartTickCount;

QueryServiceStatus(hService, ssStatus);
if(ssStatus.dwCurrentState == DesiredStatus) return true;
dwStartTickCount = Factory.TickCount;
dwOldCheckPoint = ssStatus.dwCheckPoint;

while(ssStatus.dwCurrentState == WaitStatus)
{
// Do not wait longer than the wait hint. A good interval is
// one tenth the wait hint, but no less than 1 second and no
// more than 10 seconds.

int dwWaitTime = ssStatus.dwWaitHint / 10;

if(dwWaitTime < 1000) dwWaitTime = 1000;
else if (dwWaitTime > 10000) dwWaitTime = 10000;

System.Threading.Thread.Sleep(dwWaitTime);

// Check the status again.

if(QueryServiceStatus(hService, ssStatus) == 0) break;

if(ssStatus.dwCheckPoint > dwOldCheckPoint)
{
// The service is making progress.
dwStartTickCount = Factory.TickCount;
dwOldCheckPoint = ssStatus.dwCheckPoint;
}
else
{
if(Factory.TickCount - dwStartTickCount > ssStatus.dwWaitHint)
{
// No progress made within the wait hint
break;
}
}
}
return(ssStatus.dwCurrentState == DesiredStatus);
}

private static IntPtr OpenSCManager(ServiceManagerRights Rights)
{
IntPtr scman = OpenSCManager(null, null, Rights);
if(scman == IntPtr.Zero)
{
throw new ApplicationException("Could not connect to service control manager.");
}
return scman;
}
}
} 