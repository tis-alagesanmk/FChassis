using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace FChassis.Data.ViewModel.Settings.Machine.General; 
public partial class MachineViewModel: FChassis.Data.Model.Settings.Machine.General.Machine {
   [RelayCommand] public void IncrementLimit() {
      this.OverrideLimit += 1;
   }

   string[] codes = ["12", "13", "14", "15", "16"];
}
