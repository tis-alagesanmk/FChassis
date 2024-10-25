using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class AxisSettingsPanel : Panel{
 public AxisSettingsPanel() {
      AvaloniaXamlLoader.Load(this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Configuration parameters"),
            new CheckControlInfo ("Advanced", null!),
            new ComboControlInfo ("Axis type", null!, null!),
            new ComboControlInfo ("Axis connection", null!, null!),
            new _TextControlInfo ("Axis address", null!),
            new _TextControlInfo ("Sync connection", null!),
            new _TextControlInfo ("Interpolation filter time", null!, "s"),

            new GroupControlInfo ("Resolution"),
            new _TextControlInfo ("Increaments per distance", null!),
            new _TextControlInfo ("Distance", null!),

            new GroupControlInfo ("Monitoring"),
            new _TextControlInfo ("Software limit active", null!, "mm"),
            new _TextControlInfo ("Software limit negative", null!, "mm"),
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

            new GroupControlInfo ("Speed & Acceleration"),
            new _TextControlInfo ("Moal velocity", null!, "m/min"),
            new _TextControlInfo ("Velocity", null!, "m/min"),
            new _TextControlInfo ("Acceleration", null!, "m/sec²"),
            new _TextControlInfo ("Dcceleration", null!, "m/sec²"),
            new _TextControlInfo ("Ramp Time", null!, "ms"),

            new GroupControlInfo ("Corrections"),
            new _TextControlInfo ("Blacklash compensation", null!, "mm"),

            new GroupControlInfo ("Synchronous"),
            new _TextControlInfo ("Synchronous offset", null!, "mm"),
            new _TextControlInfo ("Synchronous position deviation", null!, "mm"),

            new GroupControlInfo ("Synchronous"),
            new _TextControlInfo ("Handwheel assignment", null!),
            new _TextControlInfo ("Handwheel factor", null!),
      ]);
   }
}