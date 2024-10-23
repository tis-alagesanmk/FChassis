using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class MultipleSettings : Panel {

   public MultipleSettings () { 

      AvaloniaXamlLoader.Load (this);
      this.AddControls ();  
   }

   private void AddControls () {
      ControlInfo[] controlInfos = new ControlInfo[] {
         CreateMultipleSettingDGrid1 (),
         CreateMultipleSettingDGrid2 (),
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, controlInfos);

      #region Local function
      DGridControlInfo CreateMultipleSettingDGrid1 () {
         var dGridControlInfo = new DGridControlInfo ();
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Multiple1",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Name"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step1" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step2" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step3" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step4" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step5" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step6" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step7" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step8" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step9" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Step10" },
            }
         };
         return dGridCrtlInfo;
      }
      DGridControlInfo CreateMultipleSettingDGrid2 () {
         var dGridControlInfo = new DGridControlInfo ();
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Multiple2",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Name"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Common" },
            }
         };
         return dGridCrtlInfo;
      }
      #endregion

   }
}