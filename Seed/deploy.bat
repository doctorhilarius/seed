@ECHO off


IF [%1] == [] (
	ECHO no app pool name
) ELSE (
	SET apppool=%1
	ECHO apppool = %apppool%
)

ECHO DEPLOYING API

dotnet restore
IF %ERRORLEVEL% NEQ 0 (
	ECHO dotnet restore failed
	GOTO :error
)

IF [%1] == [] (
	ECHO skipped stopping app pool
) ELSE (
	C:\Windows\System32\inetsrv\appcmd.exe stop apppool /apppool.name:%apppool%
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO apppool could not be stopped, proceeding anyway
)

dotnet build /p:DeployOnBuild=true /p:PublishProfile=Release
if %ERRORLEVEL% NEQ 0 (
	ECHO dotnet build and deploy failed
	GOTO :error
)

IF [%1] == [] (
	ECHO skipped starting app pool
) ELSE (
	C:\Windows\System32\inetsrv\appcmd.exe start apppool /apppool.name:%apppool%
)
if %ERRORLEVEL% NEQ 0 (
	ECHO apppool could not be started, proceeding anyway
)

icacls ../DeployPackage /grant NetworkService:F /T
if %ERRORLEVEL% NEQ 0 (
	ECHO full control could not be granted for NetworkService
	GOTO :error
)

ECHO installation succeeded
GOTO :exit

:error
ECHO installation failed

:exit