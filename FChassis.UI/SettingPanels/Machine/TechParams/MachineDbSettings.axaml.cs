using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class MachineDbSettings : Panel {
   public MachineDbSettings () {
      AvaloniaXamlLoader.Load (this);

      ControlInfo[]
      ctrlInfos = new[] {
         new ControlInfo{type=ControlInfo.Type.Group, label="General" },
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Paramater1", binding = "Property1Text"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Database type" },
         new ControlInfo { type = ControlInfo.Type.Combo, label = "Database", items = ["item1", "item2"] },
         };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);
   }
}