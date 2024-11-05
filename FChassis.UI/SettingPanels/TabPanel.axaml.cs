using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.Data.Interfaces;
using FChassis.Data.Model;
using FChassis.Data.ViewModel.Settings.Machine.General;
using FChassis.UI.Panels;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using FChassis.Data.ViewModels;
using FChassis.Data.Model.Settings.Machine.General;
using FChassis.Data.JsonDB;
using System.Reflection.PortableExecutable;

namespace FChassis.UI.Settings;
public partial class TabPanel : Panel {
   public TabPanel () {
      AvaloniaXamlLoader.Load (this); }

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
         LocalSettings settings = new LocalSettings (dataContainer);

         foreach (var tabitem in tabControl.Items) {
            this.LoadJson (tabitem as TabItem, settings);
         }
      }
   }

   protected void LoadJson(TabItem tabItem,LocalSettings settings) {
      TabPanel tabPanel = ((TabPanel)(tabItem.Content as Panel));
      if (tabPanel == null) return;
      Panel[] panels = tabPanel.panels;
      foreach(var panel in panels) {
         var context = panel.DataContext;
         if (context != null && context is HMI) {
            var model = (HMI)context;
            settings.data.HMI = model;
         } else if (context != null && context is FChassis.Data.Model.Settings.Machine.General.Machine) {
            var model = (FChassis.Data.Model.Settings.Machine.General.Machine)context;
            settings.data.Machine = model;
         }     
      }

      settings.Save ();
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