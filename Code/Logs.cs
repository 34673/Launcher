using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Documents;
using System.Windows;
namespace Launcher{
	public static class Logs{
		public static List<string> raw = new();
		public static Dictionary<string,List<Run>> formatted = new();
		public static string path = "Changelogs.json";
		static Logs(){Logs.formatted[""] = new(){new("Unknown Version")};}
		public static bool Deserialize(){
			if(!Logs.path.EndsWith(".json")){Logs.path += ".json";}
			var path = Path.GetFullPath(Logs.path);
			if(!File.Exists(path)){return false;}
			var file = File.OpenRead(path);
			var deserialized = JsonSerializer.Deserialize<List<string>>(file);
			file.Close();
			if(deserialized == null){return false;}
			deserialized.ForEach(x=>Logs.Format(x));
			return true;
		}
		public static void Add(string logs){
			var separator = "------------------------------------------------------------------------"+Environment.NewLine;
			var raw = logs.Split(separator).Where(x=>x!="").ToList();
			var serialized = JsonSerializer.Serialize(raw);
			if(File.Exists(Logs.path)){
				var file = File.OpenWrite(Logs.path);
				file.SetLength(file.Length-1);
				file.Close();
				serialized = ","+serialized.TrimStart('[');
			}
			File.AppendAllText(Logs.path,serialized);
			raw.ForEach(x=>Logs.Format(x));
		}
		public static List<Run> Format(string raw){
			var headerEnd = raw.IndexOf(Environment.NewLine);
			var header = raw[0..headerEnd];
			var content = raw.Replace(header,"");
			var headerItems = header.Split("|").Select(x=>x.Trim()).ToArray();
			var revision = headerItems[0].Replace("r","");
			var author = headerItems[1];
			var date = headerItems[2].Split(" ").First().Split("-");
			var dateConverter = CultureInfo.CurrentCulture.DateTimeFormat;
			var sections = new List<Run>();
			var revisionSection = new Run("Revision "+revision+"\n");
			revisionSection.FontWeight = FontWeights.Bold;
			revisionSection.FontStyle = FontStyles.Italic;
			revisionSection.FontSize = 30;
			sections.Add(revisionSection);
			var metadata = author+" - "+dateConverter.GetMonthName(int.Parse(date[1]))+" "+date[2]+", "+date[0]+"\n";
			var metadataSection = new Run(metadata);
			metadataSection.FontStyle = FontStyles.Italic;
			metadataSection.FontSize = 20;
			sections.Add(metadataSection);
			foreach(var line in content.ReplaceLineEndings("\n").Split('\n')){
				//Implement log message formating.
				//Content should end in a list of Runs to account for all formatted effects.
				//Rules:
				//Bold • replaces --
				//[] set wrapped text to bold
				//`` (not " or ') set wrapped text background for highlighting
			}
			var contentSection = new Run(content);
			contentSection.FontSize = 15;
			sections.Add(contentSection);
			return Logs.formatted[revision] = sections;
		}
		public static List<Run> FormatUpdates(){
			//Idea: remove prefixes and color lines based on their prefix
			//A = Added,U = Updated,D = Deleted
			return default;
		}
	}
}
