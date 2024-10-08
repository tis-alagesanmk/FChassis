using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine;
public partial class TabPanel : Settings.TabPanel {
   public TabPanel () : base() {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new FChassis.UI.Settings.Machine.General.TabPanel(),
         new FChassis.UI.Settings.Machine.AxisParams.TabPanel(),
         new FChassis.UI.Settings.Machine.TechParams.TabPanel(),
         new FChassis.UI.Settings.Machine.PLCOptions.TabPanel(),
         new FChassis.UI.Settings.Machine.ProcessingDefaults.TabPanel(),
      ]);
   }

   override protected void TabItemSelected (TabItem? tabItem, string? tabName) {
      this.TabItemSelected_Default (tabItem, tabName); }
}