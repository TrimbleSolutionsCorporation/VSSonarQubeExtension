
nuget restore VSSonarQubeExtension2013.sln
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\VsDevCmd.bat"
msbuild BuildExtension.msbuild /p:VisualStudioVersion=12.0 /p:Vsix=Vs2013 /p:VsVersion=12.0 /p:VsFolder=vs13 /p:SolutionFile=VSSonarQubeExtension2013.sln /p:Configuration=Release /v:diag > buildlog2013.txt

