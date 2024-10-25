using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.Data.Model.Settings.Machine.General {
   public partial class MachineSettings :ObservableObject{
      [ObservableProperty]
      private int overrideLimit;

      public string MachineId { get; set; }
      public string AxisEmulation { get; set; }
      public string CfgCustomTech { get; set; }
      public string Code { get; set; }
      public List<string> Codes { get; set; }

      public MachineSettings () {
         MachineId = "130166";
         AxisEmulation = "1";
         CfgCustomTech = "ECUT";
         OverrideLimit = 1;
         Codes = new List<string> { "12", "13", "14", "15", "16" };
         Code = "14";
      }
   }
}
