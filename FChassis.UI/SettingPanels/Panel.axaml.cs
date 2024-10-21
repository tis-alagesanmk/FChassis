using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace FChassis.UI.Settings;
internal class ControlInfo {
   internal enum Type {
      None,
      Group,
      Text,
      Combo,
      Check,
      DGrid,
   };

   internal Type type = Type.None;
   internal string label = null!;
   internal object binding = null!;
   internal string unit = null!;
   internal object[] items = null!;
}

internal class GroupControlInfo : ControlInfo {
   internal GroupControlInfo () {
      this.type = Type.Group;
   }
}

internal class _TextControlInfo : ControlInfo {
   internal _TextControlInfo () {
      this.type = Type.Text;
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

   internal ColInfo[] columns = null!;

   internal class ColInfo {
      internal Type type = Type.None;
      internal string header = null!;
   }
}

public partial class Panel : FChassis.UI.Panels.Child {
   internal void AddParameterControls (Grid grid, ControlInfo[] controlInfos) {

      int row = grid.RowDefinitions.Count;
      Binding binding = null!;
      foreach (var ci in controlInfos) {
         grid.RowDefinitions.Add (new RowDefinition {
            Height = new (32)
         });

         Border border = null!;
         Label label = null!;
         TextBlock textBlock = null!;
         TextBox textBox = null!;
         ComboBox comboBox = null!;
         CheckBox checkBox = null!;
         DataGrid dGrid = null!;
         TemplatedControl control = null!;

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

            case ControlInfo.Type.Text:
            case ControlInfo.Type.Combo:
            case ControlInfo.Type.Check:
               label = new Label ();
               label.Content = ci.label;
               setGridRowColumn (label, row, 0, 2);
               label.Classes.Add ("info");
               grid.Children.Add (label);

               if (ci.type == ControlInfo.Type.Text) {
                  control = textBox = new TextBox ();
                  bind (textBox, TextBox.TextProperty, ci.binding);
               } else if (ci.type == ControlInfo.Type.Combo) {
                  control = comboBox = new ComboBox ();
                  bind (comboBox, ComboBox.SelectedItemProperty, ci.binding);
               } else if (ci.type == ControlInfo.Type.Check) {
                  control = checkBox = new CheckBox ();
                  bind (checkBox, CheckBox.ContentProperty, ci.binding);
               }

               grid.Children.Add (control);
               setGridRowColumn (control, row, 2);

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
               dGrid = createDGridColumns (dgi.columns);
               setGridRowColumn (dGrid, row, 0, 4);
               grid.Children.Add (dGrid);
               break;
         }
         row++;
      }

      void setGridRowColumn (Control control, int row, int col, int colSpan = 1) {
         control.SetCurrentValue (Grid.RowProperty, row);
         control.SetCurrentValue (Grid.ColumnProperty, col);
         control.SetCurrentValue (Grid.ColumnSpanProperty, colSpan);
      }

      void bind (AvaloniaObject target, AvaloniaProperty targetProperty, object? property = null) {
         if (property == null)
            return;

         binding = new Binding ();
         binding.Initiate (target, ComboBox.SelectedItemProperty, property);
         target.Bind (targetProperty, binding);
      }

      DataGrid createDGridColumns (DGridControlInfo.ColInfo[] dgcis) {
         DataGrid dGrid = new DataGrid ();
         DataGridColumn column = null!;
         foreach (var dgci in dgcis) {
            switch (dgci.type) {
               case ControlInfo.Type.Text:
                  column = new DataGridTextColumn ();
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
   }
}