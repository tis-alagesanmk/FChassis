using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo{label="General"},
            new ComboControlInfo{label="Orientation"},
            new _TextControlInfo{label="Step size to increment" },
            new _TextControlInfo{label="Maximum days keep back up files"},
            new _TextControlInfo{label="Minimum storage to keep back up files", unit="GB"},
            new ComboControlInfo{label="PLC messages to display"},
            new CheckControlInfo{label="Caption for command-bar icons"},
            new CheckControlInfo{label="Mini player"},
            new ComboControlInfo{label="Language"},
            new ComboControlInfo{label="Theme"},

            new GroupControlInfo{label="Screen size"},
            new _TextControlInfo{label="Width"},
            new _TextControlInfo{label="Height"},
      });
   }
}