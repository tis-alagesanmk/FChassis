using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Laser.LaserCutting.Cutting {
   public partial class Special : ObservableObject {
      [ObservableProperty, Prop (Prop.Type.DBGrid, null!),
                                  DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                  DBGridColProp (Prop.Type.Text, "Special", "Special")]
      private object[]? specials; 
   }
}
