using System.IO;
using System.Text.Json;

namespace FChassis.Data.JsonDB {
   public class LocalSettings : LocalBase {
      public DataContainer data;

      public LocalSettings (DataContainer container) 
         => this.data = container;
      
      public void Save () {
         string jsondata = JsonSerializer.Serialize (this.data,new JsonSerializerOptions { WriteIndented = true});
         File.WriteAllText(this.repoPath, jsondata);
      }

      public DataContainer Load () {
         if (!File.Exists (this.repoPath)) 
            return null!;

         var json = File.ReadAllText(this.repoPath);

         this.data = JsonSerializer.Deserialize<DataContainer> (json)!;
         return data;
      }
   }
}
