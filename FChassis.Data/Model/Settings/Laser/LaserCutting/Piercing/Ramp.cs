using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Laser.LaserCutting.Piercing {
   public partial class Ramp : ObservableObject {
      [ObservableProperty, Prop (Prop.Type.DBGrid, null!),
                                  DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                  DBGridColProp (Prop.Type.Text, "Power", "Power"),
                                  DBGridColProp (Prop.Type.Text, "Nozzle_Gap", "NozzleGap"),
                                  DBGridColProp (Prop.Type.Text, "RampValue", "RampValue")]
      private object[]? dataGrid1;
   }
}
