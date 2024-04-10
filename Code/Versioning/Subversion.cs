using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace Launcher.Versioning{
	public class Subversion : Versioning{
		public Subversion(string repository,string localCopy=""){
			this.repository = repository;
			this.localCopy = localCopy;
			this.toolPath = Path.GetFullPath(Options.current["SubversionPath"]);
		}
		public override (Process task,StringBuilder output) GetRepository(string version="HEAD",DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			var command = $"checkout --revision {version} {this.repository} {this.localCopy}";
			return this.Call(command,onReceive,onExit);
		}
		public override (Process task,StringBuilder output) ShowLocalVersion(DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			var command = "info --show-item=last-changed-revision "+this.localCopy;
			return this.Call(command,onReceive,onExit);
		}
		public override (Process task,StringBuilder output) ShowLatestVersion(DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			var command = "info --show-item=last-changed-revision "+this.repository;
			return this.Call(command,onReceive,onExit);
		}
		public override (Process task,StringBuilder output) Update(string version="HEAD",DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			var command = $"update --revision {version} {this.localCopy}";
			return this.Call(command,onReceive,onExit);
		}
		public override (Process task,StringBuilder output) Revert(string path,bool recursive=false,DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			var command = "revert ";
			command += recursive ? "--recursive " : "";
			command += path;
			return this.Call(command,onReceive,onExit);
		}
		public override (Process task,StringBuilder output) Cleanup(string path,DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			var command = "cleanup "+path;
			return this.Call(command,onReceive,onExit);
		}
		public override (Process task,StringBuilder output) GetLogs(string version="1",string toVersion="HEAD",DataReceivedEventHandler onReceive=null,EventHandler onExit=null,DataReceivedEventHandler onError=null){
			if(version == ""){version = "1";}
			if(toVersion == ""){toVersion = "HEAD";}
			var command = $"log --revision {version}:{toVersion} {this.repository}";
			return this.Call(command,onReceive,onExit);
		}
		public override string LocateCache(string localRoot){
			return localRoot.EndsWith("/") ? localRoot+".svn/" : localRoot+"/.svn/";
		}
	}
}
