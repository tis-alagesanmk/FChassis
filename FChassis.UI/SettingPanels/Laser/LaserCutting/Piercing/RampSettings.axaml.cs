using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Piercing;
using FChassis.Data.ViewModel;
using System.Windows.Documents;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class RampSettings : Panel {
   public RampSettings () {

      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof(Ramp),MainViewModel.rampVM);
   }
}