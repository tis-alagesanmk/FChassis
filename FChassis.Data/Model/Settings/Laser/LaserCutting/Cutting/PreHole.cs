using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Laser.LaserCutting.Cutting {
   public partial class PreHole : ObservableObject {
      [ObservableProperty, Prop (Prop.Type.DBGrid, null!),
                                  DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                  DBGridColProp (Prop.Type.Text, "PreHole", "PreHole")]
      private object[]? specials;
   }
}
