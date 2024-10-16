using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using FChassis.UI.Controls;

namespace FChassis.UI.Controls;

public class TextboxLabel : TemplatedControl
{
   //public static readonly DirectProperty<TemplatedControl1, string> LabelContentProperty = AvaloniaProperty.RegisterDirect<TemplatedControl1, string> (nameof (LabelContent), o => o.LabelContent.ToString (), (o, v) => o.LabelContent = v);
   //private string _labelcontent = "Default Value";
   //public string LabelContent {
   //   get => _labelcontent;
   //   set => SetAndRaise (LabelContentProperty,ref _labelcontent, value);
   //}

   public static readonly DirectProperty<TextboxLabel, string> LabelContenthProperty = AvaloniaProperty.RegisterDirect<TextboxLabel, string> (nameof (LabelContenth), o => o.LabelContenth.ToString (), (o, v) => o.LabelContenth = v);
   private string _labelcontenth = "Default Value";
   public string LabelContenth {
      get => _labelcontenth;
      set => SetAndRaise (LabelContenthProperty, ref _labelcontenth, value);
   }

   public static readonly DirectProperty<TextboxLabel, string> LabelContenth1Property = AvaloniaProperty.RegisterDirect<TextboxLabel, string> (nameof (LabelContenth1), o => o.LabelContenth1.ToString (), (o, v) => o.LabelContenth1 = v);
   private string _labelcontenth1 = "Default Value";
   public string LabelContenth1 {
      get => _labelcontenth1;
      set => SetAndRaise (LabelContenth1Property, ref _labelcontenth1, value);
   }

}