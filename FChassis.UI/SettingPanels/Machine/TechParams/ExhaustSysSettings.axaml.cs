using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.TechParams;

namespace FChassis.UI.Settings.Machine.TechParams; 
public partial class ExhaustSysSettings : Panel {   
   public ExhaustSysSettings () {
      AvaloniaXamlLoader.Load (this);
     
      this.AddPropControls (typeof(ExhaustSystem));
   }
}