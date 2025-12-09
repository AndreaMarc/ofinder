@echo off
SET executable=MIT.Fwk.WebApi.dll
SET mypath=%~dp0
SET mypath=%mypath%%executable%
echo %mypath%
dotnet  "%mypath%" -key Mae2019! > temp.txt
set /p mykey=<temp.txt
del temp.txt
dotnet  "%mypath%" -lic %mykey% -v 3000