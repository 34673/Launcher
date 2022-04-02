using System.Windows;
namespace Launcher{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application{
		public void Save(object sender,ExitEventArgs parameters){
			var window = Launcher.MainWindow.self;
			Options.current["BuildPath"] = window.Path.Text;
			Options.current["Version"] = window.Version.Text;
			Options.current.Save(window.basePath+"Options.config");
		}
    }
}
