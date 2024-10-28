using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Model.Settings.Machine.General;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddPropControls (grid, typeof (HMI));
   }
}