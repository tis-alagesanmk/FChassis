using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Piercing;
using FChassis.Data.ViewModel;

namespace FChassis.UI.Settings.Laser.LaserCutting.Piercing;
public partial class MultipleSettings : Panel {
   public MultipleSettings () { 
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof(Multiple), MainViewModel.multipleVM);
   }
}