using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
namespace Launcher.Versioning{
	public class Versioning{
		public string toolPath;
		public string repository;
		public string localCopy;
		public string version;
		public Dictionary<Process,StringBuilder> outputs = new();
		public virtual (Process task,StringBuilder output) GetRepository(string version="",DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){return default;}
		public virtual (Process task,StringBuilder output) ShowLocalVersion(DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){return default;}
		public virtual (Process task,StringBuilder output) ShowLatestVersion(DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){return default;}
		public virtual (Process task,StringBuilder output) Update(string version="",DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){return default;}
		public virtual (Process task,StringBuilder output) Revert(string path,bool recursive=false,DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){return default;}
		public virtual (Process task,StringBuilder output) Cleanup(string path,DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){return default;}
		public virtual (Process task,StringBuilder output) GetLogs(string version="",string toVersion="",DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){return default;}
		public virtual string LocateCache(string localRoot){return default;}
		public (Process task,StringBuilder output) Call(string parameters,DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			var resetEvent = new ManualResetEvent(false);
			var task = new Process();
			var output = new StringBuilder();
			this.outputs[task] = output;
			task.StartInfo = new(this.toolPath);
			task.StartInfo.Arguments = parameters;
			task.StartInfo.CreateNoWindow = true;
			task.StartInfo.UseShellExecute = false;
			task.StartInfo.RedirectStandardOutput = true;
			task.EnableRaisingEvents = true;
			task.OutputDataReceived += (sender,parameters)=>{
				if(parameters.Data == null){resetEvent.Set();}
				else{output.AppendLine(parameters.Data);}
				if(MainWindow.self.process.HasExited){task.Kill();}
			};
			task.Exited += (a,b)=>{
				task.CancelOutputRead();
				this.outputs.Remove(task);
			};
			if(onExit != null){task.Exited += onExit;}
			if(onError != null){task.ErrorDataReceived += onError;}
			if(onReceive != null){task.OutputDataReceived += onReceive;}
			task.Start();
			task.BeginOutputReadLine();
			return (task,output);
		}
	}
}