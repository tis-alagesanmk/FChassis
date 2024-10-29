using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FChassis.Data.ViewModel.Settings.Machine.General;
public partial class MachineViewModel : Model.Settings.Machine.General.Machine {
   [RelayCommand]
   public void IncrementLimit () {
      this.OverrideLimit += 1;
   }
}