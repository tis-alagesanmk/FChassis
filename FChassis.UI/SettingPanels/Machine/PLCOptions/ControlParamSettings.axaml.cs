using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class ControlParamSettings : Panel {
   public ControlParamSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new[] {
         new GroupControlInfo{label="Maximum override adjust level"},
         new _TextControlInfo{label="Maximum override adjust value", unit="%"},

         new GroupControlInfo{label="Height control 2 steps only for piercing"},
         new _TextControlInfo{label="HC minimum height for 2 steps", unit="mm"},

         new GroupControlInfo{label="Auto lubrication cycle"},
         new _TextControlInfo{label="Number of lubrication values"},
         new _TextControlInfo{label="X-axis lubrication feedrate", unit="mm/min"},
         new _TextControlInfo{label="Y-axis lubrication feedrate", unit="mm/min"},
         new _TextControlInfo{label="Lubrication On delay", unit="s"},

         new GroupControlInfo{label="Adjust gas pressure online"},
         new _TextControlInfo{label="Maximum gas pressure adjust", unit="bar"},
         new _TextControlInfo{label="Gas pressure adjust steps"},

         new GroupControlInfo{label="Height sensor calibration program"},
         new _TextControlInfo{label="Offset distance from negative limit for slow speed", unit="mm"},
         new _TextControlInfo{label="Tip touch offset value", unit="mm"},

         new GroupControlInfo{label="Edge detection program"},
         new _TextControlInfo{label="Slat start point along X-axis", unit="mm"},
         new _TextControlInfo{label="Slat start point along Y-axis", unit="mm"},
         new _TextControlInfo{label="Slats equal distance along X-axis", unit="mm"},
         new _TextControlInfo{label="Slats Peak to Peak distance along Y-axis", unit="mm"},
         new _TextControlInfo{label="Edge correction offset along X-axis", unit="mm"},
         new _TextControlInfo{label="Edge correction offset along Y-axis", unit="mm"},
         new _TextControlInfo{label="Speed to detect the edge", unit="mm/min"},
         new CheckControlInfo{label="Start height sensor calibration before edge detect"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Acknowledge the sheet origin point"},

         new GroupControlInfo{label="Sheet stopper specification"},
         new _TextControlInfo{label="Number of stoppers along X-axis"},
         new _TextControlInfo{label="Number of stoppers along Y-axis"},
         new _TextControlInfo{label="Stopper width along Y-axis", unit="mm"},
         new _TextControlInfo{label="Distance of first stopper from origin along X-axis", unit="mm"},
         new _TextControlInfo{label="Distance between first and second stopper along X-axis", unit="mm"},
         new _TextControlInfo{label="Distance between second and third stopper along X-axis", unit="mm"},
         new _TextControlInfo{label="Distance of first stopper from origin along Y-axis", unit="mm"},
         new _TextControlInfo{label="Distance between first and second stopper along Y-axis", unit="mm"},
         new _TextControlInfo{label="Distance between second and third stopper along Y-axis", unit="mm"},

         new GroupControlInfo{label="Suction anti blow valves configuration"},
         new _TextControlInfo{label="Number of anti blow valves"},
         new _TextControlInfo{label="Anti blow valve ON time", unit="s"},
         new _TextControlInfo{label="Anti blow valve wait time", unit="s"},
         new CheckControlInfo{label="Filter sensor"},

         new GroupControlInfo{label="Gas Pressure Adjust Levels for Analogure Control"},
         new _TextControlInfo{label="HP gas regulator command correction", unit="%"},
         new _TextControlInfo{label="HP gas regulator feedback correction", unit="%"},
         new _TextControlInfo{label="LP gas regulator command correction", unit="%"},
         new _TextControlInfo{label="LP gas regulator feedback correction", unit="%"},

         new GroupControlInfo{label="Pallet changer - up and down motion hydraulic control"},
         new _TextControlInfo{label="Up solenoid off delay time", unit="s"},
         new _TextControlInfo{label="Hydraulic motor off delay time", unit="s"},
         new _TextControlInfo{label="Hydraulic motor ON - max time out", unit="s"},
         new _TextControlInfo{label="Fwd/Rev fast move time out", unit="s"},
         new _TextControlInfo{label="Fwd/Rev slow move time out", unit="s"},
         new ComboControlInfo{label="Fwd/Rev button function type"},

         new GroupControlInfo{label="Nozzle cleaning &amp; height sensor calibration offsets"},
         new _TextControlInfo{label="Nozzle clean X-offset", unit="mm"},
         new _TextControlInfo{label="Nozzle clean Y-offset", unit="mm"},
         new _TextControlInfo{label="Nozzle clean Z-offset", unit="mm"},
         new _TextControlInfo{label="HS calibration X-offset", unit="mm"},
         new _TextControlInfo{label="HS calibration Y-offset", unit="mm"},

         new GroupControlInfo{label="Cutting head warning levels"},
         new _TextControlInfo{label="Sensor insert temperature", unit="°C"},
         new _TextControlInfo{label="Plasma value percentage", unit="°C"},
         new _TextControlInfo{label="Protective window temperature", unit="°C"},
         new _TextControlInfo{label="Collimating lens temperature", unit="°C"},
         new _TextControlInfo{label="Focal lens temperature", unit="°C"},
         new _TextControlInfo{label="Cutting head temperature", unit="°C"},
         new _TextControlInfo{label="Diffusion light level"},

         new GroupControlInfo{label="Nozzle changer configuration"},
         new _TextControlInfo{label="Set torque value for opening", unit="%"},
         new _TextControlInfo{label="Set torque value for closing", unit="%"},
         new _TextControlInfo{label="Opening delay time", unit="s"},
         new _TextControlInfo{label="Closing delay time", unit="s"},
         new _TextControlInfo{label="Unwinding nozzle position", unit="°"},

         new GroupControlInfo{label="Travesal Blow Valve Configuration"},
         new ComboControlInfo{label="Control method", unit="ms"},
         new ComboControlInfo{label="Maximum Pressure", unit="ms"},

         new GroupControlInfo{label="Laser pulsing gate(LPG) delay time"},
         new _TextControlInfo{label="LPG On delay", unit="ms"},
         new _TextControlInfo{label="LPG Off delay", unit="ms"},

         new GroupControlInfo{label="Sealing gas pressure monitor\""},
         new _TextControlInfo{label="Minimum warning level", unit="mbar"},
         new _TextControlInfo{label="Maximum warning level", unit="mbar"},
         new _TextControlInfo{label="Minimum error level", unit="mbar"},
         new _TextControlInfo{label="Maximum error level", unit="mbar"},

         new GroupControlInfo{label="Protective glass monitor"},
         new _TextControlInfo{label="Offline broken factor"},
         new _TextControlInfo{label="Online broken factor"},
         new _TextControlInfo{label="Warning limit", unit="%"},
         new _TextControlInfo{label="Error limit", unit="%"},
         new _TextControlInfo{label="Delay time to report warning", unit="s"},
      });
   }
}