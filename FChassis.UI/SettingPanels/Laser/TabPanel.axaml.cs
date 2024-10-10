using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Laser;
public partial class TabPanel : Settings.TabPanel {
   public TabPanel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new LaserCutting.TabPanel(),
         new DCAPage.Panel(),
         new RampCycles.Panel(),
      ]);
   }

   override protected void TabItemSelected (TabItem? tabItem, string? tabName) {
      this.TabItemSelected_Default (tabItem, tabName);
   }
}