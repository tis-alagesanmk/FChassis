using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class Panel : FChassis.UI.Settings.TabPanel {
   public Panel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([
         new HMISettings(),
         new MachineSettings(),
      ]);
   }
}