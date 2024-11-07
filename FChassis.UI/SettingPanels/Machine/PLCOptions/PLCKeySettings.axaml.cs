using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.PLCOptions;
using FChassis.Data.ViewModels.Setting.Machine.PLCOptions;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class PLCKeySettings : Panel {
   public PLCKeySettings () {
      AvaloniaXamlLoader.Load (this);

      this.DataContext = new PLCKeyViewModel ();
      this.AddPropControls (typeof(PLCKey));
   }
}