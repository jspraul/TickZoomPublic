ECHO TickZoomGUI with FREE Engine PostBuild
ECHO %1 %2
ECHO %~2..\bin\%~1\TickZoomEngine.dll
xcopy /Y "%~2..\Engine\TickZoomEngine*.dll" "%~2\..\bin\%~1"
ECHO Finished.
