using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      ControlInfo[] ctrlInfos = new[] {
         new ControlInfo{type=ControlInfo.Type.Group, label="General" },
         new ControlInfo{type=ControlInfo.Type.Text,  label="Paramater1", binding = "Property1Text"},
         new ControlInfo{type=ControlInfo.Type.Text,  label="Paramater2", unit="s"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Paramater3", items= ["item1", "item2"]},
         new ControlInfo{type=ControlInfo.Type.Check, label="Paramater4"},
         new ControlInfo{type=ControlInfo.Type.Button, binding = "ButtonClickedCommand"},
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);
   }
}