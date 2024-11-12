using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.ProcessingDefaults {
   public partial class CutCam : ObservableObject {
      [ObservableProperty, Prop (Prop.Type.Check, "Advanced")]
      bool? advanced = true;

      [ObservableProperty, Prop ("Finishing Rules", 
                                 Prop.Type.DBGrid, null!, null!, "BindName"),
                                       DBGridColProp (Prop.Type.Text, "Action", "BindName1"),
                                       DBGridColProp (Prop.Type.Text, "Contour Flags", "BindName2"),
                                       DBGridColProp (Prop.Type.Text, "Disabled", "BindName2"),
                                       DBGridColProp (Prop.Type.Text, "Size", "BindName2")]
      private object[]? finishingRules;

      [ObservableProperty, Prop ("Microjoint settings", 
                                 Prop.Type.Text, "Microjoint length")]
      double? microjointSettings = 12.23;
   }
}
