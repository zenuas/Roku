@if (0)==(0) echo off
cscript.exe //nologo //E:JScript "%~f0" %*
exit /B %ERRORLEVEL%
@end

var fs = WScript.CreateObject("Scripting.FileSystemObject");

var opt  = {num : 10};
var args = [];
for(var i = 0; i < WScript.Arguments.Length; i++)
{
	args.push(WScript.Arguments(i));
}
while(args.length > 0)
{
	if(args.length >= 2 && args[0] == "-n" && args[1].substr(0, 1) == "+") {opt.num = -(args[1] - 0); args.shift(); args.shift();}
	else if(args.length >= 2 && args[0] == "-n") {opt.num = args[1] - 0; args.shift(); args.shift();}
	else
	{
		break;
	}
}
WScript.Quit(main(args));

function main(args)
{
	var stdin = args.length > 0 ? fs.GetFile(args[0]).OpenAsTextStream(1) : WScript.StdIn;
	if(opt.num >= 0)
	{
		var buffer = [];
		while(!stdin.AtEndOfStream)
		{
			buffer.push(stdin.ReadLine());
			if(buffer.length > opt.num) {buffer.shift();}
		}
		
		for(var i = 0; i < buffer.length; i++)
		{
			WScript.Echo(buffer[i]);
		}
	}
	else
	{
		var max = -opt.num;
		var buffer = [];
		while(!stdin.AtEndOfStream)
		{
			buffer.push(stdin.ReadLine());
		}
		
		for(var i = max; i < buffer.length; i++)
		{
			WScript.Echo(buffer[i]);
		}
	}
	return(0);
}
