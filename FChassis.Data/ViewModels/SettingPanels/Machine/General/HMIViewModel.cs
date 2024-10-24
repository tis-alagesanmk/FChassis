using FChassis.Data.Model.SettingPanels.Machine.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.Data.ViewModels.SettingPanels.Machine.General {
   public class HMIViewModel : HMI {
      public HMIViewModel () {
         this.ComboBoxItems ();
      }
      private void ComboBoxItems () {
         var orientationitems = new List<string> () { "Portrait", "Landscape" };
         this.Orientation = orientationitems;    
         var plcmsgitems = new List<string> () { "Only error", "Warn & error","info,warn & error" };
         this.PLCMessagesToDisplay = plcmsgitems;
         var languageitems = new List<string> () { "EN", "CN","KO","BR","ES" };
         this.Language = languageitems;
         var themeitems = new List<string> () { "Grey", "Blue" };
         this.Theme = themeitems;
      }
   }
}
