using Avalonia;
using Avalonia.Controls.Primitives;

namespace FChassis.UI.Controls;

public class ComboboxLabel : TemplatedControl
{
   public static readonly DirectProperty<ComboboxLabel, string> ContentProperty = AvaloniaProperty.RegisterDirect<ComboboxLabel, string> (nameof (Content), o => o.Content.ToString (), (o, v) => o.Content = v);
   private string _labelcontenth = string.Empty;
   public string Content {
      get => _labelcontenth;
      set => SetAndRaise (ContentProperty, ref _labelcontenth, value);
   }
}