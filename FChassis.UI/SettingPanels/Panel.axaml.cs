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
   };

   internal Type type = Type.None;
   internal string label = null!;
   internal object binding = null!;
   internal string unit = null!;
   internal object[] items = null!;
}

public partial class Panel : FChassis.UI.Panels.Child {
   internal void AddParameterControls (Grid grid, ControlInfo[] controlInfos) {

      int row = 0;
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
         TemplatedControl control = null!;

         switch (ci.type) {
            case ControlInfo.Type.Group:
               border = new Border ();
               border.SetCurrentValue (Grid.RowProperty, row);
               border.SetCurrentValue (Grid.ColumnProperty, 0);
               border.SetCurrentValue (Grid.ColumnSpanProperty, 5);
               border.Classes.Add ("header");
               grid.Children.Add (border);

               textBlock = new TextBlock ();
               textBlock.Text = ci.label;
               textBlock.Classes.Add ("title");
               textBlock.SetCurrentValue (Grid.ColumnProperty, 0);
               border.Child = textBlock;
               break;

            case ControlInfo.Type.Text:
            case ControlInfo.Type.Combo:
            case ControlInfo.Type.Check:
               label = new Label ();
               label.Content = ci.label;
               setGridRowColumn (label, row, 0);
               label.SetCurrentValue (Grid.ColumnSpanProperty, 2);
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
         }

         row++;
      }

      void setGridRowColumn (TemplatedControl control, int row, int col) {
         control.SetCurrentValue (Grid.RowProperty, row);
         control.SetCurrentValue (Grid.ColumnProperty, col);
      }

      void bind (AvaloniaObject target, AvaloniaProperty targetProperty, object? property = null) {
         if (property == null)
            return;

         binding = new Binding ();
         binding.Initiate (target, ComboBox.SelectedItemProperty, property);
         target.Bind (targetProperty, binding);
      }
   }
}