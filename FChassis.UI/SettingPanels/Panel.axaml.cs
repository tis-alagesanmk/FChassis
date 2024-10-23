using Avalonia;

using Avalonia.Controls;

using Avalonia.Data;

using System.Collections;

namespace FChassis.UI.Settings;

public partial class Panel : Panels.Child {

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

            case ControlInfo.Type.Text_:

            case ControlInfo.Type.Combo:

            case ControlInfo.Type.Check:

               col = 0; colSpan = 2;

               if (ci.type == ControlInfo.Type.Check) {

                  col = 3; colSpan = 1;
               } // Back Label for Check, otherwise Front Label

               label = new Label ();

               label.Content = ci.label;

               setGridRowColumn (label, row, col, colSpan);

               label.Classes.Add ("info");

               grid.Children.Add (label);

               ci.control = ci.type switch {

                  ControlInfo.Type.Text_ => new TextBox (),

                  ControlInfo.Type.Combo => new ComboBox (),

                  ControlInfo.Type.Check => new CheckBox (),

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

               grid.RowDefinitions[row].Height = new GridLength (1, GridUnitType.Star);

               setGridRowColumnDataGrid (dGrid, row);

               break;

         }

         if (ci.control != null) {

            ControlInfo.BindInfo[] bis = ci.bindInfos!;

            if (bis == null)

               bis = ci.bindInfo != null ? [ci.bindInfo] : null!;

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

      void bind (Control control, ControlInfo.BindInfo[] bindInfos) {

         foreach (ControlInfo.BindInfo bi in bindInfos)

            control.Bind (bi.property, bi.binding);

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

internal class ControlInfo {

   internal enum Type {

      None,

      Group,

      Text_,

      Combo,

      Check,

      DGrid,

   };

   internal Type type = Type.None;

   internal string label = null!;

   internal string unit = null!;

   internal object[] items = null!;

   internal Control control = null!;

   internal object binding = null!;

   internal BindInfo bindInfo = null!;

   internal BindInfo[] bindInfos = null!;

   #region Inner Class

   internal class BindInfo {

      internal AvaloniaProperty property = null!;

      internal Binding binding = null!;

   }

   internal static class Text {

      internal static BindInfo Binding (string name) {

         return new BindInfo {

            property = TextBox.TextProperty,

            binding = new Binding (name), };

      }

   }

   internal static class Combo {

      internal static BindInfo Binding (string name) {

         return new BindInfo {

            property = ComboBox.SelectedItemProperty,

            binding = new Binding (name), };

      }

   }

   internal static class Check {

      internal static BindInfo Binding (string name) {

         return new BindInfo {

            property = CheckBox.IsCheckedProperty,

            binding = new Binding (name), };

      }
      internal static BindInfo BindingCommand (string name) {

         return new BindInfo {

            property = CheckBox.CommandProperty,

            binding = new Binding (name), };

      }

   }

   #endregion Inner Class

}

#region Specialized ControlInfo classes

internal class GroupControlInfo : ControlInfo {

   internal GroupControlInfo () {

      this.type = Type.Group;

   }

}

internal class _TextControlInfo : ControlInfo {

   internal _TextControlInfo () {

      this.type = Type.Text_;

   }

}

internal class ComboControlInfo : ControlInfo {

   internal ComboControlInfo () {

      this.type = Type.Combo;

   }

}

internal class CheckControlInfo : ControlInfo {

   internal CheckControlInfo () {

      this.type = Type.Check;

   }

}

internal class DGridControlInfo : ControlInfo {

   internal DGridControlInfo () {

      this.type = Type.DGrid;

   }

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
