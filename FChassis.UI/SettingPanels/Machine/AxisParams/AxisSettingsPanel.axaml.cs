using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;

public partial class AxisSettingsPanel : FChassis.UI.Settings.Panel{

 public AxisSettingsPanel() {

      AvaloniaXamlLoader.Load(this);
      this.AddControls();
   }

   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type=ControlInfo.Type.Group, label="Configuration parameters"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Axis type"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Axis connection"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Axis address"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Sync connection"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Interpolation filter time",unit="s"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Resolution"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Increaments per distance"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Distance"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Monitoring"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Software limit active",unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Software limit negative",unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Exact stop lag window",unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Exact stop time window",unit="s"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Referencing"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Homing velocity2",unit="m/min"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Homing acceleration2",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Homing mode"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Homing velocity1",unit="m/min"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Homing acceleration1",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Homing offset",unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Homing direction and sequence"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Speed & Acceleration"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Moal velocity",unit="m/min"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Velocity",unit="m/min"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Acceleration",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Dcceleration",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Ramp Time",unit="ms"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Corrections"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Blacklash compensation",unit="mm"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Synchronous"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Synchronous offset",unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Synchronous position deviation",unit="mm"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Synchronous"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Handwheel assignment"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Handwheel factor"},
         
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }
}