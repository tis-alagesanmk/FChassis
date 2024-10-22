using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.SettingPanels.Machine.Model;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;

public partial class CutCamSettings : Panel{
   public CutCamSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }

   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new CheckControlInfo{label="Advanced"},
         this.CreateFinishRulesDataGrid(""),
         new GroupControlInfo{ label="Microjoint settings"},
         new _TextControlInfo{label="Microjoint length"}
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }

   DGridControlInfo CreateFinishRulesDataGrid (object binding) {
      DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
         binding = binding,
         columns = new[]
         {
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Action  Number",path ="Action"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Contour flags",path="Contourflags"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Disabled",path="Disabled" },
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Size" ,path="Size"},
         }
      };

      return dGridCrtlInfo;
   }
}