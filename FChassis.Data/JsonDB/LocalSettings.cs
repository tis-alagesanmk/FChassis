using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FChassis.Data.JsonDB {
   public class LocalSettings : LocalBase {
      public DataContainer data;
      public LocalSettings (DataContainer container) { 

         this.data = container;
      }
      public void Save () {
         
         string jsondata = JsonSerializer.Serialize (this.data,new JsonSerializerOptions { WriteIndented = true});

         File.WriteAllText(this.repoPath, jsondata);
      }
      public DataContainer Load () {

         if (!File.Exists (this.repoPath)) return null;

         var json = File.ReadAllText(this.repoPath);

         data = JsonSerializer.Deserialize<DataContainer> (json);
         return data;
      }
   }
}
