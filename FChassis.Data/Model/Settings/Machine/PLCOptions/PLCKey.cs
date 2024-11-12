using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.PLCOptions; 
public partial class PLCKey : ObservableObject {
   [ObservableProperty, Prop ("PLCKey1",
                             Prop.Type.DBGrid, null!),
                                   DBGridColProp (Prop.Type.Text, "Name", "Name"),
                                   DBGridColProp (Prop.Type.Text, "Type", "Type"),
                                   DBGridColProp (Prop.Type.Text, "Function", "Function")]
   private object[]? plcKey1s;
}
