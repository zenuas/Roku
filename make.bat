@setlocal enabledelayedexpansion
@set PREVPROMPT=%PROMPT%
@prompt $E[1A
@set MAKE=make.bat
@echo on

@if "%1" == "" (set TARGET=build
) else (set TARGET=%1 && shift)

@call :%TARGET% %1 %2 %3 %4 %5 %6 %7 %8 %9
@prompt %PREVPROMPT%
@exit /b %ERRORLEVEL%

:build
	@call :parser
	dotnet build --nologo -v q --clp:NoSummary
	@exit /b %ERRORLEVEL%

:clean
	dotnet clean --nologo -v q
	@exit /b %ERRORLEVEL%

:distclean
	@call :clean
	rmdir /S /Q src\bin          2>nul
	rmdir /S /Q src\obj          2>nul
	rmdir /S /Q test\bin         2>nul
	rmdir /S /Q test\obj         2>nul
	rmdir /S /Q test-compile\bin 2>nul
	rmdir /S /Q test-compile\obj 2>nul
	rmdir /S /Q test-rk\obj      2>nul
	@exit /b %ERRORLEVEL%

:release
	git archive HEAD --output=Roku-%DATE:/=%.zip
	
	dotnet publish src --nologo -v q --clp:NoSummary -c Release -o .tmp
	powershell -NoProfile $ProgressPreference = 'SilentlyContinue' ; Compress-Archive -Force -Path .tmp\*, README.md, LICENSE -DestinationPath Roku-bin-%DATE:/=%.zip
	rmdir /S /Q .tmp 2>nul
	
	@exit /b %ERRORLEVEL%

:test
	dotnet test --nologo -v q --filter DisplayName!=Roku.Tests.FrontEndTest.CompileTest
	@call :testa
	@exit /b %ERRORLEVEL%

:testf
	dotnet test --nologo -v q
	del /F /Q test-rk\obj\*
	@call :testa -f true
	@exit /b %ERRORLEVEL%

:testa
	@pushd test-rk
	dotnet run --project ..\test-compile -c Release -- %*
	@popd
	@exit /b %ERRORLEVEL%

:parser
	@call :comparedate src\roku.y src\Parser\Parser.cs
	@if ERRORLEVEL 1 (
		call :parserd
	)
	@exit /b %ERRORLEVEL%

:parserd
	dotnet run --project ..\Yanp\src -- src\roku.y -o src\Parser -t ..\Yanp\template.cs -l 0 -v
	find /n "/reduce" < src\Parser\verbose.v.txt
	@exit /b %ERRORLEVEL%

:comparedate
	@powershell -Command "exit (Get-ItemProperty %1).LastWriteTime -gt (Get-ItemProperty %2).LastWriteTime"
	@exit /b %ERRORLEVEL%
