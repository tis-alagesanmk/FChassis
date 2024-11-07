using System.IO; 
using System.Reflection;

namespace FChassis.Data.JsonDB {
   public class LocalBase {
      public string repoPath { get; set; }
      public LocalBase() 
      {
         repoPath = Path.Combine( this.CreateFolderPath ("JSON"),"settings.json");
      }
      private string CreateFolderPath(string name) 
      {
         string? exePath = Assembly.GetExecutingAssembly ().Location;
         string? exeDirectory = Path.GetDirectoryName (exePath);
         string? folderPath = Path.Combine (exeDirectory, name);
       
         if (!Directory.Exists (folderPath)) 
         {
            Directory.CreateDirectory (folderPath);
         } 
       
         return folderPath;
      }
   }
}
