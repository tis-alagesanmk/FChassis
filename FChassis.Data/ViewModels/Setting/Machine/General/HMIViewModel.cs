using FChassis.Data.Model.Setting.Machine.General;

namespace FChassis.Data.ViewModels.Setting.Machine.General;
public class HMIViewModel : HMI {
   public HMIViewModel () {         
      // ComboBoxItems ()
      this.Orientation = ["Portrait", "Landscape" ];
      this.PLCMessagesToDisplay = ["Only error", "Warn & error", "info, warn & error"];
      this.Language = ["EN", "CN", "KO", "BR", "ES"];
      this.Theme = ["Grey", "Blue"];
   }
}
