using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Settings;

namespace FChassis.UI.Settings.Machine.General;
public partial class MachineSettings : Panel {
   public MachineSettings () {
      AvaloniaXamlLoader.Load (this);

      var vm = this.DataContext as Data.ViewModel.Settings.Machine.General.MachineViewModel;
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo("General"),
            new _TextControlInfo ("Machine Id", nameof(vm.MachineId)),
            new _TextControlInfo ("Axis emulation", nameof(vm.AxisEmulation)),
            new _TextControlInfo ("Cfg custom tech", nameof(vm.CfgCustomTech)),
            new _TextControlInfo ("Pesets", null!),
            new ComboControlInfo ("Can open bit rate", null!, "kbps"),
            new CheckControlInfo ("Netdisk server IP address", null!),
            new ComboControlInfo ("Software limit code", nameof(vm.Code), "Codes"),
            new _TextControlInfo ("Override limit", nameof(vm.OverrideLimit)),
            new ButtonControlInfo ("Limit Incrementer", nameof(vm.IncrementLimitCommand)),

            new GroupControlInfo ("Controller"),
            new _TextControlInfo ("Interpolation cycle time", null!, "ms"),
            new _TextControlInfo ("Interpolation divider", null!),
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
            new _TextControlInfo ("Block count", null!),
            new _TextControlInfo ("Reverse Block count", null!),
            new _TextControlInfo ("Parameter aray size", null!),
         ]);
   }
}