@echo off
setlocal EnableExtensions DisableDelayedExpansion
rem Toy Robot menu (Windows cmd and PowerShell; DEC-41). Lists what can be run and runs the choice by number:
rem a test-data file (its console output compared with the expected file), all of them, a typed session, or the automated tests.
rem Each run goes through run.cmd, so the .NET check and the build are the same as when run.cmd is used directly.
pushd "%~dp0"
set "out=%TEMP%\toyrobot-menu-%RANDOM%%RANDOM%.out"
set "empties=0"

:menu
echo.
echo Toy Robot - choose what to run
echo.
echo   Run a test-data file (shows what the app prints, then PASS or FAIL against the expected output)
echo    1  challenge-example-1
echo    2  challenge-example-2
echo    3  challenge-example-3
echo    4  bom-challenge-example-1
echo    5  exit-mid-file
echo    6  empty
echo    7  long-session
echo    8  all seven files, one after another
echo.
echo   Type commands yourself
echo    9  start a typed session (end with EXIT, or Ctrl+Z then Enter; you come back to this menu)
echo.
echo   Automated tests
echo   10  run all tests (dotnet test)
echo.
echo    0  quit
echo.
set "choice="
set /p "choice=Choose: "
rem An empty line, or the end of redirected input, shows the menu again; three in a row end the menu.
if not defined choice goto :nochoice
set "empties=0"
if "%choice%"=="0" goto :quit
if "%choice%"=="1" call :file challenge-example-1 & goto :menu
if "%choice%"=="2" call :file challenge-example-2 & goto :menu
if "%choice%"=="3" call :file challenge-example-3 & goto :menu
if "%choice%"=="4" call :file bom-challenge-example-1 & goto :menu
if "%choice%"=="5" call :file exit-mid-file & goto :menu
if "%choice%"=="6" call :file empty & goto :menu
if "%choice%"=="7" call :file long-session & goto :menu
if "%choice%"=="8" goto :all
if "%choice%"=="9" goto :typed
if "%choice%"=="10" goto :tests
echo Not a choice: %choice%
goto :menu

:nochoice
set /a empties+=1
if %empties% GEQ 3 goto :quit
goto :menu

:all
set "failed=0"
for %%f in (challenge-example-1 challenge-example-2 challenge-example-3 bom-challenge-example-1 exit-mid-file empty long-session) do call :file %%f
echo.
if "%failed%"=="0" (echo All 7 files PASS) else (echo %failed% of 7 files FAIL)
goto :menu

:typed
echo.
call "%~dp0run.cmd"
echo.
echo Typed session ended.
goto :menu

:tests
echo.
dotnet test ToyRobot.sln --nologo < nul
goto :menu

:quit
if exist "%out%" del "%out%"
popd
exit /b 0

:file
rem %1 is a test-data name: run testdata\%1.txt, show its console output (stdout) and compare it with testdata\%1.expected.txt.
echo.
echo === %1 ===
rem The file run gets no console input (< nul), so it cannot read the menu's own input.
call "%~dp0run.cmd" testdata\%1.txt < nul > "%out%"
set "rc=%ERRORLEVEL%"
echo --- console output (stdout):
type "%out%"
rem fc reads console input too, so it also gets < nul.
fc "%out%" testdata\%1.expected.txt < nul > nul 2>&1
if errorlevel 1 goto :filediff
if not "%rc%"=="0" goto :filecode
echo PASS: %1
exit /b 0

:filediff
echo FAIL: %1 - console output differs from testdata\%1.expected.txt
set /a failed+=1
exit /b 0

:filecode
echo FAIL: %1 - exit code %rc%, expected 0
set /a failed+=1
exit /b 0
