using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis;
using System;

namespace Avalonia.Desktop;
public partial class MainPanel : MainChildPanel {
 
   public MainPanel () : base() {
      this.mainPanel = this;
      AvaloniaXamlLoader.Load (this);
   }

   internal void switchPanel () {
      this.switchPanel (this);
   }

   void switchPanel (string name) {
      MainChildPanel panel = name switch {
         "Main Panel"          => this.mainPanel,
         "Model Viewer"        => this.modelViewer,
         "Laser DataBases"      => this.laserSettingPanel,
         "Work Offsets"  => this.workOffsetSettingPanel,
         "Machine Settings"     => this.machineSettingPanel,
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

   LaserSettingPanel laserSettingPanel = new LaserSettingPanel ();
   WorkOffsetSettingPanel workOffsetSettingPanel = new WorkOffsetSettingPanel ();
   MachineSettingPanel machineSettingPanel = new MachineSettingPanel ();
   #endregion "Field" 
}