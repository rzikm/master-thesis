@echo off

:: Default to highest Visual Studio version available
::
:: For VS2017 and later, multiple instances can be installed on the same box SxS and VSxxxCOMNTOOLS is only set if the user
:: has launched the VS2017 or VS2019 Developer Command Prompt.
::
:: Following this logic, we will default to the VS2019 toolset if VS160COMMONTOOLS tools is
:: set, as this indicates the user is running from the VS2019 Developer Command Prompt and
:: is already configured to use that toolset. Otherwise, we will fail the script if no supported VS instance can be found.

if defined VisualStudioVersion goto :RunVCVars

set __VCBuildArch=x86_amd64

set _VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %_VSWHERE% (
  for /f "usebackq tokens=*" %%i in (`%_VSWHERE% -latest -prerelease -property installationPath`) do set _VSCOMNTOOLS=%%i\Common7\Tools
)
if not exist "%_VSCOMNTOOLS%" goto :MissingVersion

call "%_VSCOMNTOOLS%\VsDevCmd.bat" -no_logo

:RunVCVars
if "%VisualStudioVersion%"=="16.0" (
    goto :VS2019
) else if "%VisualStudioVersion%"=="15.0" (
    goto :VS2017
)

:MissingVersion
:: Can't find VS 2019
echo Error: Visual Studio 2019 required
exit /b 1

:VS2019
:: Setup vars for VS2019
set __VSVersion=vs2019
set __PlatformToolset=v142
:: Set the environment for the native build
call "%VS160COMNTOOLS%..\..\VC\Auxiliary\Build\vcvarsall.bat" %__VCBuildArch%
goto DoSetup

:DoSetup

SET ROOT=%~dp0

powershell %ROOT%\setup.ps1 %*
