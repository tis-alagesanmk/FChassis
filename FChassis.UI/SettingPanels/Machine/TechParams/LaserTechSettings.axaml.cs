using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class LaserTechSettings : Panel {
   public LaserTechSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Custom parameters 1"),
            new _TextControlInfo ("X-axis park position", null!, "mm"),
            new _TextControlInfo ("Y-axis park position", null!, "mm"),
            new _TextControlInfo ("Test Run Feedrate", null!, "mm/min"),
            new _TextControlInfo ("Frog Jump adjust distance", null!, "mm"),
            new _TextControlInfo ("Frog Jump adjust time", null!, "ms"),
            new _TextControlInfo ("Contour End Control Time OFF", null!, "ms"),
            new _TextControlInfo ("Contour End Parameter Time OFF", null!, "ms"),
            new _TextControlInfo ("Delay before start of cut", null!, "ms"),
            new _TextControlInfo ("Delay after end of cut", null!, "ms"),
            new _TextControlInfo ("Laser HV ON Delay", null!, "s"),

            new GroupControlInfo ("Custom parameters 2"),
            new _TextControlInfo ("High Pressure Valve", null!, "bar"),
            new _TextControlInfo ("Low Pressure Value", null!, "bar"),
            new _TextControlInfo ("Gas Pressure difference to generate error", null!, "%"),
            new _TextControlInfo ("High Pressure to clean kerf", null!, "bar"),
            new _TextControlInfo ("Focus Ref Voltage", null!, "V"),
            new _TextControlInfo ("Focus Offset Voltage", null!, "V"),
            new _TextControlInfo ("Contour Length/No. of Pierce for Nozzle Cleaning", null!, "mm"),
            new _TextControlInfo ("Height Control Sensor Gain", null!),
            new _TextControlInfo ("Height Control Retract Speed", null!, "mm/min"),
            new _TextControlInfo ("Collision delay time", null!, "ms"),

            new GroupControlInfo ("Custom parameters 3"),
            new _TextControlInfo ("Gas Idle purge pressure", null!, "bar"),
            new _TextControlInfo ("Z-axis Negative Limit - Shuttle Table", null!, "mm"),
            new _TextControlInfo ("Exhaust Lag Time to Stop", null!, "s"),
            new _TextControlInfo ("Min distance between 2 gantries", null!, "mm"),
            new _TextControlInfo ("Tandem maximum Machine Stroke", null!, "mm"),
            new _TextControlInfo ("Gas Idle Purge time", null!, "s"),
            new _TextControlInfo ("Gas Purge pressure before start of program", null!, "bar"),
            new _TextControlInfo ("Gas Purge time before start of program", null!, "s"),
            new _TextControlInfo ("Z-axis park position", null!, "mm"),
            new _TextControlInfo ("X-axis Limit value for Nozzle Cleaning", null!, "mm"),

            new GroupControlInfo ("Custom parameters 4"),
            new _TextControlInfo ("Piercing Sensor Delay Time", null!, "ms"),
            new _TextControlInfo ("Param 2", null!),
            new _TextControlInfo ("Param 3", null!),
            new _TextControlInfo ("Param 4", null!),
            new _TextControlInfo ("Param 5", null!),
            new _TextControlInfo ("Param 6", null!),
            new _TextControlInfo ("Param 7", null!),
            new _TextControlInfo ("Param 8", null!),
            new _TextControlInfo ("Param 9", null!),
            new _TextControlInfo ("Param 10", null!),
      ]);
   }
}