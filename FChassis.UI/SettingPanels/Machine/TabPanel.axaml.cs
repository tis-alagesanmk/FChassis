using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine;
public partial class TabPanel : Settings.TabPanel {
   public TabPanel () : base() {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new General.TabPanel(),
         new AxisParams.TabPanel(),
         new TechParams.TabPanel(),
         new PLCOptions.TabPanel(),
         new ProcessingDefaults.TabPanel(),
      ]);

      FChassis.Data.IO.JSONFileRead reader = new ();
      this.AddConfiguraionNodes (reader.node);
      reader.Read ("C:/work/config.json");
   }

   override protected void TabItemSelected (TabItem? tabItem, string? tabName) {
      this.TabItemSelected_Default (tabItem, tabName); }
}