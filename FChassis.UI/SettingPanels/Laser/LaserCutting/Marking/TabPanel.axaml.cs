using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace FChassis.UI.Settings.Laser.LaserCutting.Marking;
public partial class TabPanel : Settings.TabPanel {
   public TabPanel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new LargeSettings(),
         new MediumSettings(),
         new SmallSettings(),
         new SpecialSettings(),
      ]);
   }

   override protected TabControl GetTabControl () {
      TabControl tabControl = (TabControl)this.LogicalChildren[0].LogicalChildren[1];
      return tabControl;
   }
}