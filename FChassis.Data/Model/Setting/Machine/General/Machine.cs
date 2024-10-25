using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace FChassis.Data.Model.Settings.Machine.General {
   public partial class Machine : ObservableObject{
      [ObservableProperty] int overrideLimit = 1;
      [ObservableProperty] string machineId = "130166";
      [ObservableProperty] string axisEmulation = "1";
      [ObservableProperty] string cfgCustomTech = "ECUT";
      [ObservableProperty] string code = "14";
   }
}
