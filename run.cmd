@echo off
setlocal DisableDelayedExpansion
rem Builds the Toy Robot application and runs it (Windows cmd and PowerShell).
rem   run.cmd              commands are typed (or read from redirected console input)
rem   run.cmd commands.txt commands are read from the file
rem Messages of this script (progress lines and errors) go to console messages (stderr); console output (stdout) is the application's.

rem 1. A .NET SDK, version 8 or later, must be installed.
>&2 echo Checking for a .NET SDK, version 8 or later...
set "sdkok="
for /f "tokens=1 delims=." %%v in ('dotnet --list-sdks 2^>nul') do call :checkversion %%v
if not defined sdkok (
  >&2 echo A .NET SDK, version 8 or later, is required. Download it from https://dotnet.microsoft.com/download
  exit /b 1
)

rem 2. Build the application quietly; show the build output only when the build fails.
>&2 echo Building the application (Release)...
set "log=%TEMP%\toyrobot-build-%RANDOM%%RANDOM%.log"
dotnet build "%~dp0src\ToyRobot\ToyRobot.csproj" -c Release --nologo < NUL > "%log%" 2>&1
set "rc=%ERRORLEVEL%"
if not "%rc%"=="0" goto :buildfailed
del "%log%"

rem 3. Arguments: none, or one existing file.
if not "%~2"=="" goto :howtorun
if "%~1"=="" goto :noargument
rem A name with * or ? would be expanded by the progress line below; reject it. The first token (cut at * or ?) equals the whole argument only when it holds neither.
set "wild=1"
for /f "eol=| delims=*?" %%a in ("%~1") do if "%%a"=="%~1" set "wild="
if defined wild goto :wildcard
if not exist "%~1" goto :notfound
if exist "%~1\*" goto :notfound
rem The file name is echoed through a for variable, which cmd does not parse again, so characters such as & in it stay text.
for %%f in ("%~1") do >&2 echo Running the application with commands from %%~f
"%~dp0src\ToyRobot\bin\Release\net8.0\ToyRobot.exe" < "%~1"
exit /b %ERRORLEVEL%

:noargument
>&2 echo Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.
"%~dp0src\ToyRobot\bin\Release\net8.0\ToyRobot.exe"
exit /b %ERRORLEVEL%

:buildfailed
type "%log%" 1>&2
del "%log%"
exit /b %rc%

:howtorun
>&2 echo How to run: run.cmd [commands.txt]
exit /b 1

:wildcard
>&2 echo The file name must not contain * or ?: "%~1"
exit /b 1

:notfound
>&2 echo File not found: "%~1"
exit /b 1

:checkversion
rem %1 is the first part of a line of "dotnet --list-sdks", for example 8 in "8.0.414 [...]".
echo %1| findstr /r "^[0-9][0-9]*$" >nul || exit /b 0
if %1 GEQ 8 set "sdkok=1"
exit /b 0
