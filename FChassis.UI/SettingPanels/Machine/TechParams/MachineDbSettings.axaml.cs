using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.TechParams;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class MachineDbSettings : Panel {
   public MachineDbSettings () {
      AvaloniaXamlLoader.Load (this);

      this.AddPropControls (typeof (MachineDataBase));
   }
}