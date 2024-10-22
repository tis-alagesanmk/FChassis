using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class MachineSettings : Panel {
   public MachineSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo{label="General"},
            new ComboControlInfo{label="Match Id"},
            new _TextControlInfo{label="Axis emulation" },
            new _TextControlInfo{label="Cfg custom tech"},
            new _TextControlInfo{label="Pesets"},
            new ComboControlInfo{label="Can open bit rate", unit="kbps"},
            new CheckControlInfo{label="Netdisk server IP address"},
            new CheckControlInfo{label="Software limit code"},
            new ComboControlInfo{label="Override limit"},

            new GroupControlInfo{label="Controller"},
            new _TextControlInfo{label="Interpolation cycle time",unit="ms"},
            new _TextControlInfo{label="Interpolation divider"},
            new _TextControlInfo{label="Handwheel filetr time",unit="ms"},
            new _TextControlInfo{label="Velocity",unit="m/min"},
            new _TextControlInfo{label="Acceleration",unit="m/sec²"},
            new _TextControlInfo{label="Deceleration",unit="m/sec²"},
            new _TextControlInfo{label="Ramp time",unit="ms"},
            new _TextControlInfo{label="Position tolerance MM",unit="mm"},
            new _TextControlInfo{label="Position tolerance Degree",unit="°"},
            new _TextControlInfo{label="Quick stop time",unit="ms"},
            new _TextControlInfo{label="Creep speed velocity",unit="m/min"},

            new GroupControlInfo{label="Memory reservation"},
            new _TextControlInfo{label="Block count"},
            new _TextControlInfo{label="Reverse Block count"},
            new _TextControlInfo{label="Parameter aray size"},
      });
   }
}