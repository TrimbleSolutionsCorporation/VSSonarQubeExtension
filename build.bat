@echo on
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\VsDevCmd.bat"
msbuild VSSonarQubeExtension2013.sln /p:VisualStudioVersion=12.0 /p:Configuration=Release > buildlog2013tekla.txt

call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
msbuild VSSonarQubeExtension2015.sln /p:VisualStudioVersion=14.0 /p:Configuration=Release > buildlog2015tekla.txt

