using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Panels;
using FChassis.Data.JsonDB;
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
      TabControl tabControl = this.GetTabControl ();
      this.PopulateTabItemContent (tabControl, panels);
   }

   protected void PopulateTabItemContent (TabControl tabControl, Panel[] panels) {
      if (panels == null)
         return;

      int t = 0;
      foreach (var panel in panels) {
         TabItem? tabItem = tabControl.Items[t++] as TabItem;
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
         this.AddConfiguraionNodes (writer.node);
         writer.Write ("C:/work/config.json");
      }
   }

   virtual protected void TabItemSelected (TabItem? tabItem, string? tabName) { }

   protected void TabControl_SelectionChanged (object? sender, SelectionChangedEventArgs e) {
      TabControl? tabControl = sender as TabControl;
      TabItem? tabItem = tabControl?.SelectedItem as TabItem;
      if(tabItem != null) 
         this.TabItemSelected (tabItem, tabItem.Header as string);
   }

   protected void AddConfiguraionNodes (TreeNode node) {
      node?.children?.Clear ();
      node!.content = "Configuration";
      _addConfigurationObjects (node!, this);

      #region Local function
      void _addConfigurationObjects (TreeNode node, TabPanel tabPanel) {
         TabControl tabControl = tabPanel.GetTabControl ();

         Panel panel;
         TabItem tabItem;
         TreeNode childNode = null!;
         foreach (var _tabItem in tabControl.Items) {
            tabItem = (_tabItem as TabItem)!;
            if (tabItem == null)
               continue;

            panel = (tabItem?.Content as Panel)!;
            if (panel == null)
               continue;

            if (panel.DataContext != null || panel is TabPanel) {
               if (panel.DataContext != null)
                  childNode = new () { content = panel.DataContext };
               else if (panel is TabPanel) {
                  childNode = new () { content = tabItem?.Header };
                  _addConfigurationObjects (childNode, (panel as TabPanel)!);
               }

               node?.children?.Add (childNode);
            }
         }
      }
      #endregion Local function
   }

   #region "Fields"
   #endregion 
}