@echo on
msbuild\nuget restore VSSonarQubeExtension2012.sln
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
msbuild BuildExtension.msbuild /p:VSVersion=11.0 /p:Vsix=Vs2012 /p:VsFolder=vs12 /p:SolutionFile=VSSonarQubeExtension2012.sln /p:Configuration=Release /v:diagnostic > buildlog2012.txt

