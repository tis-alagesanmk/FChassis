using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class LaserTechSettings : Panel {
   public LaserTechSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo{label="Custom parameters 1"},
            new _TextControlInfo{label="X-axis park position", unit="mm"},
            new _TextControlInfo{label="Y-axis park position", unit="mm"},
            new _TextControlInfo{label="Test Run Feedrate", unit="mm/min"},
            new _TextControlInfo{label="Frog Jump adjust distance", unit="mm"},
            new _TextControlInfo{label="Frog Jump adjust time", unit="ms"},
            new _TextControlInfo{label="Contour End Control Time OFF", unit="ms"},
            new _TextControlInfo{label="Contour End Parameter Time OFF", unit="ms"},
            new _TextControlInfo{label="Delay before start of cut", unit="ms"},
            new _TextControlInfo{label="Delay after end of cut", unit="ms"},
            new _TextControlInfo{label="Laser HV ON Delay", unit="s"},

            new GroupControlInfo{label="Custom parameters 2" },
            new _TextControlInfo{label="High Pressure Valve", unit="bar"},
            new _TextControlInfo{label="Low Pressure Value", unit="bar"},
            new _TextControlInfo{label="Gas Pressure difference to generate error", unit="%"},
            new _TextControlInfo{label="High Pressure to clean kerf", unit="bar"},
            new _TextControlInfo{label="Focus Ref Voltage", unit="V"},
            new _TextControlInfo{label="Focus Offset Voltage", unit="V"},
            new _TextControlInfo{label="Contour Length/No. of Pierce for Nozzle Cleaning", unit="mm"},
            new _TextControlInfo{label="Height Control Sensor Gain"},
            new _TextControlInfo{label="Height Control Retract Speed", unit="mm/min"},
            new _TextControlInfo{label="Collision delay time", unit="ms"},

            new GroupControlInfo{label="Custom parameters 3" },
            new _TextControlInfo{label="Gas Idle purge pressure", unit="bar"},
            new _TextControlInfo{label="Z-axis Negative Limit - Shuttle Table", unit="mm"},
            new _TextControlInfo{label="Exhaust Lag Time to Stop", unit="s"},
            new _TextControlInfo{label="Min distance between 2 gantries", unit="mm"},
            new _TextControlInfo{label="Tandem maximum Machine Stroke", unit="mm"},
            new _TextControlInfo{label="Gas Idle Purge time", unit="s"},
            new _TextControlInfo{label="Gas Purge pressure before start of program", unit="bar"},
            new _TextControlInfo{label="Gas Purge time before start of program", unit="s"},
            new _TextControlInfo{label="Z-axis park position", unit="mm"},
            new _TextControlInfo{label="X-axis Limit value for Nozzle Cleaning", unit="mm"},

            new GroupControlInfo{label="Custom parameters 4" },
            new _TextControlInfo{label="Piercing Sensor Delay Time", unit="ms"},
            new _TextControlInfo{label="Param 2"},
            new _TextControlInfo{label="Param 3"},
            new _TextControlInfo{label="Param 4"},
            new _TextControlInfo{label="Param 5"},
            new _TextControlInfo{label="Param 6"},
            new _TextControlInfo{label="Param 7"},
            new _TextControlInfo{label="Param 8"},
            new _TextControlInfo{label="Param 9"},
            new _TextControlInfo{label="Param 10"},
      ]);
   }
}