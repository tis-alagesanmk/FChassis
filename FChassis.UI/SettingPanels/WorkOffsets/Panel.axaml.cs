using FChassis.UI.Panels;

namespace FChassis.UI.Settings.WorkOffsets;
public partial class Panel : FChassis.UI.Settings.Panel {
   private void CloseBtn_Click (object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Child.mainWindow?.Switch2MainPanel ();
   }
}
