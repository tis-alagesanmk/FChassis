using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class AnalogScalingSettings : Panel {
   public AnalogScalingSettings () {

      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }
   private void AddControls () {

      ControlInfo[] ctrlInfos = new ControlInfo[29];

      for(int i=0; i<=28; i++) 
      {
         ctrlInfos[i] = i == 0 ? new ControlInfo { type = ControlInfo.Type.Group, label = "Chennals"}: 
                        new ControlInfo { type = ControlInfo.Type.Text, label = "Chennal" +" "+ i};
      }

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }
}