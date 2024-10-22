using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Panels;

namespace FChassis.UI.Settings.WorkOffsets;
public partial class Panel : Settings.Panel {
   public Panel () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[1] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new[] { createDGrid () } );

      #region Local function
      DGridControlInfo createDGrid () {
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "bindingHere",
            columns = new[] {
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "X"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Y"},
               new DGridControlInfo.ColInfo {type = ControlInfo.Type.Text_, header = "Z"},
         }};

         return dGridCrtlInfo;
      }
      #endregion Local function
   }

   private void CloseBtn_Click (object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Child.mainWindow?.Switch2MainPanel ();}
}
