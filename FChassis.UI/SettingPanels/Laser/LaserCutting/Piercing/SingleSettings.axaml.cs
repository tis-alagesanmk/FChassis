using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Piercing;
using FChassis.Data.ViewModel;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class SingleSettings : Panel {
   public SingleSettings () {
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof(Single), MainViewModel.singleVM);
   }
}