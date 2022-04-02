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
	using Launcher.Extensions;
	using Launcher.Versioning;
	public partial class MainWindow : Window{
		public static MainWindow self;
		public string basePath = AppDomain.CurrentDomain.BaseDirectory;
		public Process process = Process.GetCurrentProcess();
		public Action startClick = ()=>{};
		public bool downloading;
		public MainWindow(){
			MainWindow.self = this;
			InitializeComponent();
			Options.current.Load(this.basePath+"Options.config");
			var icon = new Uri(Options.current["IconPath"]);
			var background = new Uri(Options.current["BackgroundPath"]);
			this.Title = Options.current["WindowTitle"];
			this.Icon = new BitmapImage(icon);
			this.Grid.Background = new ImageBrush(new BitmapImage(background));
			this.SetupLogs();
			this.SetupPath();
			this.Version.Loaded += this.SetupVersion;
		}
		public void SetupLogs(){
			var repository = Options.current["Repository"];
			var exception = new Exception("Unable to query repository.");
			if(!Logs.Deserialize() && !Logs.Fetch(repository)){/*throw exception;*/}
			var localHead = Logs.formatted.Keys.Where(x=>x!="").OrderBy(x=>int.Parse(x)).Last();
			var remoteHead = Subversion.Call("info --show-item revision "+repository).Trim('\r','\n');
			if(localHead != remoteHead){Logs.Fetch(repository,localHead,remoteHead);}
		}
		//=======================
		// Path
		//=======================
		public void SetupPath(){
			this.Path.Text = Options.current["BuildPath"];
			var pathProperty = DependencyPropertyDescriptor.FromProperty(TextBox.TextProperty,typeof(TextBox));
			pathProperty.AddValueChanged(this.Path,(a,b)=>this.PathChanged());
			this.PathChanged();
		}
		public void PathChanged(){
			var path = this.Path.Text = this.Path.Text.Replace("@Base",this.basePath);
			if(this.downloading){return;}
			var buildExists = File.Exists(path+"\\"+Options.current["Executable"]);
			this.Start.Content = buildExists ? "Start" : "Download";
			if(!buildExists){this.startClick = this.DownloadBuild;}
		}
		public void PathBrowserClick(object sender,RoutedEventArgs parameters){
			var browser = new WinForms.FolderBrowserDialog();
			browser.ShowDialog();
			this.Path.Text = browser.SelectedPath;
		}
		//=======================
		// Start Button
		//=======================
		public void StartClick(object sender,RoutedEventArgs parameters) => this.startClick();
		public void SetCancel(){
			var color = Color.FromArgb(255,181,181,180);
			this.downloading = true;
			this.Start.Content = "Cancel";
			this.Start.Background = new SolidColorBrush(color);
			this.startClick = Subversion.process.Kill;
		}
		public void DownloadBuild(){
			var command = "checkout -r "+this.Version.Text+" "+Options.current["Repository"]+" \""+this.Path.Text+"\"";
			var svnFiles = new DirectoryInfo(Options.current["BuildPath"]+"\\.svn");
			if(svnFiles.Exists){
				svnFiles.SetAttributesRecursive(FileAttributes.Normal);
				svnFiles.Delete(true);
			}
			Subversion.Call(command,this.GetDownloadLogs,this.DownloadEnded);
			this.SetCancel();
		}
		public void GetDownloadLogs(object sender,DataReceivedEventArgs parameters){
			this.Dispatcher.BeginInvoke(()=>{
				this.LogBox.Text = Subversion.output.ToString();
				this.LogSlider.ScrollToEnd();
			});
		}
		public void UpdateBuild(){
			var command = "update -r "+this.Version.Text+" \""+this.Path.Text+"\"";
			Subversion.Call("cleanup "+Options.current["BuildPath"]);
			Subversion.Call(command,this.GetDownloadLogs,this.DownloadEnded);
			this.SetCancel();
		}
		public void DownloadEnded(object sender,EventArgs parameters){
			var color = Color.FromArgb(255,255,186,33);
			var action = ()=>{
				this.Start.Background = new SolidColorBrush(color);
				this.PathChanged();
				this.VersionChanged(null,null);
			};
			this.downloading = false;
			this.Dispatcher.Invoke(action);
		}
		public void StartBuild(){
			var path = this.Path.Text;
			Directory.SetCurrentDirectory(path);
			Process.Start(path+"\\"+Options.current["Executable"]);
			Directory.SetCurrentDirectory(this.basePath);
		}
		//=======================
		// Version List
		//=======================
		public void SetupVersion(object sender,EventArgs parameters){
			var template = this.Version.Template;
			var popup = template.FindName("PART_Popup",this.Version) as Popup;
			popup.Placement = PlacementMode.Top;
			foreach(var item in Logs.formatted.Keys){
				if(item == ""){continue;}
				this.Version.Items.Add(item);
			}
			this.Version.SelectedValue = Options.current["Version"];
			this.Version.MaxDropDownHeight /= 2;
		}
		public void VersionChanged(object sender,SelectionChangedEventArgs parameters){
			var buildExists = File.Exists(this.Path.Text+"\\"+Options.current["Executable"]);
			var command = "info --show-item last-changed-revision \""+this.Path.Text+"\"";
			var (selected,installed) = (this.Version.SelectedValue.ToString(),Subversion.Call(command).Trim());
			var (name,action) = selected != installed ? ("Update",(Action)this.UpdateBuild) : ("Start",this.StartBuild);
			this.LogBox.Text = "";
			Logs.formatted[selected].ForEach(x=>this.LogBox.Inlines.Add(x));
			if(!buildExists || this.downloading){return;}
			this.Start.Content = name;
			this.startClick = action;
		}
    }
}