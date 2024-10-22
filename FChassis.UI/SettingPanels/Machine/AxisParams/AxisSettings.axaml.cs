using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class AxisSettingsPanel : Panel{
 public AxisSettingsPanel() {
      AvaloniaXamlLoader.Load(this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo{label="Configuration parameters"},
            new CheckControlInfo{label="Axis type"},
            new ComboControlInfo{label="Axis connection"},
            new ComboControlInfo{label="Axis address"},
            new _TextControlInfo{label="Sync connection"},
            new _TextControlInfo{label="Interpolation filter time",unit="s"},

            new GroupControlInfo{label="Resolution"},
            new _TextControlInfo{label="Increaments per distance"},
            new _TextControlInfo{label="Distance"},

            new GroupControlInfo{label="Monitoring"},
            new _TextControlInfo{label="Software limit active",unit="mm"},
            new _TextControlInfo{label="Software limit negative",unit="mm"},
            new _TextControlInfo{label="Exact stop lag window",unit="mm"},
            new _TextControlInfo{label="Exact stop time window",unit="s"},

            new GroupControlInfo{label="Referencing"},
            new _TextControlInfo{label="Homing velocity2",unit="m/min"},
            new _TextControlInfo{label="Homing acceleration2",unit="m/sec²"},
            new _TextControlInfo{label="Homing mode"},
            new _TextControlInfo{label="Homing velocity1",unit="m/min"},
            new _TextControlInfo{label="Homing acceleration1",unit="m/sec²"},
            new _TextControlInfo{label="Homing offset",unit="mm"},
            new _TextControlInfo{label="Homing direction and sequence"},

            new GroupControlInfo{label="Speed & Acceleration"},
            new _TextControlInfo{label="Moal velocity",unit="m/min"},
            new _TextControlInfo{label="Velocity",unit="m/min"},
            new _TextControlInfo{label="Acceleration",unit="m/sec²"},
            new _TextControlInfo{label="Dcceleration",unit="m/sec²"},
            new _TextControlInfo{label="Ramp Time",unit="ms"},

            new GroupControlInfo{label="Corrections"},
            new _TextControlInfo{label="Blacklash compensation",unit="mm"},

            new GroupControlInfo{label="Synchronous"},
            new _TextControlInfo{label="Synchronous offset",unit="mm"},
            new _TextControlInfo{label="Synchronous position deviation",unit="mm"},

            new GroupControlInfo{label="Synchronous"},
            new _TextControlInfo{label="Handwheel assignment"},
            new _TextControlInfo{label="Handwheel factor"},
      });
   }
}