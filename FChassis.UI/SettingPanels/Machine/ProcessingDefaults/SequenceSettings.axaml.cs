using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;

public partial class SequenceSettings : Panel{

   public SequenceSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }


   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type=ControlInfo.Type.Group, label="Laser Sequence"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Laser processing sequence"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Laser Seq"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Do pre-piercing part-by-part"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Route Traverse"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Move pierce points to reduce traverse"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Move pierce points to prevent tilting"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Microjoint nested holes if tilting"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Ignore holes smaller tha this"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Minimum cutting head height when traversing"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Route traverse lines around holes"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Allowance when routing around holes"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="lift nozzle if routing penalty more than"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Allowance when routing around tilting holes"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Max.head down traverse distance"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Laser Heads"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Cute with single head"},

      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }

}