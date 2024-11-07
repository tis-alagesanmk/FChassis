using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.General;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof (HMI));
   }
}