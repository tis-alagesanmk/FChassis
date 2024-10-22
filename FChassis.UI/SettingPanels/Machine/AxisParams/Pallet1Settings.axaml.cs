using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class Pallet1Settings : Panel {
   public Pallet1Settings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new[] {
            new GroupControlInfo{label="Configuration parameters"},
            new CheckControlInfo{label="Advanced"},
            new ComboControlInfo{label="Axis Type"},
            new ComboControlInfo{label="Axis Connection"} as ControlInfo,
            new _TextControlInfo{label="Axis address"},
            new _TextControlInfo{label="Interpolation filter time", unit="s"},

            new GroupControlInfo{label="Resolution"},
            new _TextControlInfo{label="Increments per distance"},
            new _TextControlInfo{label="Average Power", unit="watts"},

            new GroupControlInfo{label="Monitoring"},
            new CheckControlInfo{label="Software limit active"},
            new _TextControlInfo{label="Software limit negative", unit="mm"},
            new _TextControlInfo{label="Software limit positive", unit="mm"},
            new _TextControlInfo{label="Exact stop lag window", unit="mm"},
            new _TextControlInfo{label="Exact stop time window", unit="s"},

            new GroupControlInfo{label="Referencing"},
            new _TextControlInfo{label="Homing velocity2", unit="m/min"},
            new _TextControlInfo{label="Homing acceleration2", unit="m/sec²"},
            new _TextControlInfo{label="Homing mode"},
            new _TextControlInfo{label="Homing velocity1", unit="m/min"},
            new _TextControlInfo{label="Homing acceleration1", unit="m/sec²"},
            new _TextControlInfo{label="Homing offset", unit="mm"},
            new _TextControlInfo{label="Homing direction and sequence"},

            new GroupControlInfo{label="Speed &amp; Acceleration"},
            new _TextControlInfo{label="Modal velocity", unit="m/min"},
            new _TextControlInfo{label="Velocity", unit="m/min"},
            new _TextControlInfo{label="Acceleration", unit="m/sec²"},
            new _TextControlInfo{label="Deceleration", unit="m/sec²"},
            new _TextControlInfo{label="Ramp Time", unit="ms"},

            new GroupControlInfo{label="Corrections"},
            new _TextControlInfo{label="Backlash compensation", unit="mm"},

            new GroupControlInfo{label="Synchronous"},
            new _TextControlInfo{label="Synchronous offset", unit="mm"},
            new _TextControlInfo{label="Synchronous position deviation", unit="mm"},

            new GroupControlInfo{label="Handwheel"},
            new _TextControlInfo{label="Handwheel assignment"},
            new _TextControlInfo{label="Handwheel factor"},
      });
   }
}