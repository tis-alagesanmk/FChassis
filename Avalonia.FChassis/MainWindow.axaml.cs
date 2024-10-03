using Avalonia.Controls;

namespace Avalonia.Desktop;
public partial class MainWindow : Window {
   public MainWindow () {
      InitializeComponent ();
      this.Content = new SceneViewer ();
   }
}