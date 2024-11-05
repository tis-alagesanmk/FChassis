using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.ProcessingDefaults;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class ProfileCamSettings : Panel{

   public ProfileCamSettings () {
      AvaloniaXamlLoader.Load (this);

      this.AddPropControls (typeof(ProfileCam));
   }
}