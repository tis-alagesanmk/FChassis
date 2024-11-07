using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.PLCOptions {
   public partial class PLCKey : ObservableObject{
      [ObservableProperty, Prop (Prop.Type.Text, "Number",  null!, "Name")]
      private string? name;

   }
}
