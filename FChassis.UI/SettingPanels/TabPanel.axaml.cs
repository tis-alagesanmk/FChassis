using Avalonia.Controls;
using System;

namespace FChassis.UI.Settings;
public partial class TabPanel : Panel {
   protected void PopulateTabItemContent(Panel[] panels) {
      int t = 0;
      Avalonia.Controls.TabControl tabControl = (Avalonia.Controls.TabControl)this.LogicalChildren[0];
      foreach (var panel in panels) {
         Avalonia.Controls.TabItem tabItem = tabControl.Items[t++] as Avalonia.Controls.TabItem;
         tabItem.Content = panel;
      }
   }

   virtual protected void TabItemSelected (TabItem? tabItem, string? tabName) { }

   protected void TabControl_SelectionChanged (object? sender, SelectionChangedEventArgs e) {
      TabItem tabItem = (sender as TabControl).SelectedItem as TabItem;
      this.TabItemSelected (tabItem, tabItem.Header as string);
   }
}