using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine;
public partial class TabPanel : FChassis.UI.Settings.TabPanel {
   public TabPanel () : base() {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new FChassis.UI.Settings.Machine.General.Panel(),
         new FChassis.UI.Settings.Machine.AxisParams.Panel(),
         new FChassis.UI.Settings.Machine.TechParams.Panel(),
         new FChassis.UI.Settings.Machine.PLCOptions.Panel(),
         new FChassis.UI.Settings.Machine.ProcessingDefaults.Panel(),
      ]);
   }

   override protected void TabItemSelected (TabItem? tabItem, string? tabName) {
      this.TabItemSelected_Default (tabItem, tabName); }
}