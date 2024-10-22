using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {

   public HMISettings () 
   {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }


   private void AddControls() 
   {
      ControlInfo[] ctrlInfos = new ControlInfo[] 
      {
         new ControlInfo{type=ControlInfo.Type.Group, label="General"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Orientation"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Step size to increment" },
         new ControlInfo{type=ControlInfo.Type.Text_, label="Maximum days keep back up files"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Minimum storage to keep back up files", unit="GB"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="PLC messages to display"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Caption for command-bar icons"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Mini player"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Language"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Theme"},
         new ControlInfo{type=ControlInfo.Type.Group, label="Screen size"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Width"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Height"},
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }

}