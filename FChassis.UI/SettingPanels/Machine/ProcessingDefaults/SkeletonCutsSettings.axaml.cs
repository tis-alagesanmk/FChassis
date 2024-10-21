using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class SkeletonCutsSettings : Panel{
   public SkeletonCutsSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }


   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type=ControlInfo.Type.Group, label="Sheet cutting rules"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Create sheet cut"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="X spacing between vertical sheet cuts"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Y spacing between horizontal sheet cuts"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Create remainder sheet"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Minimum remainder sheet width"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Final cut X offset"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Process sheet cut after all part"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Sheet cut parameters"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Micro joint gap at sheet edge"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Micro joint gap at part edge"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Pierce distance from part edge"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Measuring distance from sheet edge"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Overtravel after sheet edge"},

      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }
}