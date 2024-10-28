using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class ProfileCamSettings : Panel{

   public ProfileCamSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new CheckControlInfo{label="Advanced"},

            new GroupControlInfo{label="Cutting"},
            new ComboControlInfo{label="Choose cutting condition by"},
            new ComboControlInfo{label="Process for open polylines"},
            new _TextControlInfo{label="Stitch cutting threshold distance (0=disable)"},

            new GroupControlInfo{label="Pierce settings"},
            new CheckControlInfo{label="Allow approach that is more than 0.5 distance to opposite side"},

            new GroupControlInfo{label="Scrap cutting"},
            new _TextControlInfo{label="Scrap grid width"},
            new _TextControlInfo{label="Approach length for separating cuts"},
      ]);
   }
}