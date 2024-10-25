using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Settings;

namespace FChassis.UI.Settings.Machine.General;
public partial class MachineSettings : Panel {
   public MachineSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo("General"),
            new _TextControlInfo ("Machine Id") {bindInfos=[ControlInfo.Text.Binding("MachineId")] },
            new _TextControlInfo ("Axis emulation") {bindInfos=[ControlInfo.Text.Binding("AxisEmulation")] },
            new _TextControlInfo ("Cfg custom tech") {bindInfos=[ControlInfo.Text.Binding("CfgCustomTech")] },
            new _TextControlInfo ("Pesets"),
            new ComboControlInfo ("Can open bit rate", null!, "kbps"),
            new CheckControlInfo ("Netdisk server IP address"),
            new ComboControlInfo ("Software limit code") {bindInfos=[ControlInfo.Combo.Binding("Code"), ControlInfo.Combo.BindingItems("Codes")]},
            new _TextControlInfo ("Override limit") {bindInfos= [ControlInfo.Text.Binding("OverrideLimit")]},
            new ButtonControlInfo ("Limit Incrementer") {bindInfos=[ControlInfo.Button.Binding("IncrementLimitCommand")]},

            new GroupControlInfo ("Controller"),
            new _TextControlInfo ("Interpolation cycle time",null!, "ms"),
            new _TextControlInfo ("Interpolation divider"),
            new _TextControlInfo ("Handwheel filetr time", null!, "ms"),
            new _TextControlInfo ("Velocity", null!, "m/min"),
            new _TextControlInfo ("Acceleration", null!, "m/sec²"),
            new _TextControlInfo ("Deceleration", null!, "m/sec²"),
            new _TextControlInfo ("Ramp time", null!, "ms"),
            new _TextControlInfo ("Position tolerance MM", null!, "mm"),
            new _TextControlInfo ("Position tolerance Degree", null!, "°"),
            new _TextControlInfo ("Quick stop time", null!, "ms"),
            new _TextControlInfo ("Creep speed velocity", null!, "m/min"),

            new GroupControlInfo ("Memory reservation"),
            new _TextControlInfo ("Block count"),
            new _TextControlInfo ("Reverse Block count"),
            new _TextControlInfo ("Parameter aray size"),
      });
   }
}