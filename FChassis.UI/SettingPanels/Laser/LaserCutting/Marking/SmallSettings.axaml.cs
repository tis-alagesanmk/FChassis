using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Marking;
using FChassis.Data.ViewModel;

namespace FChassis.UI.Settings.Laser.LaserCutting.Marking;
public partial class SmallSettings : Panel {
   public SmallSettings () { 
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls (typeof (Small),MainViewModel.smallVM);
   }
}