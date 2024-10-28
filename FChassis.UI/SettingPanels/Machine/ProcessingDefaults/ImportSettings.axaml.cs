using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class ImportSettings : Panel {

   public ImportSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Import settings"),
            new ComboControlInfo ("Units for DFX files", null!, null!),
            new _TextControlInfo ("Stitch together lines/arcs closer than this", null!),
            new _TextControlInfo ("Maximun thickness for sheet-metal part", null!),
            new CheckControlInfo ("Ignore layer in DXF/DWG files", null!),
            new CheckControlInfo ("Explode blocks in 2D drawing", null!),
            new CheckControlInfo ("Convert white entities to black", null!),
            new CheckControlInfo ("Darken colors during DXF import", null!),

            new GroupControlInfo ("DXF Settings"),
            new CheckControlInfo ("Angles in DXF are interior angles", null!),

            new GroupControlInfo ("Spline Coversion"),
            new ComboControlInfo ("Covert splines on import", null!, null!),

            new GroupControlInfo ("Layer mapping", null!),
            new _TextControlInfo ("Auxilary Layers Names", null!),
            new _TextControlInfo ("Mark Layers Names", null!),
            new _TextControlInfo ("Mark Layers Names", null!),
      ]);
   }
}