using System.IO;
using System.Linq;
namespace Launcher.Extensions{
	public static class DirectoryInfoExtensions{
		public static void SetAttributesRecursive(this DirectoryInfo directory,FileAttributes attribute){
			directory.GetFiles().ToList().ForEach(x=>x.Attributes = attribute);
			foreach(var item in directory.GetDirectories()){
				item.Attributes = attribute;
				item.SetAttributesRecursive(attribute);
			}
		}
	}
}
