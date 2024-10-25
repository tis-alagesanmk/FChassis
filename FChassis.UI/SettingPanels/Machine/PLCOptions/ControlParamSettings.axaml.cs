using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class ControlParamSettings : Panel {
   public ControlParamSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Maximum override adjust level"),
            new _TextControlInfo ("Maximum override adjust value", null!, "%"),

            new GroupControlInfo ("Height control 2 steps only for piercing"),
            new _TextControlInfo ("HC minimum height for 2 steps", null!, "mm"),

            new GroupControlInfo ("Auto lubrication cycle"),
            new _TextControlInfo ("Number of lubrication values", null!),
            new _TextControlInfo ("X-axis lubrication feedrate", null!, "mm/min"),
            new _TextControlInfo ("Y-axis lubrication feedrate", null!, "mm/min"),
            new _TextControlInfo ("Lubrication On delay", null!, "s"),

            new GroupControlInfo ("Adjust gas pressure online"),
            new _TextControlInfo ("Maximum gas pressure adjust", null!, "bar"),
            new _TextControlInfo ("Gas pressure adjust steps", null!),

            new GroupControlInfo ("Height sensor calibration program"),
            new _TextControlInfo ("Offset distance from negative limit for slow speed", null!, "mm"),
            new _TextControlInfo ("Tip touch offset value", null!, "mm"),

            new GroupControlInfo ("Edge detection program"),
            new _TextControlInfo ("Slat start point along X-axis", null!, "mm"),
            new _TextControlInfo ("Slat start point along Y-axis", null!, "mm"),
            new _TextControlInfo ("Slats equal distance along X-axis", null!, "mm"),
            new _TextControlInfo ("Slats Peak to Peak distance along Y-axis", null!, "mm"),
            new _TextControlInfo ("Edge correction offset along X-axis", null!, "mm"),
            new _TextControlInfo ("Edge correction offset along Y-axis", null!, "mm"),
            new _TextControlInfo ("Speed to detect the edge", null!, "mm/min"),
            new CheckControlInfo ("Start height sensor calibration before edge detect", null!),
            new CheckControlInfo ("Acknowledge the sheet origin point", null!),

            new GroupControlInfo ("Sheet stopper specification"),
            new _TextControlInfo ("Number of stoppers along X-axis", null!),
            new _TextControlInfo ("Number of stoppers along Y-axis", null!),
            new _TextControlInfo ("Stopper width along Y-axis", null!, "mm"),
            new _TextControlInfo ("Distance of first stopper from origin along X-axis", null!, "mm"),
            new _TextControlInfo ("Distance between first and second stopper along X-axis", null!, "mm"),
            new _TextControlInfo ("Distance between second and third stopper along X-axis", null!, "mm"),
            new _TextControlInfo ("Distance of first stopper from origin along Y-axis", null!, "mm"),
            new _TextControlInfo ("Distance between first and second stopper along Y-axis", null!, "mm"),
            new _TextControlInfo ("Distance between second and third stopper along Y-axis", null!, "mm"),

            new GroupControlInfo ("Suction anti blow valves configuration"),
            new _TextControlInfo ("Number of anti blow valves", null!),
            new _TextControlInfo ("Anti blow valve ON time", null!, "s"),
            new _TextControlInfo ("Anti blow valve wait time", null!, "s"),
            new CheckControlInfo ("Filter sensor", null!),

            new GroupControlInfo ("Gas Pressure Adjust Levels for Analogure Control"),
            new _TextControlInfo ("HP gas regulator command correction", null!, "%"),
            new _TextControlInfo ("HP gas regulator feedback correction", null!, "%"),
            new _TextControlInfo ("LP gas regulator command correction", null!, "%"),
            new _TextControlInfo ("LP gas regulator feedback correction", null!, "%"),

            new GroupControlInfo ("Pallet changer - up and down motion hydraulic control"),
            new _TextControlInfo ("Up solenoid off delay time", null!, "s"),
            new _TextControlInfo ("Hydraulic motor off delay time", null!, "s"),
            new _TextControlInfo ("Hydraulic motor ON - max time out", null!, "s"),
            new _TextControlInfo ("Fwd/Rev fast move time out", null!, "s"),
            new _TextControlInfo ("Fwd/Rev slow move time out", null!, "s"),
            new ComboControlInfo ("Fwd/Rev button function type", null!, null!),

            new GroupControlInfo ("Nozzle cleaning &amp; height sensor calibration offsets"),
            new _TextControlInfo ("Nozzle clean X-offset", null!, "mm"),
            new _TextControlInfo ("Nozzle clean Y-offset", null!, "mm"),
            new _TextControlInfo ("Nozzle clean Z-offset", null!, "mm"),
            new _TextControlInfo ("HS calibration X-offset", null!, "mm"),
            new _TextControlInfo ("HS calibration Y-offset", null!, "mm"),

            new GroupControlInfo ("Cutting head warning levels"),
            new _TextControlInfo ("Sensor insert temperature", null!, "°C"),
            new _TextControlInfo ("Plasma value percentage", null!, "°C"),
            new _TextControlInfo ("Protective window temperature", null!, "°C"),
            new _TextControlInfo ("Collimating lens temperature", null!, "°C"),
            new _TextControlInfo ("Focal lens temperature", null!, "°C"),
            new _TextControlInfo ("Cutting head temperature", null!, "°C"),
            new _TextControlInfo ("Diffusion light level", null!),

            new GroupControlInfo ("Nozzle changer configuration"),
            new _TextControlInfo ("Set torque value for opening", null!, "%"),
            new _TextControlInfo ("Set torque value for closing", null!, "%"),
            new _TextControlInfo ("Opening delay time", null!, "s"),
            new _TextControlInfo ("Closing delay time", null!, "s"),
            new _TextControlInfo ("Unwinding nozzle position", null!, "°"),

            new GroupControlInfo ("Travesal Blow Valve Configuration"),
            new ComboControlInfo ("Control method", null!, "ms"),
            new ComboControlInfo ("Maximum Pressure", null!, "ms"),

            new GroupControlInfo ("Laser pulsing gate(LPG) delay time"),
            new _TextControlInfo ("LPG On delay", null!, "ms"),
            new _TextControlInfo ("LPG Off delay", null!, "ms"),

            new GroupControlInfo ("Sealing gas pressure monitor\""),
            new _TextControlInfo ("Minimum warning level", null!, "mbar"),
            new _TextControlInfo ("Maximum warning level", null!, "mbar"),
            new _TextControlInfo ("Minimum error level", null!, "mbar"),
            new _TextControlInfo ("Maximum error level", null!, "mbar"),

            new GroupControlInfo ("Protective glass monitor"),
            new _TextControlInfo ("Offline broken factor", null!),
            new _TextControlInfo ("Online broken factor", null!),
            new _TextControlInfo ("Warning limit", null!, "%"),
            new _TextControlInfo ("Error limit", null!, "%"),
            new _TextControlInfo ("Delay time to report warning", null!, "s"),
      ]);
   }
}