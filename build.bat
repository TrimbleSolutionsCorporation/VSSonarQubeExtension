@echo on


call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
msbuild BuildExtension.msbuild /p:VisualStudioVersion=14.0 /p:Vsix=Vs2015 /p:VsFolder=vs15 /p:SolutionFile=VSSonarQubeExtension2015.sln /p:Configuration=Release
msbuild VSSonarQubeExtension2012.sln /p:VisualStudioVersion=12.0 /p:Vsix=Vs2012 /p:VsFolder=vs12 /p:SolutionFile=VSSonarQubeExtension2012.sln /p:Configuration=Release

call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\VsDevCmd.bat"
msbuild VSSonarQubeExtension2013.sln /p:VisualStudioVersion=12.0 /p:Vsix=Vs2013 /p:VsFolder=vs13 /p:SolutionFile=VSSonarQubeExtension2013.sln /p:Configuration=Release





