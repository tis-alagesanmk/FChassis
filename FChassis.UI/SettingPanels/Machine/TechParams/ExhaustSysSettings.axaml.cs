using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.SettingPanels.Machine.TechParams.ViewModel;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class ExhaustSysSettings : Panel {
   public ExhaustSysSettings () {
      AvaloniaXamlLoader.Load (this);
      ExhaustSystemViewModel vm = new ExhaustSystemViewModel ();
      this.DataContext = vm;
   }
}