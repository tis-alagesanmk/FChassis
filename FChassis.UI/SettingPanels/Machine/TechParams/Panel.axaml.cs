using Avalonia.Markup.Xaml;
using FChassis.UI.Settings.Machine.AxisParams;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class Panel : FChassis.UI.Settings.TabPanel {
   public Panel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([this.mFunctions, this.mFunctions, this.laserTechnology, this.laserTechnology, this.machineDatabase]);
   }

   #region "Fields"
   MFunctions mFunctions = new ();
   LaserTechnology laserTechnology = new ();
   MachineDatabase machineDatabase = new ();
   #endregion "Fields"
}