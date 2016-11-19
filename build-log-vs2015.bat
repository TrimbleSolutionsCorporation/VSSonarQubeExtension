@echo on
msbuild\nuget restore VSSonarQubeExtension2015.sln
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
msbuild BuildExtension.msbuild /p:VisualStudioVersion=14.0 /p:Vsix=Vs2015 /p:VsVersion=14.0 /p:VsFolder=vs15 /p:SolutionFile=VSSonarQubeExtension2015.sln /p:Configuration=Release > buildlog2015.txt

