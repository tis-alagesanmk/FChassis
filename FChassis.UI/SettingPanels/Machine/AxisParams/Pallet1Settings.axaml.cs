using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class Pallet1Settings : Panel {
   public Pallet1Settings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Configuration parameters"),
            new CheckControlInfo ("Advanced", null!),
            new ComboControlInfo ("Axis Type", null!, null!),
            new ComboControlInfo ("Axis Connection", null!, null!),
            new _TextControlInfo ("Axis address", null!),
            new _TextControlInfo ("Interpolation filter time", null!, "s"),

            new GroupControlInfo ("Resolution"),
            new _TextControlInfo ("Increments per distance", null!),
            new _TextControlInfo ("Average Power", null!, "watts"),

            new GroupControlInfo ("Monitoring"),
            new CheckControlInfo ("Software limit active", null!),
            new _TextControlInfo ("Software limit negative", null!, "mm"),
            new _TextControlInfo ("Software limit positive", null!, "mm"),
            new _TextControlInfo ("Exact stop lag window", null!, "mm"),
            new _TextControlInfo ("Exact stop time window", null!, "s"),

            new GroupControlInfo ("Referencing"),
            new _TextControlInfo ("Homing velocity2", null!, "m/min"),
            new _TextControlInfo ("Homing acceleration2", null!, "m/sec²"),
            new _TextControlInfo ("Homing mode", null!),
            new _TextControlInfo ("Homing velocity1", null!, "m/min"),
            new _TextControlInfo ("Homing acceleration1", null!, "m/sec²"),
            new _TextControlInfo ("Homing offset", null!, "mm"),
            new _TextControlInfo ("Homing direction and sequence", null!),

            new GroupControlInfo ("Speed &amp; Acceleration"),
            new _TextControlInfo ("Modal velocity", null!, "m/min"),
            new _TextControlInfo ("Velocity", null!, "m/min"),
            new _TextControlInfo ("Acceleration", null!, "m/sec²"),
            new _TextControlInfo ("Deceleration", null!, "m/sec²"),
            new _TextControlInfo ("Ramp Time", null!, "ms"),

            new GroupControlInfo ("Corrections"),
            new _TextControlInfo ("Backlash compensation", null!, "mm"),

            new GroupControlInfo ("Synchronous"),
            new _TextControlInfo ("Synchronous offset", null!, "mm"),
            new _TextControlInfo ("Synchronous position deviation", null!, "mm"),

            new GroupControlInfo ("Handwheel"),
            new _TextControlInfo ("Handwheel assignment", null!),
            new _TextControlInfo ("Handwheel factor", null!),
      ]);
   }
}