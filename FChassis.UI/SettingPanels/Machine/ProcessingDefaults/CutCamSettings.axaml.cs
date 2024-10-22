using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class CutCamSettings : Panel {
   public CutCamSettings () {
      AvaloniaXamlLoader.Load (this);
      
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null) {
         ControlInfo[] controlInfos = new ControlInfo[4];
         controlInfos[0] = new GroupControlInfo {label = "Finishing rules"};
         controlInfos[1] = createFinishingRuleDGrid ();
         controlInfos[2] = new GroupControlInfo {label = "Microjoint Settings" };
         controlInfos[3] = new _TextControlInfo {label = "Microjoint length" };

         this.AddParameterControls (grid, controlInfos);
      }

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