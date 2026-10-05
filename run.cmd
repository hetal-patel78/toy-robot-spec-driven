@echo off
setlocal EnableExtensions DisableDelayedExpansion
rem Builds the Toy Robot application and runs it (Windows cmd and PowerShell).
rem   run.cmd              typed at a keyboard: shows the menu (DEC-41); redirected or piped console input: the commands are read from it
rem   run.cmd commands.txt commands are read from the file
rem   run.cmd --help       shows the application's help (DEC-04)
rem Messages of this script (progress lines and errors) go to console messages (stderr); console output (stdout) is the application's
rem and the menu's own lines.

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
del "%log%" >nul 2>&1
set "app=%~dp0src\ToyRobot\bin\Release\net8.0\ToyRobot.exe"

rem 3. Arguments: none, --help, or one existing file.
if not "%~2"=="" goto :howtorun
if "%~1"=="" goto :noargument
rem --help (exactly: if without /i is case-sensitive, as the application matches it) shows the application's help.
if "%~1"=="--help" goto :help
rem A name with * or ? would be expanded by the progress line below; reject it. The first token (cut at * or ?) equals the whole argument only when it holds neither.
set "wild=1"
for /f "eol=| delims=*?" %%a in ("%~1") do if "%%a"=="%~1" set "wild="
if defined wild goto :wildcard
if not exist "%~1" goto :notfound
if exist "%~1\*" goto :notfound
rem The file name is echoed through a for variable, which cmd does not parse again, so characters such as & in it stay text.
for %%f in ("%~1") do >&2 echo Running the application with commands from %%~f
"%app%" < "%~1"
exit /b %ERRORLEVEL%

:noargument
rem Console input from a keyboard shows the menu. Redirected or piped console input is read as commands:
rem timeout cannot wait on redirected input and fails at once. TOYROBOT_MENU shows the menu even then (CI-08 uses it).
if defined TOYROBOT_MENU goto :menustart
timeout /t 0 >nul 2>&1
if errorlevel 1 goto :readinput
goto :menustart

:readinput
>&2 echo Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.
"%app%"
exit /b %ERRORLEVEL%

:help
rem No commands are read, so there is no "Running" line.
"%app%" --help < nul
exit /b %ERRORLEVEL%

:buildfailed
type "%log%" 1>&2
del "%log%" >nul 2>&1
exit /b %rc%

:howtorun
>&2 echo How to run: run testdata\challenge-example-1.txt to run the commands in a file, or run with no file for the menu. In PowerShell, use .\run.cmd instead of run.
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

rem The menu (DEC-41): type commands, run a test-data file (its console output compared with the expected file), or run the tests.
:menustart
set "out=%TEMP%\toyrobot-menu-%RANDOM%%RANDOM%.out"
set "err=%out%.err"
set "sum=%out%.sum"
set "hlp=%out%.hlp"
set "empties=0"
rem Colours (Windows 10 or later): PASS green, FAIL red, warnings yellow, the app's console output bright cyan, headings bold.
rem Set NO_COLOR to turn them off.
set "green=" & set "red=" & set "yellow=" & set "bright=" & set "bold=" & set "reset="
if defined NO_COLOR goto :menu
rem The escape character comes from a prompt that shows it ($E); there is no other built-in way to get it in cmd.
for /f "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do set "ESC=%%b"
set "green=%ESC%[92m" & set "red=%ESC%[91m" & set "yellow=%ESC%[93m" & set "bright=%ESC%[1;96m" & set "bold=%ESC%[1m" & set "reset=%ESC%[0m"

:menu
echo.
echo %bold%Toy Robot - choose what to run%reset%
echo.
echo    1  Type commands yourself
echo    2  Run a test-data file
echo    3  Run the automated tests (dotnet test)
echo    4  Show help (commands, rules, how to end a session)
echo    0  Quit
echo.
call :choose
if defined endmenu goto :quit
if "%choice%"=="" goto :menu
if "%choice%"=="0" goto :quit
if "%choice%"=="1" goto :typed
if "%choice%"=="2" goto :files
if "%choice%"=="3" goto :tests
if "%choice%"=="4" goto :showhelp
if not "%choice%"=="?" echo Not a choice: %choice%
goto :menu

:files
echo.
echo %bold%Toy Robot - choose a test-data file%reset% (shows what the app prints, then PASS or FAIL against the expected output)
echo.
echo    1  challenge-example-1
echo    2  challenge-example-2
echo    3  challenge-example-3
echo    4  bom-challenge-example-1
echo    5  exit-mid-file
echo    6  empty
echo    7  long-session
echo    8  all seven files, one after another
echo    0  back
echo.
call :choose
if defined endmenu goto :quit
if "%choice%"=="" goto :files
if "%choice%"=="0" goto :menu
if "%choice%"=="1" call :file challenge-example-1 & goto :files
if "%choice%"=="2" call :file challenge-example-2 & goto :files
if "%choice%"=="3" call :file challenge-example-3 & goto :files
if "%choice%"=="4" call :file bom-challenge-example-1 & goto :files
if "%choice%"=="5" call :file exit-mid-file & goto :files
if "%choice%"=="6" call :file empty & goto :files
if "%choice%"=="7" call :file long-session & goto :files
if "%choice%"=="8" goto :all
if not "%choice%"=="?" echo Not a choice: %choice%
goto :files

:choose
rem Reads one choice. An empty line, or the end of redirected input, shows the menu again; three in a row end the menu.
rem What was typed is only compared and shown with delayed expansion, so no character in it can act as cmd syntax;
rem anything other than one of the digits 0 to 8 is reported here as not a choice and becomes "?".
set "choice="
set /p "choice=Choose: "
if not defined choice goto :chooseempty
set "empties=0"
setlocal EnableDelayedExpansion
set "safe=?"
for %%v in (0 1 2 3 4 5 6 7 8) do if "!choice!"=="%%v" set "safe=%%v"
if "!safe!"=="?" echo(Not a choice: !choice!
endlocal & set "choice=%safe%"
exit /b 0

:chooseempty
set /a empties+=1
if %empties% GEQ 3 set "endmenu=1"
exit /b 0

:all
set "failed=0"
if exist "%sum%" del "%sum%"
for %%f in (challenge-example-1 challenge-example-2 challenge-example-3 bom-challenge-example-1 exit-mid-file empty long-session) do call :file %%f
echo.
echo %bold%Summary%reset%
for /f "usebackq tokens=1,*" %%a in ("%sum%") do if "%%a"=="PASS:" (echo    %green%%%a %%b%reset%) else (echo    %red%%%a %%b%reset%)
del "%sum%"
if "%failed%"=="0" (echo %green%All 7 files PASS%reset%) else (echo %red%%failed% of 7 files FAIL%reset%)
goto :files

:typed
echo.
>&2 echo Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.
"%app%"
echo.
echo Typed session ended.
goto :menu

:tests
echo.
dotnet test "%~dp0ToyRobot.sln" --nologo < nul
goto :menu

:showhelp
rem The help menu (DEC-41): one topic of the help at a time, so each fits on a small screen. Topics are plain text.
echo.
echo %bold%Toy Robot - help: choose a topic%reset%
echo.
echo    1  How to start
echo    2  The table
echo    3  Commands
echo    4  Rules
echo    5  Ending a session
echo    6  All of it
echo    0  back
echo.
call :choose
if defined endmenu goto :quit
if "%choice%"=="" goto :showhelp
if "%choice%"=="0" goto :menu
if "%choice%"=="1" call :helptopic "HOW TO START" & goto :showhelp
if "%choice%"=="2" call :helptopic "THE TABLE" & goto :showhelp
if "%choice%"=="3" call :helptopic "COMMANDS" & goto :showhelp
if "%choice%"=="4" call :helptopic "RULES" & goto :showhelp
if "%choice%"=="5" call :helptopic "ENDING A SESSION" & goto :showhelp
if "%choice%"=="6" call :helptopic "" & goto :showhelp
if not "%choice%"=="?" echo Not a choice: %choice%
goto :showhelp

:helptopic
rem Writes the section of the application's help text whose heading line starts with %1, up to the line before the
rem next blank line; with "" all of it. The text is the application's own --help output, so there is one help text
rem (DEC-04). findstr /n numbers every line, so blank lines are kept (as "12:"); each line is written through a for
rem variable, which cmd does not parse again, so the < in the help stays text.
"%app%" --help < nul > "%hlp%"
echo.
if "%~1"=="" (type "%hlp%" & exit /b 0)
set "start="
set "stop="
for /f "delims=:" %%n in ('findstr /n /b /c:"%~1" "%hlp%"') do if not defined start set "start=%%n"
if not defined start exit /b 0
for /f "tokens=1* delims=:" %%a in ('findstr /n "^" "%hlp%"') do if %%a GTR %start% if "%%b"=="" if not defined stop set "stop=%%a"
if not defined stop set "stop=9999"
for /f "tokens=1* delims=:" %%a in ('findstr /n "^" "%hlp%"') do if %%a GEQ %start% if %%a LSS %stop% echo(%%b
exit /b 0

:quit
for %%t in ("%out%" "%err%" "%sum%" "%hlp%") do if exist %%t del %%t
exit /b 0

:file
rem %1 is a test-data name: run testdata\%1.txt and show its commands with any warnings (console messages, stderr),
rem its output (console output, stdout) and PASS or FAIL against testdata\%1.expected.txt.
echo.
echo %bold%---------------- %1 ----------------%reset%
rem The run gets its console input from the file only, so it cannot read the menu's own input.
"%app%" < "%~dp0testdata\%1.txt" > "%out%" 2> "%err%"
set "rc=%ERRORLEVEL%"
echo %bold%Commands%reset%  (testdata\%1.txt)
rem The app echoes each line it reads as "> line"; that prefix is dropped. Other lines are warnings, shown in yellow.
set "lines="
for /f "usebackq tokens=1,* delims= " %%a in ("%err%") do (
  set "lines=1"
  if "%%a"==">" (echo(   %%b) else (echo(   %yellow%%%a %%b%reset%)
)
if not defined lines echo    (none)
echo %bold%Output%reset%
call :show "%out%" "%bright%"
rem fc reads console input too, so it gets < nul.
fc "%out%" "%~dp0testdata\%1.expected.txt" < nul > nul 2>&1
if errorlevel 1 goto :filediff
if not "%rc%"=="0" goto :filecode
echo %green%PASS: %1 - matches testdata\%1.expected.txt%reset%
>>"%sum%" echo PASS: %1
exit /b 0

:filediff
echo %bold%Expected%reset%
call :show "%~dp0testdata\%1.expected.txt" ""
echo %red%FAIL: %1 - output differs from testdata\%1.expected.txt%reset%
>>"%sum%" echo FAIL: %1 - output differs
set /a failed+=1
exit /b 0

:filecode
echo %red%FAIL: %1 - exit code %rc%, expected 0%reset%
>>"%sum%" echo FAIL: %1 - exit code %rc%
set /a failed+=1
exit /b 0

:show
rem Shows the lines of file %1, indented, in colour %2; "(nothing)" when it has none.
set "lines="
for /f "usebackq delims=" %%l in (%1) do (
  set "lines=1"
  echo(   %~2%%l%reset%
)
if not defined lines echo    (nothing)
exit /b 0
