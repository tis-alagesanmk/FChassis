using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.TechParams {
   public partial class MachineDataBase : ObservableObject{

      [ObservableProperty,Prop ("General", Prop.Type.Check,"Varimode")]
      bool? varimode = true;

      [ObservableProperty, Prop ("Database type", Prop.Type.Combo, "Database", null!, null!, ["Default"])]
      string? databaseType = "Default";
   }
}
