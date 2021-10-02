@if (0)==(0) echo off
cscript.exe //nologo //E:JScript "%~f0" %*
exit /B %ERRORLEVEL%
@end

var sh = WScript.CreateObject("WScript.Shell");

Array.prototype.map = function (f) {var xs = []; for(var i = 0; i < this.length; i++) {xs.push(f(this[i]));} return(xs);};

var opt  = {num : -1, quot : false, noquot : false, verbose : false};
var args = [];
for(var i = 0; i < WScript.Arguments.Length; i++)
{
	args.push(WScript.Arguments(i));
}
while(args.length > 0)
{
	if(args.length >= 2 && args[0] == "-n") {opt.num = args[1] - 0; args.shift(); args.shift();}
	else if(args[0] == "-q") {opt.quot    = true; args.shift();}
	else if(args[0] == "-Q") {opt.noquot  = true; args.shift();}
	else if(args[0] == "-t") {opt.verbose = true; args.shift();}
	else
	{
		break;
	}
}
WScript.Quit(main(args));

function main(args)
{
	var stdin = WScript.StdIn;
	var xs    = [];
	if(stdin.AtEndOfStream)
	{
		exec(args);
	}
	else
	{
		while(!stdin.AtEndOfStream)
		{
			Array.prototype.push.apply(xs, command_split(stdin.ReadLine()));
			while(opt.num > 0 && opt.num <= xs.length)
			{
				exec(args.concat(xs.slice(0, opt.num)));
				xs = xs.slice(opt.num);
			}
		}
		if(xs.length > 0) {exec(args.concat(xs));}
	}
	return(0);
}

function exec(args)
{
	var cmd = args.map(function(x) {return(escape(x));}).join(" ");
	if(opt.verbose) {WScript.Echo(cmd);}
	var exec = sh.Exec("cmd /d /c " + cmd + " 2>&1")
	while(!exec.StdOut.AtEndOfStream)
	{
		WScript.Echo(exec.StdOut.ReadLine());
	}
}

function escape(s)
{
	if(!opt.noquot && (s.indexOf(" ") >= 0 || opt.quot))
	{
		return("\"" + s + "\"");
	}
	else
	{
		return(s);
	}
}

function command_split(s, splitter)
{
	splitter = splitter || " ";
	
	var quote   = [];
	var xs      = [];
	var command = "";
	
	for(var i = 0; i < s.length; i++)
	{
		var c = s.substring(i, i + 1);
		if(quote.length == 0 && c == splitter)
		{
			if(splitter != " " || command != "")
			{
				xs.push(command);
			}
			command = "";
		}
		else if(quote.length > 0 && c == quote[quote.length - 1])
		{
			command += c;
			quote.pop();
		}
		else
		{
			if(c == "\"" || c == "'")
			{
				quote.push(c);
			}
			command += c;
		}
	}
	//if(quote.length > 0) {throw new Error("quote error [" + s + "]");}
	
	if(command != "") {xs.push(command);}
	
	return(xs);
}
