using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Setting.Machine.AxisParams;
using FChassis.Data.ViewModels.Setting.Machine.AxisParams;
using System.Windows;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class AxisSettingsPanel : Panel{
 public AxisSettingsPanel() {

      AvaloniaXamlLoader.Load(this);

      this.AddPropControls (typeof (Axis));
   }
}