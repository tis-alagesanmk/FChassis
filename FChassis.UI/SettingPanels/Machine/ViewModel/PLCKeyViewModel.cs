using FChassis.UI.SettingPanels.Machine.Model;
using System.Collections.ObjectModel;

namespace FChassis.UI.SettingPanels.Machine.ViewModel;
public class PLCKeyViewModel {
   public ObservableCollection<PLCKeyModel> PLCKey1 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey2 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey3 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey4 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey5 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey6 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey7 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey8 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey9 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey10 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey11 { get; set; }
   public ObservableCollection<PLCKeyModel> PLCKey12 { get; set; }

   public PLCKeyViewModel () {


      var plcKey = new ObservableCollection<PLCKeyModel> () {
         new PLCKeyModel("F1"),
         new PLCKeyModel("F2"),
         new PLCKeyModel("F3"),
         new PLCKeyModel("F4"),
         new PLCKeyModel("F5"),
         new PLCKeyModel("F6"),
         new PLCKeyModel("F7"),
         new PLCKeyModel("S3"),
         new PLCKeyModel("S4"),
         new PLCKeyModel("S5"),
         new PLCKeyModel("S6"),
      };

      PLCKey1 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey2 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey3 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey4 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey5 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey6 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey7 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey8 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey9 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey10 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey11 = new ObservableCollection<PLCKeyModel> (plcKey);
      PLCKey12 = new ObservableCollection<PLCKeyModel> (plcKey);
   }
}
