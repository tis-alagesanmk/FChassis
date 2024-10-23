using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Settings;

namespace FChassis.UI.Settings.Machine.General;
public partial class MachineSettings : Panel {
   public MachineSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("General"),
            new ComboControlInfo ("Match Id"),
            new _TextControlInfo ("Axis emulation"),
            new _TextControlInfo ("Cfg custom tech"),
            new _TextControlInfo ("Pesets"),
            new ComboControlInfo ("Can open bit rate") {unit="kbps"},
            new CheckControlInfo ("Netdisk server IP address"),
            new CheckControlInfo ("Software limit code"),
            new ComboControlInfo ("Override limit"),

            new GroupControlInfo ("Controller"),
            new _TextControlInfo ("Interpolation cycle time", "BindName", "ms"),
            new _TextControlInfo ("Interpolation divider"),
            new _TextControlInfo ("Handwheel filetr time", "BindName", "ms"),
            new _TextControlInfo ("Velocity", "BindName", "m/min"),
            new _TextControlInfo ("Acceleration", "BindName", "m/sec²"),
            new _TextControlInfo ("Deceleration", "BindName", "m/sec²"),
            new _TextControlInfo ("Ramp time", "BindName", "ms"),
            new _TextControlInfo ("Position tolerance MM", "BindName", "mm"),
            new _TextControlInfo ("Position tolerance Degree", "BindName", "°"),
            new _TextControlInfo ("Quick stop time", "BindName", "ms"),
            new _TextControlInfo ("Creep speed velocity", "BindName", "m/min"),

            new GroupControlInfo ("Memory reservation"),
            new _TextControlInfo ("Block count"),
            new _TextControlInfo ("Reverse Block count"),
            new _TextControlInfo ("Parameter aray size"),
      ]);
   }
}