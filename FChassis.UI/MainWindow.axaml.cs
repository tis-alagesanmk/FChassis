using Avalonia.Controls;
using FChassis.Data.JsonDB;
using FChassis.Data.Model.Settings.Machine.General;
using FChassis.Data.ViewModels;

namespace FChassis.UI;
public partial class MainWindow : Window {
   FChassis.UI.Panels.MainPanel mainPanel = new ();
   public MainWindow () {
      InitializeComponent ();

      this.LoadJSON ();

      FChassis.UI.Panels.Child.mainWindow = this;
      this.mainPanel.switchPanel ();
   }

   internal void Switch2MainPanel () {
      this.mainPanel.switchPanel ();
   }
   internal void LoadJSON() {

      DataContainer dataContainer = null;
      LocalSettings settings = new LocalSettings (dataContainer);

      dataContainer = settings.Load ();
   }
}