using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class PeckSettings : Panel {

   public PeckSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }

   private void AddControls() {
      ControlInfo[] controlInfos = new ControlInfo[] {
         CreatePeckSettingDGrid1 (),
         CreatePeckSettingDGrid2 (),
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, controlInfos);

      #region Local function
      DGridControlInfo CreatePeckSettingDGrid1() {
         var dGridControlInfo = new DGridControlInfo ();
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Peck1",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Name"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Start" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Check, header = "Increment"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "RampStart" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "RampEnd" },
            }
         };
         return dGridCrtlInfo;
      }
      DGridControlInfo CreatePeckSettingDGrid2 () {
         var dGridControlInfo = new DGridControlInfo ();
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Peck2",
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
