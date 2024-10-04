using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using System;

namespace Avalonia.Desktop;
public partial class MainWindow : Window {
   public MainWindow () 
   {
      InitializeComponent ();
      this.InitializePanel ();
   }

   /// <summary>
   /// Initialize main panel and raise event
   /// </summary>
   private void InitializePanel() {

      MainPanel mainPanel = new MainPanel ();

      mainPanel.ButtonRaised += OnPressButton;
      this.maincontent.Content = mainPanel;
   }

   private void SwitchViewContent(UserControl control) {
      this.maincontent.Content = control;
   }
   private void OnPressButton (object sender, EventArgs e) {
      string name = sender as string;

      switch (name) {
         case "1":
            SceneViewer sceneViewer = new SceneViewer ();
            this.SwitchViewContent (sceneViewer);
            break;
         case "2":
            MachineSettingPanel machineSettingPanel = new MachineSettingPanel ();
            this.SwitchViewContent (machineSettingPanel);
            break;
         default:
            this.SwitchViewContent (null);
            break;
      }
   }

   }