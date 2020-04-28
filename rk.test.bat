@prompt $$$S
@echo off

set PATH=%PATH%;^
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319

for %%v in (test\rk\obj\*.il) do (
	call :il "%%v"
)

exit /B 0

:il
	set IL=%1
	set ILOUT=%IL:.il=.dll%
	
	ilasm %IL% /out:%ILOUT% /quit /dll
	copy rk.test.runtimeconfig.json %IL:.il=%.runtimeconfig.json 1>NUL
	
	echo ---
	echo %~n1
	dotnet %ILOUT%
	exit /B 0
