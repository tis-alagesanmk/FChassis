using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Panels;

namespace FChassis.UI.Settings.Machine;
public partial class Panel : FChassis.UI.Settings.TabPanel {
   public Panel () {
      AvaloniaXamlLoader.Load (this);
   FChassis.UI.Settings.Machine.ProcessingDefaults.Panel processingDefaults = new ();   
      this.PopulateTabItemContent ([this.general, this.axisParams, this.techParams, 
                                    this.plcOptions, this.processingDefaults]);
   }

   override protected void TabItemSelected (TabItem? tabItem, string? tabName) {
      if (tabName == "Close") {
         //Child.mainWindow?.Switch2MainPanel ();
      }
   }

   #region "Fields"
   FChassis.UI.Settings.Machine.General.Panel general = new ();
   FChassis.UI.Settings.Machine.AxisParams.Panel axisParams = new ();
   FChassis.UI.Settings.Machine.TechParams.Panel techParams = new ();
   FChassis.UI.Settings.Machine.PLCOptions.Panel plcOptions = new ();
   FChassis.UI.Settings.Machine.ProcessingDefaults.Panel processingDefaults = new ();   
   #endregion "Fields"
}