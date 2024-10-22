using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;

public partial class WorkSupportSettings : Panel{
   public WorkSupportSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }


   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type=ControlInfo.Type.Group, label="Work support configuration"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Distance between slats"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Offset of first slat from sheet edge"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Distance between support pins in a slat"},
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }
}