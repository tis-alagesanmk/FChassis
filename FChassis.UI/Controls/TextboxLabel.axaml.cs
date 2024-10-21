using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using FChassis.UI.Controls;
using System.Reflection.Emit;

namespace FChassis.UI.Controls;

public class TextboxLabel : TemplatedControl
{

   public static readonly DirectProperty<TextboxLabel, string> ContentProperty = AvaloniaProperty.RegisterDirect<TextboxLabel, string> (nameof (Content), o => o.Content.ToString (), (o, v) => o.Content = v);
   private string _labelcontenth = string.Empty;
   public string Content {
      get => _labelcontenth;
      set => SetAndRaise (ContentProperty, ref _labelcontenth, value);
   }

   public static readonly DirectProperty<TextboxLabel, string> TextProperty = AvaloniaProperty.RegisterDirect<TextboxLabel, string> (nameof (Text), o => o.Text.ToString (), (o, v) => o.Text = v);
   private string _text = string.Empty;
   public string Text {
      get => _text;
      set => SetAndRaise (TextProperty, ref _text, value);
   }

   public static readonly StyledProperty<int> RowProperty = AvaloniaProperty.Register<TextboxLabel, int> (nameof (RowPro), 0);
   public int RowPro {
      get => GetValue (RowProperty);
      set => SetValue (RowProperty, value);
   }

   public static readonly StyledProperty<int>ColumnProperty = AvaloniaProperty.Register<TextboxLabel, int> (nameof (ColumnPro), 0);
   public int ColumnPro {
      get => GetValue (ColumnProperty);
      set => SetValue (ColumnProperty, value);
   }
   protected override void OnPropertyChanged (AvaloniaPropertyChangedEventArgs change) 
   {
     Grid.SetRow(this,RowPro);
     Grid.SetColumn (this, ColumnPro);
      Grid.SetColumnSpan (this, 2);
   }
}