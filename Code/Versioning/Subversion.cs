using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
namespace Launcher.Versioning{
	public static class Subversion{
		public static string path = AppDomain.CurrentDomain.BaseDirectory+"Subversion 1.14.1\\bin\\";
		public static Process process = new();
		public static StringBuilder output = new();
		public static string Call(string parameter,DataReceivedEventHandler onReceive=null,EventHandler onExit=null){
			var resetEvent = new ManualResetEvent(false);
			Subversion.process = new();
			Subversion.process.StartInfo = new(Subversion.path+"svn.exe");
			Subversion.process.StartInfo.Arguments = parameter;
			Subversion.process.StartInfo.CreateNoWindow = true;
			Subversion.process.StartInfo.UseShellExecute = false;
			Subversion.process.StartInfo.RedirectStandardOutput = true;
			Subversion.process.EnableRaisingEvents = true;
			Subversion.process.OutputDataReceived += (sender,parameters)=>{
				if(parameters.Data == null){resetEvent.Set();}
				else{Subversion.output.AppendLine(parameters.Data);}
				if(MainWindow.self.process.HasExited){Subversion.process.Kill();}
			};
			Subversion.process.Exited += (a,b)=>Subversion.process.CancelOutputRead();
			if(onReceive != null){Subversion.process.OutputDataReceived += onReceive;}
			if(onExit != null){Subversion.process.Exited += onExit;}
			Subversion.output.Clear();
			Subversion.process.Start();
			Subversion.process.BeginOutputReadLine();
			return Subversion.output.ToString();
		}
	}
}
