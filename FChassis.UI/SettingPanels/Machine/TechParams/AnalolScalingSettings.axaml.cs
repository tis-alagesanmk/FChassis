using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class AnalogScalingSettings : Panel {
   public AnalogScalingSettings () {
      AvaloniaXamlLoader.Load (this);

      ControlInfo[] ctrlInfos = new ControlInfo[29];
      ctrlInfos[0] = new GroupControlInfo {label = "Chennals"};

      for (int i = 1; i <= 28; i++) 
         ctrlInfos[i] = new _TextControlInfo {label = $"Chennal {i}"};

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);
   }
}