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
         new ControlInfo{type=ControlInfo.Type.Text_, label="Axis emulation" },
         new ControlInfo{type=ControlInfo.Type.Text_, label="Cfg custom tech"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Pesets"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Can open bit rate", unit="kbps"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Netdisk server IP address"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Software limit code"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Override limit"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Controller"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Interpolation cycle time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Interpolation divider"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Handwheel filetr time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Velocity",unit="m/min"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Acceleration",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Deceleration",unit="m/sec²"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Ramp time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Position tolerance MM",unit="mm"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Position tolerance Degree",unit="°"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Quick stop time",unit="ms"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Creep speed velocity",unit="m/min"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Memory reservation"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Block count"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Reverse Block count"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Parameter aray size"},

      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }
}