using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class ImportSettings : Panel {

   public ImportSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo{label="Import settings"},
            new ComboControlInfo{label="Units for DFX files"},
            new _TextControlInfo{label="Stitch together lines/arcs closer than this"},
            new _TextControlInfo{label="Maximun thickness for sheet-metal part"},
            new CheckControlInfo{label="Ignore layer in DXF/DWG files"},
            new CheckControlInfo{label="Explode blocks in 2D drawing"},
            new CheckControlInfo{label="Convert white entities to black"},
            new CheckControlInfo{label="Darken colors during DXF import"},

            new GroupControlInfo{label="DXF Settings"},
            new CheckControlInfo{label="Angles in DXF are interior angles"},

            new GroupControlInfo{label="Spline Coversion"},
            new ComboControlInfo{label="Covert splines on import"},

            new GroupControlInfo{label="Layer mapping"},
            new _TextControlInfo{label="Auxilary Layers Names"},
            new _TextControlInfo{label="Mark Layers Names"},
            new _TextControlInfo{label="Mark Layers Names"},
      ]);
   }
}