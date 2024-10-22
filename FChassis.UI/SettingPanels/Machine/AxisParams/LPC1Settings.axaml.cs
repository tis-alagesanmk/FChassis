using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class LPC1Settings : Panel {
   public LPC1Settings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo{label="Configuration parameters"},
            new CheckControlInfo{label="Advanced"},
            new ComboControlInfo{label="Axis Type"},
            new ComboControlInfo{label="Axis Connection"},
            new _TextControlInfo{label="Axis address"},
            new _TextControlInfo{label="Max Laser Power", unit="watts"},
            new _TextControlInfo{label="Average Power", unit="watts"}
      });
   }
}