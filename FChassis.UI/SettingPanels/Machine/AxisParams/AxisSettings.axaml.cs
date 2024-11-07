using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Setting.Machine.AxisParams;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class AxisSettingsPanel : Panel{
 public AxisSettingsPanel() {
      AvaloniaXamlLoader.Load(this);

      this.AddPropControls (typeof (Axis));
   }
}