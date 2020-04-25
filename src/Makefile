#
# usage: make.bat
#

PATH:=build-tools;$(PATH);
RELEASE=Release
OUT=bin\$(RELEASE)\netcoreapp3.1\roku.exe
VBFLAGS=/debug-

YANP=..\..\Yanp\bin\Debug\yanp.exe
YANP_OUT=Parser\Parser.cs

.PHONY: all clean distclean release test parser parserd node

all: $(OUT)

clean:
	dotnet clean

distclean: clean
	rmdir /S /Q bin 2>NUL || exit /B 0
	rmdir /S /Q obj 2>NUL || exit /B 0

release:

test:

parser: $(YANP_OUT)

$(YANP_OUT): roku.y
	@$(MAKE) parserd

parserd:
	#cd ..\..\Yanp && msbuild Yanp.sln /nologo /v:q /t:build /p:Configuration=Debug
	$(YANP) \
		-i roku.y \
		-v .\\roku.txt \
		-c .\\roku.csv \
		-p .\\Parser\\ \
		-b ..\\..\\Yanp \
		-t cs
	
	find /n "/reduce" < .\roku.txt || exit /B 0
