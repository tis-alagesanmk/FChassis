using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Cutting;
using FChassis.Data.ViewModel;

namespace FChassis.UI.Settings.Laser.LaserCutting.Cutting;
public partial class SmallSettings : Panel {
   public SmallSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof (Small), MainViewModel.smallVM);
   }
}