using Avalonia.Controls;
using System;

namespace Avalonia.Desktop;
public partial class MainWindow : Window {
   MainPanel mainPanel = new ();
   public MainWindow () {
      InitializeComponent ();

      MainChildPanel.mainWindow = this;
      mainPanel.switchPanel ();
   }
}