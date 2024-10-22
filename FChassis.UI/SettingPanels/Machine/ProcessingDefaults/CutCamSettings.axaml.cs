using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using FChassis.UI.SettingPanels.Machine.Model;


namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class CutCamSettings : Panel {
   public CutCamSettings () {
      AvaloniaXamlLoader.Load (this); 

      ControlInfo[] ctrlInfos = new ControlInfo[] {
         new CheckControlInfo{label="Advanced"},
         createFinishingRuleDGrid(),
         new GroupControlInfo{ label="Microjoint settings"},
         new _TextControlInfo{label="Microjoint length"}
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

      #region Local Function
      DGridControlInfo createFinishingRuleDGrid () {
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Finishing Rules",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Wire Auto"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Contour Flags" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Check, header = "Disabled"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Size" },
         }};

         return dGridCrtlInfo;
      }
      #endregion Local Function
   }
}