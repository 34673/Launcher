using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;
namespace Launcher{
	using Launcher.Versioning;
	public partial class MainWindow : Window{
		public static MainWindow self;
		public string basePath = AppDomain.CurrentDomain.BaseDirectory;
		public Process process = Process.GetCurrentProcess();
		public Action startClick = ()=>{};
		public Subversion versioning;
		public bool downloading;
		public MainWindow(){
			MainWindow.self = this;
			this.InitializeComponent();
			Options.current.Load("Options.config");
			var icon = new Uri(Path.GetFullPath(Options.current["IconPath"]));
			var background = new Uri(Path.GetFullPath(Options.current["BackgroundPath"]));
			this.Title = Options.current["WindowTitle"];
			this.Icon = new BitmapImage(icon);
			this.Grid.Background = new ImageBrush(new BitmapImage(background));
			this.SetupPath();
			this.SetupVersioning();
		}
		public void SetupVersioning(){
			var repository = Options.current["Repository"];
			this.LogBox.Text = "Not linked to a repository.";
			if(repository == ""){
				throw new("MainWindow.SetupVersioning(): no repository assigned.");
			}
			if(repository.StartsWith("svn://")){
				this.versioning = new Subversion(repository,this.BuildPath.Text);
			}
			//else if(repository.EndsWith(".git")){
			//	this.versioning = new Git(repository);
			//}
			if(this.versioning == null){
				throw new("MainWindow.SetupVersioning(): unknown versioning protocol.");
			}
			var error = "MainWindow.SetupVersioning(): couldn't contact the versioning server.";
			var task = this.versioning.ShowLatestVersion(onError:(a,b)=>throw new(error)).task;
			task.Exited += (a,b)=>this.Dispatcher.BeginInvoke(()=>this.SetupLogs());
		}
		public void SetupLogs(string fromVersion=""){
			var error = "MainWindow.SetupLogs(): couldn't contact the versioning server.";
			if(!Logs.Deserialize() || fromVersion != ""){
				var (task,output) = this.versioning.GetLogs(fromVersion,onError:(a,b)=>throw new(error));
				task.Exited += (a,b)=>{
					this.Dispatcher.Invoke(()=>Logs.Add(output.ToString()));
					this.SetupVersion();
				};
				return;
			}
			var localLatest = Logs.formatted.Keys.Where(x=>x!="").Max(x=>int.Parse(x)).ToString();
			var (taskB,remoteLatest) = this.versioning.ShowLatestVersion();
			taskB.Exited += (a,b)=>{
				if(localLatest == remoteLatest.ToString().Trim()){
					this.SetupVersion();
					return;
				}
				this.SetupLogs(localLatest);
			};
		}
		//=======================
		// Path
		//=======================
		public void SetupPath(){
			var path = Options.current["BuildPath"];
			var pathProperty = DependencyPropertyDescriptor.FromProperty(TextBox.TextProperty,typeof(TextBox));
			this.BuildPath.Text = path == "" ? this.basePath+"NewApplication" : Path.GetRelativePath(this.basePath,path); 
			pathProperty.AddValueChanged(this.BuildPath,(a,b)=>this.PathChanged());
			this.PathChanged();
		}
		public void PathChanged(){
			if(this.downloading){return;}
			var path = this.BuildPath.Text+"/"+Options.current["Executable"];
			var buildExists = File.Exists(Path.GetFullPath(path));
			this.Start.Content = buildExists ? "Start" : "Download";
			if(!buildExists){this.startClick = this.DownloadBuild;}
		}
		public void PathBrowserClick(object sender,RoutedEventArgs parameters){
			var browser = new WinForms.FolderBrowserDialog();
			browser.ShowDialog();
			this.BuildPath.Text = browser.SelectedPath;
		}
		//=======================
		// Start Button
		//=======================
		public void StartClick(object sender,RoutedEventArgs parameters) => this.startClick();
		public void SetCancel(Process task){
			var color = Color.FromArgb(255,181,181,180);
			this.downloading = true;
			this.Start.Content = "Cancel";
			this.Start.Background = new SolidColorBrush(color);
			this.startClick = task.Kill;
		}
		public void DownloadBuild(){
			var cache = this.versioning.LocateCache(Path.GetFullPath(Options.current["BuildPath"]));
			var files = new DirectoryInfo(cache);
			if(files.Exists){
				files.GetFiles("*",SearchOption.AllDirectories).ToList().ForEach(x=>x.Attributes = FileAttributes.Normal);
				files.Delete(true);
			}
			var (task,output) = this.versioning.GetRepository(this.Version.Text,onExit:this.DownloadEnded);
			task.OutputDataReceived += (a,b)=>this.GetDownloadLogs(output.ToString());
			this.SetCancel(task);
		}
		public void UpdateBuild(){
			var cleanup = this.versioning.Cleanup(Path.GetFullPath(Options.current["BuildPath"])).task;
			var action = ()=>{
				var (task,output) = this.versioning.Update(this.Version.Text,onExit:this.DownloadEnded);
				task.OutputDataReceived += (a,b)=>this.GetDownloadLogs(output.ToString());
				this.SetCancel(task);
			};
			cleanup.Exited += (a,b)=>this.Dispatcher.BeginInvoke(action);
		}
		public void GetDownloadLogs(string logs){
			this.Dispatcher.BeginInvoke(()=>{
				this.LogBox.Text = logs;
				this.LogSlider.ScrollToEnd();
			});
		}
		public void DownloadEnded(object? sender,EventArgs parameters){
			var action = ()=>{
				var color = Color.FromArgb(255,255,186,33);
				this.downloading = false;
				this.Start.Background = new SolidColorBrush(color);
				this.PathChanged();
				this.VersionChanged();
			};
			this.Dispatcher.BeginInvoke(action);
		}
		public void StartBuild(){
			var path = Path.GetFullPath(this.BuildPath.Text);
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = path+"/"+Options.current["Executable"];
			startInfo.UseShellExecute = true;
			Process.Start(startInfo);
		}
		//=======================
		// Version List
		//=======================
		public void SetupVersion(){
			var action = ()=>{
				var template = this.Version.Template;
				var popup = template.FindName("PART_Popup",this.Version) as Popup;
				popup!.Placement = PlacementMode.Top;
				foreach(var item in Logs.formatted.Keys.OrderByDescending(x=>x)){
					if(item == ""){continue;}
					this.Version.Items.Add(item);
				}
				var previousSelected = Options.current["Version"];
				this.Version.SelectedValue = previousSelected == "" ? this.Version.Items[0] : previousSelected;
				this.Version.MaxDropDownHeight /= 2;
			};
			this.Dispatcher.Invoke(action);
		}
		public void VersionChanged(object sender=null,SelectionChangedEventArgs parameters=null){
			var buildExists = File.Exists(this.BuildPath.Text+"/"+Options.current["Executable"]);
			var (task,output) = this.versioning.ShowLocalVersion();
			var action = ()=>{
				var (selected,installed) = (this.Version.SelectedValue.ToString(),output.ToString().Trim());
				Options.current["Version"] = installed;
				this.LogBox.Text = "";
				this.LogBox.Inlines.AddRange(Logs.formatted[selected]);
				if(!buildExists || this.downloading){return;}
				this.Start.Content = selected != installed ? "Update" : "Start";
				this.startClick = selected != installed ? this.UpdateBuild : this.StartBuild;
			};
			task.Exited += (a,b)=>this.Dispatcher.BeginInvoke(action);
		}
    }
}