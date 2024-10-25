using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FChassis.Data.ViewModel.Settings.Machine.General;
public partial class MachineViewModel : FChassis.Data.Model.Settings.Machine.General.Machine {
   [RelayCommand]
   public void IncrementLimit () {
      this.OverrideLimit += 1;
   }

   [ObservableProperty] private string[] codes = ["12", "13", "14", "15", "16"];
}
