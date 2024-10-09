using FChassis.UI.SettingPanels.Machine.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.UI.SettingPanels.Machine.ViewModel {
   public class CutCamViewModel {
      public ObservableCollection<CutcamSetting> CutcamSettings { get;}
      public CutCamViewModel () { 
      }
   }
}
