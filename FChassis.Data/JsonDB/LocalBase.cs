using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.Data.JsonDB {
   public class LocalBase {
      public string repoPath { get; set; }
      public LocalBase() 
      {
         repoPath = Path.Combine( this.CreateFolderPath ("JSON"),"settings.json");
      }
      private string CreateFolderPath(string name) 
      {
         string exePath = Assembly.GetExecutingAssembly ().Location;
         string exeDirectory = Path.GetDirectoryName (exePath);
         string newFolderPath = Path.Combine (exeDirectory, name);
         try 
         {
            if (!Directory.Exists (newFolderPath)) 
            {
               Directory.CreateDirectory (newFolderPath);
               Console.WriteLine ($"Folder created at: {newFolderPath}");
            } 
            else 
            {
               Console.WriteLine ($"Folder already exists at: {newFolderPath}");
            }
         } 
         catch (Exception ex) 
         {
            Console.WriteLine ($"Error creating folder: {ex.Message}");
         }
         return newFolderPath;
      }
   }
}
