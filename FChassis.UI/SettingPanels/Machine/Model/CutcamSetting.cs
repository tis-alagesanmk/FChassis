using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.UI.SettingPanels.Machine.Model; 
public class CutcamSetting : ObservableObject {
   public string Action { get; set; }
   public string Contourflags { get; set; }
   public string Disabled { get; set; }
   public string Size { get; set; }
}
