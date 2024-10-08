using Avalonia.Markup.Xaml;
using FChassis.UI.Settings.Machine.AxisParams;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class TabPanel : Settings.TabPanel {
   public TabPanel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new AnalogScalingSettings(),
         new MFuncSettings(),
         new LaserTechSettings(),
         new ExhaustSysSettings(),
         new MachineDbSettings(),
      ]);
   }
}