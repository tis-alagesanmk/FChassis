using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.TechParams;
using FChassis.Data.ViewModels.Setting.Machine.TechParams;
using System.Linq;

namespace FChassis.UI.Settings.Machine.TechParams; 
public partial class ExhaustSysSettings : Panel {   
   public ExhaustSysSettings () {
      AvaloniaXamlLoader.Load (this);
     
      this.AddPropControls (typeof(ExhaustSystem));
   }
}