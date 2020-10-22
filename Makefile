#
# usage: make.bat
#

PATH:=$(WINDIR)\Microsoft.NET\Framework64\v4.0.30319;$(PATH)
TESTS:=$(wildcard test\rk\*.rk)

YANP=..\Yanp\bin\Debug\yanp.exe
YANP_OUT=src\Parser\Parser.cs

.PHONY: all clean distclean release test testd parser parserd node metric

all:
	dotnet build --nologo

clean:
	dotnet clean --nologo

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
	-@dotnet test --nologo

$(TESTS):
	@echo $(subst test\rk\,,$(patsubst %.rk,%,$@))
	-@if exist $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.il,$@)). ilasm $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.il,$@)) /out:$(subst \rk\,\rk\obj\,$(patsubst %.rk,%.dll,$@)) /quit /dll
	@copy rk.test.runtimeconfig.json $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.runtimeconfig.json,$@)) 1>NUL
	-@if exist $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.dll,$@)). dotnet $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.dll,$@)) < $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.testin,$@)) > $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.stdout,$@))
	@fc $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.testout,$@)) $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.stdout,$@)) > $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.diff,$@)) || type $(subst \rk\,\rk\obj\,$(patsubst %.rk,%.diff,$@))

metric:
	dotnet msbuild /t:metrics src\Roku.csproj -nologo
	metrics.xml_to_csv.bat src\Roku.Metrics.xml > src\Roku.Metrics.csv
	start src\Roku.Metrics.csv
