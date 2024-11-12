using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.PLCOptions;
using FChassis.Data.ViewModel;
using FChassis.Data.ViewModel.Settings.Machine.PLCOptions;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class PLCKeySettings : Panel {
   public PLCKeySettings () {
      AvaloniaXamlLoader.Load (this);

      this.AddPropControls(typeof(PLCKey),MainViewModel.plcKeyVM);
   }
}