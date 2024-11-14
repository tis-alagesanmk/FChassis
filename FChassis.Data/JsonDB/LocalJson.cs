using FChassis.Data.Model.Settings.Machine.General;
using FChassis.Data.ViewModel.Settings.Machine.General;
using System.IO;
using System.Text.Json;

namespace FChassis.Data.JsonDB {
   public class LocalJson : LocalBase {
      public DataContainer? data = null!;
      
      public LocalJson (DataContainer container)
         => this.data = container;

      public void Save () {
         string jsondata = JsonSerializer.Serialize (this.data, new JsonSerializerOptions { WriteIndented = true});
         System.IO.File.WriteAllText(this.repoPath, jsondata);
      }

      public void Load () {
         if (!System.IO.File.Exists (this.repoPath))
            return;

         string? json = System.IO.File.ReadAllText(this.repoPath);
         this.data = JsonSerializer.Deserialize<DataContainer> (json)!;
      }

      public void GetData(object dataContext) {
         if (this.data == null)
            return;

         object? context = dataContext switch {
             HMIViewModel     => this.data.HMI = (HMIViewModel) dataContext,
             MachineViewModel => this.data.Machine = (MachineViewModel)dataContext,
             _       => null
         };
      }

      public object LoadData (object dataContext) {
         if (this.data == null)
            return null!;

         object? context = dataContext switch {
            HMIViewModel     => this.data.HMI ,
            MachineViewModel => this.data.Machine,
            _       => null
         };

         return context!;
      }
   }
}
