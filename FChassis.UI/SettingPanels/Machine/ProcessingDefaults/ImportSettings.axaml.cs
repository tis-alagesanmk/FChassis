using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.ProcessingDefaults;
using FChassis.Data.ViewModels.Setting.Machine.ProcessingDefaults;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class ImportSettings : Panel {

   public ImportSettings () {
      AvaloniaXamlLoader.Load (this);

      ImportViewModel vm = new ImportViewModel ();
      this.DataContext = vm;

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddPropControls (grid, typeof (Import));
   }
}