using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Cutting;
using FChassis.Data.ViewModel;

namespace FChassis.UI.Settings.Laser.LaserCutting.Cutting;
public partial class LargeSettings : Panel {
   public LargeSettings() {
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof (Large),MainViewModel.largeVM);
   }
}