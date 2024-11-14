using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Evapourating;
using FChassis.Data.ViewModel;

namespace FChassis.UI.Settings.Laser.LaserCutting.Evapourating;
public partial class SmallSettings : Panel {
   public SmallSettings () { 
      AvaloniaXamlLoader.Load (this);
      this.AddPropControls(typeof(Small),MainViewModel.evapouratingSmallVM);
   }
}