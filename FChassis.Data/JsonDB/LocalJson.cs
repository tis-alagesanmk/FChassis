using FChassis.Data.Model.Settings.Machine.General;
using FChassis.Data.ViewModel.Settings.Machine.General;
using System.IO;
using System.Text.Json;

namespace FChassis.Data.JsonDB {
   public class LocalJson : LocalBase {
      public DataContainer? data = null!;
      
      public LocalJson (DataContainer container)
         => this.data = container;

      private bool FileExits (string path) => System.IO.File.Exists (path);
      private string ReadFromFile (string path) => System.IO.File.ReadAllText (path);
      private void WriteToFile (string path,string content) => System.IO.File.WriteAllText (path,content);

      public void Save () {
         string jsondata = JsonSerializer.Serialize (this.data, new JsonSerializerOptions { WriteIndented = true});
         this.WriteToFile(this.repoPath, jsondata);
      }

      public void Load () {
         if (this.FileExits (this.repoPath))
            return;

         string? json = this.ReadFromFile(this.repoPath);
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
