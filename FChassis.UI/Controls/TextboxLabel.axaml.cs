using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using FChassis.UI.Controls;

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

}