using FChassis.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.UI.SettingPanels.Machine.Model {
   public class CutcamSetting :ViewModelBase
   {
      public string Action { get; set; }
      public string Contourflags { get; set; }
      public string Disabled { get; set; }
      public string Size { get; set; }
   }
}
