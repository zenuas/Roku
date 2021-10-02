#
# usage: make.bat
#

PATH:=bin;$(WINDIR)\Microsoft.NET\Framework64\v4.0.30319;$(PATH)
TESTS:=$(wildcard test\rk\*.rk)

YANP=..\Yanp\bin\Debug\yanp.exe
YANP_OUT=src\Parser\Parser.cs

.PHONY: all clean distclean release test testd parser parserd fetchil metric

all:
	dotnet build --nologo -v q

clean:
	dotnet clean --nologo -v q

distclean: clean
	-rmdir /S /Q src\bin 2>NUL
	-rmdir /S /Q src\obj 2>NUL
	-rmdir /S /Q test\bin 2>NUL
	-rmdir /S /Q test\obj 2>NUL
	-rmdir /S /Q test\rk\obj 2>NUL

release:

parser: $(YANP_OUT)

$(YANP_OUT): src\roku.y
	@$(MAKE) parserd

parserd:
	#cd ..\Yanp && msbuild Yanp.sln /nologo /v:q /t:build /p:Configuration=Debug
	$(YANP) \
		-i src\\roku.y \
		-v src\\roku.txt \
		-c src\\roku.csv \
		-p src\\Parser\\ \
		-b ..\\Yanp \
		-t cs
	
	-find /n "/reduce" < src\roku.txt

test: testd $(TESTS)

testd:
	-@dotnet test --nologo | tail -n +7 | head -n -3

$(TESTS):
	@$(set RK_OBJ=$(subst \rk\,\rk\obj\,$(patsubst %.rk,%,$@)))
	
	@$(echo $(cat $(RK_OBJ).testname utf-8))
	ifeq $(exists $(RK_OBJ).testskip) 0
		-@if exist $(RK_OBJ).il. ilasm $(RK_OBJ).il /out:$(RK_OBJ).dll /quit /dll
		@copy rk.test.runtimeconfig.json $(RK_OBJ).runtimeconfig.json 1>NUL
		-@if exist $(RK_OBJ).dll. dotnet $(RK_OBJ).dll < $(RK_OBJ).testin > $(RK_OBJ).stdout
		@fc $(RK_OBJ).testout $(RK_OBJ).stdout > $(RK_OBJ).diff || type $(RK_OBJ).diff
	endif

fetchil:
	@echo $(TESTS) | xargs -n 1 -t cscript /nologo bin\fetchil.js

metric:
	dotnet msbuild /t:metrics src\Roku.csproj -nologo
	metrics.xml_to_csv.bat src\Roku.Metrics.xml > src\Roku.Metrics.csv
	start src\Roku.Metrics.csv
