using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Launcher{
	public class Options : Dictionary<string,string>{
		public static Options current = new();
		public Options(){
			//Defaults here
			this["WindowTitle"] = "DefaultName";
			this["IconPath"] = "Images/Icon.png";
			this["BackgroundPath"] = "Images/Background.png";
			this["Repository"] = "";
			this["Executable"] = "DefaultApp.exe";
			this["BuildPath"] = "DefaultName";
			this["Version"] = "";
		}
		public bool Load(string path){
			path = Path.GetFullPath(path);
			if(!File.Exists(path)){return false;}
			foreach(var line in File.ReadAllLines(path)){
				var tokens = line.Split("=").Select(x=>x.Trim()).ToArray();
				if(tokens.Length > 1){this[tokens[0]] = tokens[1];}
			}
			return true;
		}
		public void Save(string path){
			var serialized = this.Select(x=>x.Key+" = "+x.Value);
			File.WriteAllLines(Path.GetFullPath(path),serialized);
		}
	}
}
