using FChassis.Data.Model.Settings.Machine.General;
using System.IO;
using System.Text.Json;

namespace FChassis.Data.JsonDB {
   public class LocalJson : LocalBase {
      public DataContainer? data;
      public LocalJson (DataContainer container)
         => this.data = container;
      public void Save () {
         
         string jsondata = JsonSerializer.Serialize (this.data,new JsonSerializerOptions { WriteIndented = true});

         File.WriteAllText(this.repoPath, jsondata);
      }
      public void Load () {

         if (!File.Exists (this.repoPath))
            return;

         string? json = File.ReadAllText(this.repoPath);

         this.data = JsonSerializer.Deserialize<DataContainer> (json)!;

      }
       
      public void GetData(object dataContext) 
      {
         switch (dataContext) {
            case HMI:
               this.data!.HMI = dataContext as HMI; break;
            case Machine:
               this.data!.Machine = dataContext as Machine; break;
            default:
               break;
         }
      }
      public object LoadData (object dataContext) 
      {
         object? context = dataContext switch 
         {
            HMI     => this.data?.HMI ,
            Machine => this.data?.Machine,
            _       => null
         };

         return context!;
      }
   }
}
