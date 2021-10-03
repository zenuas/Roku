@if (0)==(0) echo off
cscript.exe //nologo //E:JScript "%~f0" %*
exit /B %ERRORLEVEL%
@end

WScript.Quit(main(WScript.Arguments));

function main(args)
{
	var dom = WScript.CreateObject("Msxml2.DOMDocument");
	
	dom.load(args(0));
	var xs = dom.documentElement.getElementsByTagName("Metrics");
	WScript.Echo("タイプ,名前,保守容易性指数,サイクロマティック複雑度,クラス結合度,継承の深さ,コード行,実行可能コード行");
	for(var i = 0; i < xs.length; i++)
	{
		var metrics = xs[i];
		var metric  = metrics.getElementsByTagName("Metric");
		var mx = {
				TagName : "",
				Name : "",
				MaintainabilityIndex : 0,
				CyclomaticComplexity : 0,
				ClassCoupling : 0,
				DepthOfInheritance : 0,
				SourceLines : 0,
				ExecutableLines : 0
			};
		
		mx["TagName"] = metrics.parentNode.tagName;
		mx["Name"]    = metrics.parentNode.getAttribute("Name");
		for(var j = 0; j < metric.length; j++)
		{
			var m = metric[j];
			mx[m.getAttribute("Name")] = m.getAttribute("Value") - 0;
		}
		WScript.Echo(
				mx["TagName"] + "," +
				"\"" + mx["Name"] + "\"," +
				mx["MaintainabilityIndex"] + "," +
				mx["CyclomaticComplexity"] + "," +
				mx["ClassCoupling"] + "," +
				mx["DepthOfInheritance"] + "," +
				mx["SourceLines"] + "," +
				mx["ExecutableLines"]
			);
	}
}
