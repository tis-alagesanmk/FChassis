using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class WorkSupportSettings : Panel{
   public WorkSupportSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo{label="Work support configuration"},
            new ComboControlInfo{label="Distance between slats"},
            new ComboControlInfo{label="Offset of first slat from sheet edge"},
            new ComboControlInfo{label="Distance between support pins in a slat"},
      });
   }
}