using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace FChassis.Data.ViewModel.Settings.Machine.General; 
public partial class MachineSettings:ObservableObject {

   [ObservableProperty]
   private Model.Settings.Machine.General.MachineSettings machine;

   public MachineSettings () {
      Machine = new Model.Settings.Machine.General.MachineSettings ();
   }

   [RelayCommand]
   public void IncrementLimit() {
      Machine.OverrideLimit += 1;
   }
}
