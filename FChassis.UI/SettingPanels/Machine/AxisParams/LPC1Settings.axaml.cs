using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class LPC1Settings : Panel {
   public LPC1Settings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Configuration parameters"),
            new CheckControlInfo ("Advanced", null!),
            new ComboControlInfo ("Axis Type", null!),
            new ComboControlInfo ("Axis Connection", null!),
            new _TextControlInfo ("Axis address", null!),
            new _TextControlInfo ("Max Laser Power", null!, "watts"),
            new _TextControlInfo ("Average Power", null!, "watts"),
      ]);
   }
}