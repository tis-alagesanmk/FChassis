using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Panels;
using FChassis.Data.JsonDB;

namespace FChassis.UI.Settings;
public partial class TabPanel : Panel {
   public TabPanel () {
      AvaloniaXamlLoader.Load (this); 
   }

   virtual protected TabControl GetTabControl () {
      TabControl tabControl = (TabControl)this.LogicalChildren[0];
      return tabControl;
   }

   protected void PopulateTabItemContent (Panel[] panels) {
      this.panels = panels;
      TabControl tabControl = this.GetTabControl ();
      this.PopulateTabItemContent (tabControl, this.panels);
   }

   protected void PopulateTabItemContent (TabControl tabControl, Panel[] panels) {
      if (panels == null)
         return;

      int t = 0;
      foreach (var panel in panels) {
         TabItem? tabItem = tabControl.Items[t++] as TabItem;
         this.LoadJsonData (panel);
         if(tabItem != null) 
            tabItem.Content = panel;
      }
   }

   protected void TabItemSelected_Default (TabItem? tabItem, string? tabName) {
      if (tabName == "Close") {
         // Select first tab
         TabControl tabControl = this.GetTabControl ();
         TabItem? firstTabItem = tabControl.Items[0] as TabItem;
         tabControl.SelectedItem = firstTabItem;

         Child.mainWindow?.Switch2MainPanel ();

         DataContainer dataContainer = new DataContainer ();
         LocalJson settings = new LocalJson (dataContainer);

         foreach (var tabitem in tabControl.Items) {
            this.SaveAppData ((tabitem as TabItem)!, settings);
         }
      }
   }

   protected void SaveAppData(TabItem tabItem, LocalJson json) {
      TabPanel? tabPanel = tabItem.Content as TabPanel;
      if (tabPanel is null) 
         return;

      return;

      Panel[] panels = tabPanel.panels;
      foreach(var panel in panels) {
         var context = panel.DataContext;
         if(context is null) continue;
         json.GetData (context);
      }

      json.Save ();
   } 

   protected void LoadJsonData(Panel panel) {
      if(panel is null) 
         return;

      return;

      DataContainer dataContainer = null!;
      LocalJson json = new LocalJson (dataContainer);

      json.Load ();
      panel.DataContext = json.LoadData(panel.DataContext!);
   }

   virtual protected void TabItemSelected (TabItem? tabItem, string? tabName) { }

   protected void TabControl_SelectionChanged (object? sender, SelectionChangedEventArgs e) {
      TabControl? tabControl = sender as TabControl;
      TabItem? tabItem = tabControl?.SelectedItem as TabItem;
      if(tabItem != null) 
         this.TabItemSelected (tabItem, tabItem.Header as string);
   }

   #region "Fields"
   protected Panel[] panels = null!;
   #endregion 
}