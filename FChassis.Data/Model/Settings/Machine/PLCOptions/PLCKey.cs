using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.PLCOptions; 
public partial class PLCKey : ObservableObject {

   [ObservableProperty, Prop ("PLC Key1", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys1;

   [ObservableProperty, Prop ("PLC Key2", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys2;

   [ObservableProperty, Prop ("PLC Key3", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys3;

   [ObservableProperty, Prop ("PLC Key4", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys4;

   [ObservableProperty, Prop ("PLC Key5", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys5;

   [ObservableProperty, Prop ("PLC Key6", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys6;

   [ObservableProperty, Prop ("PLC Key7", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys7;

   [ObservableProperty, Prop ("PLC Key8", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys8;

   [ObservableProperty, Prop ("PLC Key9", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys9;

   [ObservableProperty, Prop ("PLC Key10", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys10;

   [ObservableProperty, Prop ("PLC Key11", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys11;

   [ObservableProperty, Prop ("PLC Key12", Prop.Type.DBGrid, "", null!, "BindName"),
                        DBGridColProp (Prop.Type.Text, "Name", "BindName1"),
                        DBGridColProp (Prop.Type.Text, "Type", "BindName2"),
                        DBGridColProp (Prop.Type.Text, "Function", "BindName2")]
   private object[]? plcKeys12;
}
