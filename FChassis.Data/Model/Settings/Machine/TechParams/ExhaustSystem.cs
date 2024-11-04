
using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.TechParams {
   public partial class ExhaustSystem : ObservableObject {

      [ObservableProperty, Prop (Prop.Type.DBGrid, "Sections", null!, "BindName"),
                        DBGridColPropInfo (Prop.Type.Text, "Section Number", "BindName1"),
                        DBGridColPropInfo (Prop.Type.Text, "X ON", "BindName2"),
                        DBGridColPropInfo (Prop.Type.Text, "X OFF", "BindName2"),
                        DBGridColPropInfo (Prop.Type.Text, "Y ON", "BindName2"),
                        DBGridColPropInfo (Prop.Type.Text, "Y OFF", "BindName2")]
      private string? sections;

      [ObservableProperty, Prop (Prop.Type.DBGrid, "Splitters", null!, "BindName"),
                       DBGridColPropInfo (Prop.Type.Text, "X ON", "BindName1"),
                       DBGridColPropInfo (Prop.Type.Text, "X OFF", "BindName2"),
                       DBGridColPropInfo (Prop.Type.Text, "Y ON", "BindName2"),
                       DBGridColPropInfo (Prop.Type.Text, "Y OFF", "BindName2")]
      private string? splitters;
   }
}
