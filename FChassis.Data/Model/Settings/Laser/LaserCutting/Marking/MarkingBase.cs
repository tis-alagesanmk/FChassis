using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Laser.LaserCutting.Marking {
   public partial class MarkingBase : ObservableObject {
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

   public partial class Large : MarkingBase { }
   public partial class Medium : MarkingBase { }
   public partial class Small : MarkingBase { }
}
