using FChassis.Data.Model.Settings.Machine.General;
using FChassis.Data.ViewModel.Settings.Machine.General;

namespace FChassis.Data.JsonDB {
   public class DataContainer {
      public HMIViewModel? HMI { get; set; }
      public MachineViewModel? Machine { get; set; }
   }
}
