using FChassis.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.UI.SettingPanels.Machine.TechParams.Model {
   public class ExhaustSysModel : ViewModelBase {
      public string SectionNumber { get; set; }
      public string XOn { get; set; }
      public string XOff { get; set; }
      public string YOn { get; set; }
      public string YOff { get; set; }
   }
}
