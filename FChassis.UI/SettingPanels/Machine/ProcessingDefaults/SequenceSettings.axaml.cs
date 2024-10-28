using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Settings;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class SequenceSettings : Panel{

   public SequenceSettings () {
      AvaloniaXamlLoader.Load (this);
      
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo("Laser Sequence", null!),
            new ComboControlInfo ("Laser processing sequence", null!, null!),
            new ComboControlInfo ("Laser Seq", null!, null!),
            new CheckControlInfo ("Do pre-piercing part-by-part", null!),

            new GroupControlInfo ("Route Traverse", null!),
            new CheckControlInfo ("Move pierce points to reduce traverse", null!),
            new CheckControlInfo ("Move pierce points to prevent tilting", null!),
            new CheckControlInfo ("Microjoint nested holes if tilting", null!),
            new _TextControlInfo ("Ignore holes smaller tha this", null!),
            new _TextControlInfo ("Minimum cutting head height when traversing", null!),
            new CheckControlInfo ("Route traverse lines around holes", null!),
            new _TextControlInfo ("Allowance when routing around holes", null!),
            new _TextControlInfo ("lift nozzle if routing penalty more than", null!),
            new _TextControlInfo ("Allowance when routing around tilting holes", null!),
            new _TextControlInfo ("Max.head down traverse distance", null!),

            new GroupControlInfo ("Laser Heads", null!),
            new CheckControlInfo ("Cute with single head", null!),
         ]);
   }
}