using CommunityToolkit.Mvvm.Input;

namespace FChassis.Data.ViewModel.Settings.Machine.General; 
public partial class MachineViewModel: FChassis.Data.Model.Settings.Machine.General.Machine {

   [RelayCommand]
   public void IncrementLimit() {
      this.OverrideLimit += 1;
   }
}
