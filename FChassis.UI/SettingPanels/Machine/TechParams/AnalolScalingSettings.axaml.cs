using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using FChassis.Data.ViewModels.Setting.Machine.TechParams;
using System.Windows;
using FChassis.Data.Model.Settings.Machine.TechParams;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class AnalogScalingSettings : Panel {
   public AnalogScalingSettings () {
      AvaloniaXamlLoader.Load (this);

      AnalogScalingViewModel vm = new AnalogScalingViewModel ();
      this.DataContext = vm;

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddPropControls (grid, typeof(AnalogScaling));
   }
}