using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace FChassis.Data.ViewModel.SettingPanels.Machine.General; 
public partial class MachineSettings:ObservableObject {

   [ObservableProperty]
   private string machineId;
   
   [ObservableProperty]
   private string axisEmulation;
   
   [ObservableProperty]
   private string cfgCustomTech;

   [ObservableProperty]
   private int overrideLimit;

   public MachineSettings () {
      MachineId = "130166";
      AxisEmulation = "1";
      CfgCustomTech= "ECUT";
      OverrideLimit = 1;
   }


   [RelayCommand]
   public void IncrementLimit() {
      OverrideLimit += 1;
   }
}
