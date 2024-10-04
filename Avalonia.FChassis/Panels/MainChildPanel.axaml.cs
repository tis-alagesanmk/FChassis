using Avalonia.Controls;

namespace Avalonia.Desktop;
public partial class MainChildPanel : UserControl {
   static internal MainWindow? mainWindow = null;
   public MainChildPanel () {
      InitializeComponent ();
   }

   public void switchPanel(MainChildPanel panel) {
      if (panel == null || panel == this.currentPanel)
         return;

      this.currentPanel = panel;
      mainWindow.Content = panel;
   }

  MainChildPanel currentPanel = null;
}