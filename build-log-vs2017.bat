@echo on
msbuild\nuget restore VSSonarQubeExtension2017.sln
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsMSBuildCmd.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
msbuild BuildExtension.msbuild /p:AssemblyPatcherTaskOn=true /p:VisualStudioVersion=15.0 /p:Vsix=Vs2017 /p:VsVersion=15.0 /p:VsFolder=vs17 /p:SolutionFile=VSSonarQubeExtension2017.sln /p:Configuration=Release > buildlog2017.txt

