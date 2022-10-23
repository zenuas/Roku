
var fs = WScript.CreateObject("Scripting.FileSystemObject");

var args = [];
for(var i = 0; i < WScript.Arguments.Length; i++)
{
	var arg = WScript.Arguments(i);
	args.push(arg);
}
run(args);
WScript.Quit(0);

function run(args)
{
	var front  = "";
	var middle = "";
	var end    = "";
	
	var sep = args[0].lastIndexOf("\\");
	var dir = args[0].substr(0, 1 + sep) + "obj\\";
	var il  = args[0].substr(sep + 1, args[0].length - sep - 4) + ".il";
	if(!fs.FileExists(dir + il)) return;
	
	var rk = read(args[0], "UTF-8").split("\r\n");
	var current = 0;
	for(var i = 0; i < rk.length; i++)
	{
		if(i == rk.length - 1 && rk[i] == "") break;
		var s = rk[i];
		
		if(current == 0) front += s + "\r\n";
		if(current == 1) middle += s;
		
		if(current == 0 && s == "###start") current++;
		if(current == 0 && s == "###failed") return;
		if(current == 0 && s == "###error") return;
		if(current == 1 && s == "###end") current++;
		
		if(current == 2) end += s + "\r\n";
	}
	
	middle = read(dir + il, "UTF-8");
	write(args[0], "UTF-8", front + middle + end);
}

function read(filename, charset)
{
	charset = charset || "_autodetect_all";
	
	var stream = WScript.CreateObject("ADODB.Stream");
	stream.Type = 2; // adTypeText
	stream.charset = charset;
	stream.Open();
	try
	{
		stream.LoadFromFile(filename);
		return(stream.ReadText(-1)); // adReadAll
	}
	finally
	{
		stream.Close();
	}
}

function write(filename, charset, text)
{
	charset = charset || "_autodetect_all";
	if(charset == "UTF-8") return write_utf8trimbom(filename, text);
	
	var stream = WScript.CreateObject("ADODB.Stream");
	stream.Type = 2; // adTypeText
	stream.charset = charset;
	stream.Open();
	try
	{
		stream.WriteText(text, 0); // adWriteChar
		stream.SaveToFile(filename, 2); // adSaveCreateOverWrite
	}
	finally
	{
		stream.Close();
	}
}

function write_utf8trimbom(filename, text)
{
	var stream = WScript.CreateObject("ADODB.Stream");
	stream.Type = 2; // adTypeText
	stream.charset = "UTF-8";
	var utf8bin;
	stream.Open();
	try
	{
		stream.WriteText(text, 0); // adWriteChar
		stream.Position = 0;
		stream.Type = 1; // adTypeBinary
		stream.Position = 3;
		utf8bin = stream.Read();
	}
	finally
	{
		stream.Close();
	}
	
	var stream2 = WScript.CreateObject("ADODB.Stream");
	stream2.Type = 1; // dTypeBinary
	stream2.Open();
	try
	{
		stream2.Write(utf8bin);
		stream2.SaveToFile(filename, 2); // adSaveCreateOverWrite
	}
	finally
	{
		stream2.Close();
	}
}
