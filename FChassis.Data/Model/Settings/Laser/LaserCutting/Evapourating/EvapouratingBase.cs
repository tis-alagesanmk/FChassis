using CommunityToolkit.Mvvm.ComponentModel;
using FChassis.Data.Model.Settings.Laser.LaserCutting.Marking;

namespace FChassis.Data.Model.Settings.Laser.LaserCutting.Evapourating {
   public partial class EvapouratingBase : ObservableObject {
      [ObservableProperty, Prop (Prop.Type.DBGrid, null!),
                                  DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                  DBGridColProp (Prop.Type.Text, "Contour", "Contour"),
                                  DBGridColProp (Prop.Type.Text, "RampControl", "RampControl")]
      private object[]? dataGrid1;

      [ObservableProperty, Prop (Prop.Type.DBGrid, null!),
                                  DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                  DBGridColProp (Prop.Type.Text, "Common", "Common")]
      private object[]? dataGrid2;
   }
   public partial class Large : EvapouratingBase { }
   public partial class Medium : EvapouratingBase { }
   public partial class Small : EvapouratingBase { }
}
