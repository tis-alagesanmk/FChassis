using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Cutting;
using FChassis.Data.ViewModel;

namespace FChassis.UI.Settings.Laser.LaserCutting.Cutting;
public partial class PreholeSettings : Panel {
   public PreholeSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof (PreHole), MainViewModel.preholeVM);
   }
}