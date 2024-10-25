using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class NormalSettings : Panel {
   public NormalSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }
   private void AddControls () {
      ControlInfo[] controlInfos = new ControlInfo[] {
       CreateDGridNormal(),
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, controlInfos);

      #region Local function
      DGridControlInfo CreateDGridNormal() {
         var dGridControlInfo = new DGridControlInfo ();
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Normal",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Name"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Normal" },
            }
         };
         return dGridCrtlInfo;
      }
      #endregion

   }
}