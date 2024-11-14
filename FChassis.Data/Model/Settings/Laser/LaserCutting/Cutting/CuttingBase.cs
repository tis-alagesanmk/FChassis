using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Laser.LaserCutting.Cutting {
   public partial class CuttingBase : ObservableObject {
      [ObservableProperty, Prop (Prop.Type.DBGrid, null!),
                                  DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                  DBGridColProp (Prop.Type.Text, "Start", "Start"),
                                  DBGridColProp (Prop.Type.Text, "Contour", "Contour"),
                                  DBGridColProp (Prop.Type.Text, "End", "End"),
                                  DBGridColProp (Prop.Type.Text, "Corner", "Corner"),
                                  DBGridColProp (Prop.Type.Text, "RampControl", "RampControl")]
      private object[]? dataGrid1;

      [ObservableProperty, Prop (Prop.Type.DBGrid, null!),
                                  DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                  DBGridColProp (Prop.Type.Text, "Common", "Common")]
      private object[]? dataGrid2;
   }

   public partial class Large : CuttingBase { }
   public partial class Medium : CuttingBase { }
   public partial class Small : CuttingBase { }
}
