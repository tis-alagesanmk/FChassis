using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class LaserTechSettings : Panel {
   public LaserTechSettings () {
      AvaloniaXamlLoader.Load (this);

      ControlInfo[] ctrlInfos = new[] {
         new ControlInfo{type=ControlInfo.Type.Group, label="Custom parameters 1" },
         new ControlInfo{type=ControlInfo.Type.Text_,  label="X-axis park position", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Y-axis park position", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Test Run Feedrate", unit="mm/min"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Frog Jump adjust distance", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Frog Jump adjust time", unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Contour End Control Time OFF", unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Contour End Parameter Time OFF", unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Delay before start of cut", unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Delay after end of cut", unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Laser HV ON Delay", unit="s"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Custom parameters 2" },
         new ControlInfo{type=ControlInfo.Type.Text_,  label="High Pressure Valve", unit="bar"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Low Pressure Value", unit="bar"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Gas Pressure difference to generate error", unit="%"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="High Pressure to clean kerf", unit="bar"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Focus Ref Voltage", unit="V"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Focus Offset Voltage", unit="V"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Contour Length/No. of Pierce for Nozzle Cleaning", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Height Control Sensor Gain"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Height Control Retract Speed", unit="mm/min"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Collision delay time", unit="ms"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Custom parameters 3" },
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Gas Idle purge pressure", unit="bar"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Z-axis Negative Limit - Shuttle Table", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Exhaust Lag Time to Stop", unit="s"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Min distance between 2 gantries", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Tandem maximum Machine Stroke", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Gas Idle Purge time", unit="s"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Gas Purge pressure before start of program", unit="bar"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Gas Purge time before start of program", unit="s"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Z-axis park position", unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="X-axis Limit value for Nozzle Cleaning", unit="mm"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Custom parameters 4" },
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Piercing Sensor Delay Time", unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 2"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 3"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 4"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 5"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 6"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 7"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 8"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 9"},
         new ControlInfo{type=ControlInfo.Type.Text_,  label="Param 10"},
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);
   }
}