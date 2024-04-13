using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using TestWebAPI.Common.Types;

string webComponentPath = @"..\..\..\..\TestWebAPI.MyWebApi\bin\Debug\net8.0\TestWebAPI.MyWebApi.dll";

//var ctx = new System.Runtime.Loader.AssemblyLoadContext(true);

//var asm = System.Reflection.Assembly.LoadFrom(webComponentPath);
//var t = (from type in asm.GetTypes()
//         where typeof(IDoSomething).IsAssignableFrom(type)
//         select type).ToList();
//var obj = Activator.CreateInstance(t[0]);
//var tobj = obj as IDoSomething;
//if (tobj != null) await tobj.DoSomething();
//var x = 0;

var myProcess = new Process()
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "dotnet.exe",
        Arguments = Path.Combine(Environment.CurrentDirectory, webComponentPath),
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    }
};

myProcess.ErrorDataReceived += (_, e) => Console.Out.WriteLine(e.Data);
myProcess.OutputDataReceived += (_, e) => Console.Out.WriteLine(e.Data);
myProcess.EnableRaisingEvents = true;

Console.CancelKeyPress += (_, e) =>
{
    if (!myProcess.HasExited)
        myProcess.Kill(true);
    e.Cancel = false;
};

AppDomain.CurrentDomain.ProcessExit += (_, e) =>
{
    if (!myProcess.HasExited)
        myProcess.Kill(true);
    if (myProcess is not null)
        myProcess.Dispose();
};

myProcess.Exited += (_, e) =>
{
    Console.WriteLine("Process {0} has exited {1}.", myProcess.Id, myProcess.ExitCode);
};

myProcess.StartInfo.Environment.Clear();
myProcess.StartInfo.Environment.Add("SystemRoot", @"C:\Windows");
myProcess.StartInfo.Environment.Add("ASPNETCORE_URLS", "http://localhost:5005");

myProcess.Start();
myProcess.BeginErrorReadLine();
myProcess.BeginOutputReadLine();
await myProcess.WaitForExitAsync();

Console.WriteLine("Exiting");