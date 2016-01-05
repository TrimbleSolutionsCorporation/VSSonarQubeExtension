@echo OFF


call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
msbuild BuildExtension.msbuild /p:VsVersion=14.0 /p:Vsix=Vs2015 /p:VsFolder=vs15 /p:SolutionFile=VSSonarQubeExtension2015.sln /p:Configuration=Release

IF ERRORLEVEL 1 GOTO ERROR

call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\VsDevCmd.bat"
msbuild BuildExtension.msbuild /p:VsVersion=12.0 /p:Vsix=Vs2013 /p:VsFolder=vs13 /p:SolutionFile=VSSonarQubeExtension2013.sln /p:Configuration=Release
IF ERRORLEVEL 1 GOTO ERROR
GOTO no_error

:ERROR
exit 1


:no_error