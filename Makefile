#
# usage: make.bat
#

PATH:=bin;$(USERPROFILE)\.nuget\packages\microsoft.netcore.ilasm\6.0.0\runtimes\native;$(PATH)
TESTS:=$(wildcard test\rk\*.rk)

YANP=..\Yanp\bin\Debug\yanp.exe
YANP_OUT=src\Parser\Parser.cs

.PHONY: all clean distclean release test testf testd testdf testa parser parserd fetchil metric

all:
	dotnet build --nologo -v q --clp:NoSummary

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
	dotnet run --project ..\Yanp\src src\roku.y -o src\Parser -t ..\Yanp\template.cs -l 0
	-find /n "/reduce" < src\Parser\verbose.txt

test:  testd  testa
testf: testdf testa

testd:
	-@dotnet test --nologo | tail -n +7 | head -n -3
	@$(set RK_SKIP=$(cat test\rk\obj\.success utf-8))

testdf:
	-@dotnet test --nologo -s test\none-skip.runsettings | tail -n +7 | head -n -3

testa: $(TESTS)

$(TESTS):
	@$(set RK_OBJ=$(subst \rk\,\rk\obj\,$(patsubst %.rk,%,$@)))
	@$(set RK_NAME=$(subst test\rk\,,$(patsubst %.rk,%,$@)))
	
	ifeq $(regex "$(RK_SKIP)" "^$(RK_NAME)\b.*$" m) 0
		@$(echo $(cat $(RK_OBJ).testname utf-8))
		-@if exist $(RK_OBJ).il. ilasm $(RK_OBJ).il /out:$(RK_OBJ).exe /quit /exe
		-@if exist $(RK_OBJ).exe. $(RK_OBJ).exe < $(RK_OBJ).testin > $(RK_OBJ).stdout
		@fc $(RK_OBJ).testout $(RK_OBJ).stdout > $(RK_OBJ).diff || type $(RK_OBJ).diff
	else
		@$(echo $("$&"))
	endif

fetchil:
	@echo $(TESTS) | xargs -n 1 -t cscript /nologo bin\fetchil.js

metric:
	dotnet msbuild /t:metrics src\Roku.csproj -nologo
	metrics.xml_to_csv.bat src\Roku.Metrics.xml > src\Roku.Metrics.csv
	start src\Roku.Metrics.csv
