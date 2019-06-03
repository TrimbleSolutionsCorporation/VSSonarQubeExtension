@echo on
msbuild\nuget restore VSSonarQubeExtension2019.sln
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsMSBuildCmd.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat"
msbuild VSSonarQubeExtension2019.sln /p:Configuration=Release > buildlog2019.txt

