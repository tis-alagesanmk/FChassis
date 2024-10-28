using FChassis.Data.Model;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FChassis.UI.Settings;
public partial class Panel : Panels.Child {
   internal void AddPropControls (Grid grid, Type type, Type colType = null!) {
      int row = grid.RowDefinitions.Count;

      var fields = type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo f in fields) {
         Prop p = f.GetCustomAttribute<Prop> ()!;
         if (p == null) continue;

         Border border = null!;
         TextBlock textBlock = null!;
         if (p.groupName != null) {
            border = new Border ();
            grid.RowDefinitions.Add (new RowDefinition { Height = new (32) });
            setGridRowColumn (border, row++, 0, 5);
            border.Classes.Add ("header");
            grid.Children.Add (border);

            textBlock = new TextBlock ();
            textBlock.Text = p.groupName;
            textBlock.Classes.Add ("title");
            border.Child = textBlock;
         }

         grid.RowDefinitions.Add (new RowDefinition { Height = new (32) });

         Label label = null!;
         DataGrid dGrid = null!;
         Control control = null!;
         switch (p.type) {
            case Prop.Type.Button:
               p.control = control = new Button () { Content = p.label };
               setGridRowColumn (control, row, 2);
               break;

            case Prop.Type.Text:
            case Prop.Type.Combo:
            case Prop.Type.Check:
               if (p.type != Prop.Type.Check) {
                  label = new Label ();
                  label.Content = p.label;
                  setGridRowColumn (label, row, 0, 2);
                  label.Classes.Add ("info");
                  grid.Children.Add (label);
               }

               p.control = control = p.type switch {
                  Prop.Type.Text => new TextBox (),
                  Prop.Type.Combo => new ComboBox (),
                  Prop.Type.Check => new CheckBox () { Content = p.label },
                  _ => null!
               };

               setGridRowColumn (control, row, 2);

               if (p.unit != null) {
                  label = new Label ();
                  label.Content = p.unit;
                  label.Classes.Add ("blue");
                  setGridRowColumn (label, row, 3);
                  grid.Children.Add (label);
               }
               break;

            case Prop.Type.DGrid:
               p.control = control = dGrid = createDGridColumns (colType);//createDGridColumns (p.columns, p.collections);
               dGrid.Bind (DataGrid.ItemsSourceProperty, new Binding (p.itemsName));
               grid.RowDefinitions[row].Height = new GridLength (1, GridUnitType.Auto);
               setGridRowColumnDataGrid (dGrid, row);
               break;
         }

         if (control != null) {
            List<Prop.BindInfo> bis = p.bindInfos!;
            if (bis != null)
               bind (control, bis!);

            switch (p.type) {
               case Prop.Type.Text:
                  (control as TextBox)!.Bind (TextBox.TextProperty, new Binding (CapitalizeFirstLetter (f.Name)));
                  break;

               case Prop.Type.Check:
                  (control as CheckBox)!.Bind (CheckBox.IsCheckedProperty, new Binding (CapitalizeFirstLetter (f.Name)));
                  break;

               case Prop.Type.Combo:
                  (control as ComboBox)!.Bind (ComboBox.SelectedItemProperty, new Binding (CapitalizeFirstLetter (f.Name)));
                  if (p.items != null)
                     (control as ComboBox)!.ItemsSource = p.items;
                  else
                     (control as ComboBox)!.Bind (ComboBox.ItemsSourceProperty, new Binding (CapitalizeFirstLetter (p.itemsName)));
                  break;
            }

            grid.Children.Add (control);
         }

         row++;
      }

      void setGridRowColumn (Control control, int row, int col, int colSpan = 1) {
         control.SetCurrentValue (Grid.RowProperty, row);
         control.SetCurrentValue (Grid.ColumnProperty, col);
         control.SetCurrentValue (Grid.ColumnSpanProperty, colSpan);
      }

      void setGridRowColumnDataGrid (Control control, int row)
         => control.SetCurrentValue (Grid.RowProperty, row);
      
      string CapitalizeFirstLetter (string str) {
         if (string.IsNullOrEmpty (str)) {
            return str; // Return original string if it's null or empty
         }

         // Convert the first character to uppercase and concatenate with the rest of the string
         return char.ToUpper (str[0]) + str.Substring (1);
      }
      #region Local function
      void bind (Control control, List<Prop.BindInfo> bindInfos) {
         foreach (Prop.BindInfo bi in bindInfos) {
            if (bi == null) continue;
            control.Bind ((AvaloniaProperty)bi.property, new Binding (bi.name));
         }
      }

      DataGrid createDGridColumns (Type type) {
         DataGrid dGrid = new DataGrid ();
         //dGrid.ItemsSource = collections;
         DataGridColumn column = null!;

         var fields = type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
         foreach (var f in fields) {
            Prop? p = f.GetCustomAttribute<Prop> ();
            switch (p.type) {
               case Prop.Type.Text:
                  column = new DataGridTextColumn ();
                  ((DataGridTextColumn)column).Binding = new Binding (p.dGridPath);
                  break;

               case Prop.Type.Check:
                  column = new DataGridCheckBoxColumn ();
                  break;
            }

            if (column == null)
               continue;

            column.Header = p.label;
            dGrid.Columns.Add (column);
            column = null!;
         }
         return dGrid;
      }
      #endregion Local function
   }

   internal void AddParameterControls (Grid grid, ControlInfo[] controlInfos) {
      int row = grid.RowDefinitions.Count;
      foreach (var ci in controlInfos) {
         grid.RowDefinitions.Add (new RowDefinition { Height = new (32) });

         Border border = null!;
         Label label = null!;
         TextBlock textBlock = null!;
         DataGrid dGrid = null!;
         int col, colSpan;

         switch (ci.type) {
            case ControlInfo.Type.Group:
               border = new Border ();
               setGridRowColumn (border, row, 0, 5);
               border.Classes.Add ("header");
               grid.Children.Add (border);

               textBlock = new TextBlock ();
               textBlock.Text = ci.label;
               textBlock.Classes.Add ("title");
               border.Child = textBlock;
               break;

            case ControlInfo.Type.Button:
               ci.control = new Button () { Content = ci.label };
               setGridRowColumn (ci.control, row, 2);
               break;

            case ControlInfo.Type.Text_:
            case ControlInfo.Type.Combo:
            case ControlInfo.Type.Check:
               col = 0; colSpan = 2;
               if (ci.type == ControlInfo.Type.Check) {
                  col = 3; colSpan = 1;
               } // Back Label for Check, otherwise Front Label

               if (ci.type != ControlInfo.Type.Check) {
                  label = new Label ();
                  label.Content = ci.label;
                  setGridRowColumn (label, row, col, colSpan);
                  label.Classes.Add ("info");
                  grid.Children.Add (label);
               }

               ci.control = ci.type switch {
                  ControlInfo.Type.Text_ => new TextBox (),
                  ControlInfo.Type.Combo => new ComboBox (),
                  ControlInfo.Type.Check => new CheckBox () { Content = ci.label},
                  _ => null!
               };

               setGridRowColumn (ci.control, row, 2);

               if (ci.unit != null) {
                  label = new Label ();
                  label.Content = ci.unit;
                  label.Classes.Add ("blue");
                  setGridRowColumn (label, row, 3);
                  grid.Children.Add (label);
               }
               break;

            case ControlInfo.Type.DGrid:
               DGridControlInfo dgi = (DGridControlInfo)ci;
               ci.control = dGrid = createDGridColumns (dgi.columns, dgi.collections);
               grid.RowDefinitions[row].Height = new GridLength (1, GridUnitType.Auto);
               setGridRowColumnDataGrid (dGrid, row);
               break;
         }

         if (ci.control != null) {
            List<ControlInfo.BindInfo> bis = ci.bindInfos!;
            if (bis != null)
               bind (ci.control, bis!);

            grid.Children.Add (ci.control);
         }

         row++;
      }

      void setGridRowColumn (Control control, int row, int col, int colSpan = 1) {
         control.SetCurrentValue (Grid.RowProperty, row);
         control.SetCurrentValue (Grid.ColumnProperty, col);
         control.SetCurrentValue (Grid.ColumnSpanProperty, colSpan);
      }

      void setGridRowColumnDataGrid (Control control, int row)
         => control.SetCurrentValue (Grid.RowProperty, row);

      #region Local function
      void bind (Control control, List<ControlInfo.BindInfo> bindInfos) {
         foreach (ControlInfo.BindInfo bi in bindInfos) {
            if (bi == null) continue;
            control.Bind (bi.property, bi.binding); }
         }

         DataGrid createDGridColumns (DGridControlInfo.ColInfo[] dgcis, IEnumerable collections) {
         DataGrid dGrid = new DataGrid ();
         dGrid.ItemsSource = collections;
         DataGridColumn column = null!;
         foreach (var dgci in dgcis) {
            switch (dgci.type) {
               case ControlInfo.Type.Text_:
                  column = new DataGridTextColumn ();
                  ((DataGridTextColumn)column).Binding = new Binding (dgci.path);
                  break;
               case ControlInfo.Type.Check:
                  column = new DataGridCheckBoxColumn ();
                  break;
            }

            if (column == null)
               continue;

            column.Header = dgci.header;
            dGrid.Columns.Add (column);
            column = null!;
         }

         return dGrid;
      }
      #endregion Local function
   }
}

#region Run Time ControlInfo 
internal class ControlInfo (ControlInfo.Type type, string label, string unit = null!) {
   static internal BindInfo Bind (string name, AvaloniaProperty property)
      => new BindInfo (name, property);

   // -------------------------------------------------------------------------
   #region Types enum
   internal enum Type {
      None,
      Group,
      Text_,
      Combo,
      Check,
      Button,
      DGrid,
   };
   #endregion Types enum

   internal Type type = type;
   internal string label = label;
   internal string unit = unit;
   internal object[] items = null!;

   internal Control control = null!;
   internal object binding = null!;
   internal List<BindInfo> bindInfos = new();

   #region Inner Class --------------------------------------------------------
   internal class BindInfo(string name, AvaloniaProperty property) {
      internal AvaloniaProperty property = property;
      internal Binding binding = new Binding (name);
   }
   #endregion Inner Class
}

#region Specialized ControlInfo classes ---------------------------------------
internal class GroupControlInfo : ControlInfo {
   internal GroupControlInfo (string label, string unit = null!)
      : base (Type.Group, label, unit) { }
}

internal class _TextControlInfo : ControlInfo {
   internal _TextControlInfo (string label, string bindName, string unitName = null!)
      : base (Type.Text_, label, unitName) {
      this.bindInfos = [Bind (bindName, TextBox.TextProperty)]; }
}

internal class ComboControlInfo : ControlInfo {
   internal ComboControlInfo (string label, string bindName, string itemsName, string unitName = null!)
      : base (Type.Combo, label, unitName) {
      this.bindInfos.Add(Bind (bindName, ComboBox.SelectedItemProperty));
      if (itemsName != null)
         this.bindInfos.Add (Bind (itemsName, ComboBox.ItemsSourceProperty));
   }
}

internal class CheckControlInfo : ControlInfo {
   internal CheckControlInfo (string label, string bindName)
      : base (Type.Check, label) {
         this.bindInfos = [Bind (bindName, CheckBox.IsCheckedProperty)]; }
}

internal class ButtonControlInfo : ControlInfo {
   internal ButtonControlInfo (string label, string bindName)
      : base (Type.Button, label) {
         this.bindInfos = [Bind (bindName, Avalonia.Controls.Button.CommandProperty)]; }
}

internal class DGridControlInfo : ControlInfo {
   internal DGridControlInfo (string label = null!)
      : base (Type.DGrid, label) { }

   internal IEnumerable collections { get; set; } = null!;
   internal ColInfo[] columns = null!;

   internal class ColInfo {
      internal Type type = Type.None;
      internal string header = null!;
      internal string path = null!;
   }
}
#endregion Specialized ControlInfo classes
#endregion  Run Time ControlInfo 