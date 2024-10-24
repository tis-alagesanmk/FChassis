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

   [ObservableProperty]
   private string code;

   [ObservableProperty]
   private List<string> codes;

   public MachineSettings () {
      MachineId = "130166";
      AxisEmulation = "1";
      CfgCustomTech= "ECUT";
      OverrideLimit = 1;
      codes = new List<string> { "12", "13", "14", "15", "16" };
      code = "13";
   }


   [RelayCommand]
   public void IncrementLimit() {
      OverrideLimit += 1;
   }
}
