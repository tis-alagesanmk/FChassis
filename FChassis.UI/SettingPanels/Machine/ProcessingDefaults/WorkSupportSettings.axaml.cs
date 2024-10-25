using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class WorkSupportSettings : Panel{
   public WorkSupportSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Work support configuration"),
            new ComboControlInfo ("Distance between slats", null!, null!),
            new ComboControlInfo ("Offset of first slat from sheet edge", null!, null!),
            new ComboControlInfo ("Distance between support pins in a slat", null!, null!),
      ]);
   }
}