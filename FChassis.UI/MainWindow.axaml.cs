using Avalonia.Controls;

namespace FChassis.UI;
public partial class MainWindow : Window {
   FChassis.UI.Panels.MainPanel mainPanel = new ();
   public MainWindow () {
      InitializeComponent ();

      FChassis.UI.Panels.Child.mainWindow = this;
      mainPanel.switchPanel ();
   }
}