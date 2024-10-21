using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class MachineSettings : Panel {

   public MachineSettings () 
   {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }
   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type=ControlInfo.Type.Group, label="General"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Match Id"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Axis emulation" },
         new ControlInfo{type=ControlInfo.Type.Text, label="Cfg custom tech"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Pesets"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Can open bit rate", unit="kbps"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Netdisk server IP address"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Software limit code"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Override limit"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Controller"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Interpolation cycle time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Interpolation divider"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Handwheel filetr time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Velocity",unit="m/min"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Acceleration",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Deceleration",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Ramp time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Position tolerance MM",unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Position tolerance Degree",unit="°"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Quick stop time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Creep speed velocity",unit="m/min"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Memory reservation"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Block count"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Reverse Block count"},
         new ControlInfo{type=ControlInfo.Type.Text, label="Parameter aray size"},

      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }
}