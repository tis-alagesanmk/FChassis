using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class GentleSettings : Panel {
   public GentleSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }
   private void AddControls () {
      ControlInfo[] controlInfos = new ControlInfo[] {
       CreateDGridGentle(),
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, controlInfos);

      #region Local function
      DGridControlInfo CreateDGridGentle () {
         var dGridControlInfo = new DGridControlInfo ();
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Gentle",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Name"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Gentle" },
            }
         };
         return dGridCrtlInfo;
      }
      #endregion

   }
}