using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.UI.SettingPanels.Machine.Model; 
public class ExhaustSysModel : ObservableObject {
   public string? SectionNumber { get; set; }
   public string? XOn { get; set; }
   public string? XOff { get; set; }
   public string? YOn { get; set; }
   public string? YOff { get; set; }
}
