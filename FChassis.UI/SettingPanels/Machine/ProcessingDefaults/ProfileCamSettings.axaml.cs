using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;

public partial class ProfileCamSettings : Panel{

   public ProfileCamSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddControls ();
   }


   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type=ControlInfo.Type.Check, label="Advanced"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Cutting"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Choose cutting condition by"},
         new ControlInfo{type=ControlInfo.Type.Combo, label="Process for open polylines"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Stitch cutting threshold distance (0=disable)"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Pierce settings"},
         new ControlInfo{type=ControlInfo.Type.Check, label="Allow approach that is more than 0.5 distance to opposite side"},

         new ControlInfo{type=ControlInfo.Type.Group, label="Scrap cutting"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Scrap grid width"},
         new ControlInfo{type=ControlInfo.Type.Text_, label="Approach length for separating cuts"},

      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }

}