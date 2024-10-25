using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class ProfileCamSettings : Panel{

   public ProfileCamSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new CheckControlInfo ("Advanced", null!),

            new GroupControlInfo ("Cutting"),
            new ComboControlInfo ("Choose cutting condition by", null!),
            new ComboControlInfo ("Process for open polylines", null!),
            new _TextControlInfo ("Stitch cutting threshold distance (0=disable)", null!),

            new GroupControlInfo ("Pierce settings", null!),
            new CheckControlInfo ("Allow approach that is more than 0.5 distance to opposite side", null!),

            new GroupControlInfo ("Scrap cutting"),
            new _TextControlInfo ("Scrap grid width", null!),
            new _TextControlInfo ("Approach length for separating cuts", null!),
      ]);
   }
}