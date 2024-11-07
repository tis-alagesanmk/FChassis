using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using FChassis.Data.ViewModels.Setting.Machine.TechParams;
using System.Windows;
using FChassis.Data.Model.Settings.Machine.TechParams;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class AnalogScalingSettings : Panel {
   public AnalogScalingSettings () {
      AvaloniaXamlLoader.Load (this);

      this.AddPropControls (typeof(AnalogScaling));
   }
}