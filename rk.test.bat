@prompt $$$S
@echo off
setlocal

set PATH=%PATH%;^
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319

for %%v in (test\rk\obj\*.il) do (
	call :il "%%v"
)

exit /B 0

:il
	set IL=%1
	set FILENAME=%IL:.il=%
	set ILOUT=%FILENAME%.dll
	set TESTIN=%FILENAME%.testin
	set TESTOUT=%FILENAME%.testout
	set TESTERR=%FILENAME%.testerr
	set TESTARGS=%FILENAME%.testargs
	set STDOUT=%FILENAME%.stdout
	set STDERR=%FILENAME%.stderr
	set DIFF=%FILENAME%.diff
	
	ilasm %IL% /out:%ILOUT% /quit /dll
	copy rk.test.runtimeconfig.json %FILENAME%.runtimeconfig.json 1>NUL
	
	echo %~n1
	dotnet %ILOUT% < %TESTIN% > %STDOUT%
	fc %TESTOUT% %STDOUT% >%DIFF% || type %DIFF%
	exit /B 0
