@prompt $$$S
@echo off
setlocal

set PATH=%PATH%;^
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319

for %%v in (test\rk\*.rk) do (
	call :il "%%v"
)

exit /B 0

:il
	set RK=%1
	set IL=%~dp1\obj\%~n1.il
	set FILENAME=%IL:.il=%
	set ILOUT=%FILENAME%.dll
	set TESTIN=%FILENAME%.testin
	set TESTOUT=%FILENAME%.testout
	set TESTERR=%FILENAME%.testerr
	set TESTARGS=%FILENAME%.testargs
	set STDOUT=%FILENAME%.stdout
	set STDERR=%FILENAME%.stderr
	set DIFF=%FILENAME%.diff
	
	echo %~n1
	if not exist "%IL%" (
		echo   not found
		exit /B0
	)
	
	ilasm %IL% /out:%ILOUT% /quit /dll
	if not exist "%ILOUT%" (
		echo   compile error
		exit /B0
	)
	copy rk.test.runtimeconfig.json %FILENAME%.runtimeconfig.json 1>NUL
	
	dotnet %ILOUT% < %TESTIN% > %STDOUT%
	fc %TESTOUT% %STDOUT% >%DIFF% || type %DIFF%
	exit /B 0
