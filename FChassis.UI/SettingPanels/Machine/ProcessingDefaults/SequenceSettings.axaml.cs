using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class SequenceSettings : Panel{

   public SequenceSettings () {
      AvaloniaXamlLoader.Load (this);
      
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo{label="Laser Sequence"},
            new ComboControlInfo{label="Laser processing sequence"},
            new ComboControlInfo{label="Laser Seq"},
            new CheckControlInfo{label="Do pre-piercing part-by-part"},

            new GroupControlInfo{label="Route Traverse"},
            new CheckControlInfo{label="Move pierce points to reduce traverse"},
            new CheckControlInfo{label="Move pierce points to prevent tilting"},
            new CheckControlInfo{label="Microjoint nested holes if tilting"},
            new _TextControlInfo{label="Ignore holes smaller tha this"},
            new _TextControlInfo{label="Minimum cutting head height when traversing"},
            new CheckControlInfo{label="Route traverse lines around holes"},
            new _TextControlInfo{label="Allowance when routing around holes"},
            new _TextControlInfo{label="lift nozzle if routing penalty more than"},
            new _TextControlInfo{label="Allowance when routing around tilting holes"},
            new _TextControlInfo{label="Max.head down traverse distance"},

            new GroupControlInfo{label="Laser Heads"},
            new CheckControlInfo{label="Cute with single head"},
      ]);
   }
}