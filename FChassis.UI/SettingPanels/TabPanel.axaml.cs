using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Panels;
using FChassis.Data.JsonDB;
using System.Collections.Generic;
using FChassis.Data.IO;

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

         FChassis.Data.IO.JSONFileWrite writer = new ();
         this.prepareConfiguraionNodes (writer.nodes);
         writer.Write ("C:/work/config.json");

         DataContainer dataContainer = new DataContainer ();
         LocalJson settings = new LocalJson (dataContainer);

         foreach (var tabitem in tabControl.Items) {
            this.SaveAppData ((tabitem as TabItem)!, settings);
         }
      }
   }

   void prepareConfiguraionNodes(TreeNodes nodes) {
      nodes.Clear ();

      TreeNode treeNode = new () {
         content = "Configuration", };
      nodes.Add(treeNode);

      addConfigurationObjects (treeNode, this);

      void addConfigurationObjects (TreeNode node, TabPanel tabPanel) {
         TabControl tabControl = tabPanel.GetTabControl ();

         TabItem tabItem;
         Panel panel;
         TreeNode childNode = null!;
         foreach (var _tabItem in tabControl.Items) {
            tabItem = (_tabItem as TabItem)!;
            if (tabItem == null)
               continue;

            panel = (tabItem?.Content as Panel)!;
            if (panel == null)
               continue;

            if (panel.DataContext != null || panel is TabPanel) {
               if (panel.DataContext != null && node != null)
                  node.content = panel.DataContext;
               else if (panel is TabPanel) {
                  childNode = new ();
                  node?.children?.Add (childNode);

                  TabPanel? childTabPanel = panel as TabPanel;
                  addConfigurationObjects (childNode, childTabPanel!);
               }

               childNode = null!;
            }
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