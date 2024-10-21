using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;

namespace FChassis.UI.Controls;

public class ComboboxLabel : TemplatedControl
{
   public static readonly DirectProperty<ComboboxLabel, string> ContentProperty = AvaloniaProperty.RegisterDirect<ComboboxLabel, string> (nameof (Content), o => o.Content.ToString (), (o, v) => o.Content = v);
   private string _labelcontenth = string.Empty;
   public string Content {
      get => _labelcontenth;
      set => SetAndRaise (ContentProperty, ref _labelcontenth, value);
   }

   public static readonly StyledProperty<int> RowProperty = AvaloniaProperty.Register<TextboxLabel, int> (nameof (RowPro), 0);
   public int RowPro {
      get => GetValue (RowProperty);
      set => SetValue (RowProperty, value);
   }

   public static readonly StyledProperty<int> ColumnProperty = AvaloniaProperty.Register<TextboxLabel, int> (nameof (ColumnPro), 0);
   public int ColumnPro {
      get => GetValue (ColumnProperty);
      set => SetValue (ColumnProperty, value);
   }
   protected override void OnPropertyChanged (AvaloniaPropertyChangedEventArgs change) 
   {
      Grid.SetRow (this, RowPro);
      Grid.SetColumn (this, ColumnPro);
   }
}