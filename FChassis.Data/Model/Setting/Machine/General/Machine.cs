using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace FChassis.Data.Model.Settings.Machine.General {
   public partial class Machine : ObservableObject{
      [ObservableProperty] private int overrideLimit = 1;
      [ObservableProperty] private string machineId = "130166";
      [ObservableProperty] private string axisEmulation = "1";
      [ObservableProperty] private string cfgCustomTech = "ECUT";
      [ObservableProperty] private string code = "14";
      [ObservableProperty] private List<string> codes = ["12", "13", "14", "15", "16"];
   }
}
