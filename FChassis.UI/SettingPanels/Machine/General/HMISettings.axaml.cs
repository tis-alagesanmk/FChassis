using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null) 
         this.AddParameterControls (grid, new[] {
         new ControlInfo{type=ControlInfo.Type.Group, label="General"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Paramater1"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Paramater2", unit="s"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Paramater3", items= ["item1", "item2"]},
         new ControlInfo{type=ControlInfo.Type.Check, label="Paramater4"},
      });

      if (grid != null)
         this.AddParameterControls (grid, new[] {
         new ControlInfo{type=ControlInfo.Type.Group, label="General"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Paramater1"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Paramater2", unit="s"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Paramater3", items= ["item1", "item2"]},
         new ControlInfo{type=ControlInfo.Type.Check, label="Paramater4"},
      });

      grid = this.LogicalChildren[0].LogicalChildren[0].LogicalChildren[1] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new[] {
         new ControlInfo{type=ControlInfo.Type.Group, label="General"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Paramater1"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Paramater2", unit="s"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Paramater3", items= ["item1", "item2"]},
         new ControlInfo{type=ControlInfo.Type.Check, label="Paramater4"},
      });
   }
}