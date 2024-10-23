using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class RampSettings : Panel {
   public RampSettings () {

      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }
   private void AddControls () {
      ControlInfo[] controlInfos = new ControlInfo[] {
       CreateDGridRamp(),
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, controlInfos);

      #region Local function
      DGridControlInfo CreateDGridRamp () {
         var dGridControlInfo = new DGridControlInfo ();
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Ramp",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Name"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Power" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Duty" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Nozzle_Gap" },
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "RampValue" },
               
            }
         };
         return dGridCrtlInfo;
      }
      #endregion

   }
}