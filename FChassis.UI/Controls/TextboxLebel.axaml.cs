
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Controls;

public partial class TextboxLebel : UserControl{
   public TextboxLebel () 
   {
      InitializeComponent ();
   }

   public static readonly DirectProperty<TextboxLebel, string> MyTextProperty = AvaloniaProperty.RegisterDirect<TextboxLebel, string> (nameof (LabelContent),label => label.Content.ToString(),(o, v) => o.Content = v);
   public string LabelContent 
   {
      get => GetValue (MyTextProperty);
      set => SetValue (MyTextProperty,value);
   }

   public static readonly StyledProperty<string> TextProprty = AvaloniaProperty.Register<TextboxLebel, string> (nameof (TextBoxContent));

   public string TextBoxContent {
      get => GetValue (TextProprty);
      set => SetValue (TextProprty, value);
   }

   private void InitializeComponent () {
      AvaloniaXamlLoader.Load (this);
   }


}