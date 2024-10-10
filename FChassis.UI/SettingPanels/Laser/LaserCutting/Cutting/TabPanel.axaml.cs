using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Laser.LaserCutting.Cutting;
public partial class TabPanel : Settings.TabPanel {
   public TabPanel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new LargeSettings(),
         new MediumSettings(),
         new SmallSettings(),
         new SpecialSettings(),
         new PreholeSettings(),
      ]);
   }

   override protected TabControl GetTabControl () {
      TabControl tabControl = (TabControl)this.LogicalChildren[0].LogicalChildren[1];
      return tabControl;
   }
}