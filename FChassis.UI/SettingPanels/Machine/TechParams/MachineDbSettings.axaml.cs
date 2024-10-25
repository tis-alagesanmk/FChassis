using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class MachineDbSettings : Panel {
   public MachineDbSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("General"),
            new _TextControlInfo ("Paramater1", "Property1Text"),

            new GroupControlInfo ("Database type"),
            new ComboControlInfo ("Database", null!, null!),
      ]);
   }
}