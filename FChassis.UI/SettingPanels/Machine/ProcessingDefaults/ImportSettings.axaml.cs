using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.ProcessingDefaults;
using FChassis.Data.ViewModels.Setting.Machine.ProcessingDefaults;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class ImportSettings : Panel {

   public ImportSettings () {
      AvaloniaXamlLoader.Load (this);

      this.AddPropControls (typeof (Import));
   }
}