using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;

public partial class AnalogScaling : FChassis.UI.Settings.Panel {
   public AnalogScaling () {

      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }

   private void AddControls () {

      ControlInfo[] ctrlInfos = new ControlInfo[] { };

      ctrlInfos[0] = new ControlInfo () { type = ControlInfo.Type.Group, label = "Channels" };
      for (int i = 1; i <= 10; i++) {      
            ctrlInfos[i] = new ControlInfo () { type = ControlInfo.Type.Text_, label = "Chennal"+" "+i };
      }

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }
}