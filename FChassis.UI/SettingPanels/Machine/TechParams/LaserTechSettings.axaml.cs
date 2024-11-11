using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.TechParams;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class LaserTechSettings : Panel {
   public LaserTechSettings () {
      AvaloniaXamlLoader.Load (this);

       this.AddPropControls (typeof (LaserTechnology));
   }
}