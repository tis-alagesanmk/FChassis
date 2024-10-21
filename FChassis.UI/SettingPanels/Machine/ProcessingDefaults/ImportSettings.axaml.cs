using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;

public partial class ImportSettings : Panel {

   public ImportSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }


   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type=ControlInfo.Type.Group, label="Import settings"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Units for DFX files"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Stitch together lines/arcs closer than this"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Maximun thickness for sheet-metal part"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Ignore layer in DXF/DWG files"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Explode blocks in 2D drawing"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Convert white entities to black"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Darken colors during DXF import"},

         new ControlInfo{type=ControlInfo.Type.Group, label="DXF Settings"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Angles in DXF are interior angles"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Spline Coversion"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Covert splines on import"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Layer mapping"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Auxilary Layers Names"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Mark Layers Names"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Mark Layers Names"},
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }

}