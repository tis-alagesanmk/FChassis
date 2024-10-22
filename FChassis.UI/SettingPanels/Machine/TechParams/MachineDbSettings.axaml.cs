using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class MachineDbSettings : Panel {
   public MachineDbSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo{label="General" },
            new _TextControlInfo{label="Paramater1", binding="Property1Text"},

            new GroupControlInfo{label="Database type" },
            new ComboControlInfo {label="Database", items=["item1", "item2"] },
      });
   }
}