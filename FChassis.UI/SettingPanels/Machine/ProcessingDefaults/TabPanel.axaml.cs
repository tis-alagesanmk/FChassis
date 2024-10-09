using Avalonia.Markup.Xaml;
using FChassis.UI.Settings.Machine.AxisParams;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class TabPanel : Settings.TabPanel {
   public TabPanel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new ImportSettings(),
         new CutCamSettings(),
         new ProfileCamSettings(),
      ]);
   }
}