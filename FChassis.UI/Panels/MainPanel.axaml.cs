using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Panels;
public partial class MainPanel : Child {
 
   public MainPanel () : base() {
      this.mainPanel = this;
      AvaloniaXamlLoader.Load (this);
   }

   internal void switchPanel () {
      this.switchPanel (this);
   }

   void switchPanel (string name) {
      Child panel = name switch {
         "Main Panel"         => this.mainPanel,
         "Model Viewer"       => this.modelViewer,
         "Laser DataBases"    => this.laserSettingPanel,
         "Work Offsets"       => this.workOffsetSettingPanel,
         "Machine Settings"   => this.machineSettingPanel,
      };

      this.switchPanel (panel);
   }

   private void Button_Click (object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Button button = sender as Button;
      var name = button?.Content ?? null;
      this.switchPanel (name as string);
   }

   #region "Field" 
   MainPanel mainPanel;
   ModelViewer modelViewer = new ModelViewer ();

   Settings.Laser.Panel laserSettingPanel = new Settings.Laser.Panel ();
   Settings.WorkOffsets.Panel workOffsetSettingPanel = new Settings.WorkOffsets.Panel ();
   Settings.Machine.Panel machineSettingPanel = new Settings.Machine.Panel ();
   #endregion "Field" 
}