using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.General;
[System.AttributeUsage (System.AttributeTargets.Field,
                        AllowMultiple = true)  /* Multiuse attribute*/]
public class Prop(string label, string unit = null!) : System.Attribute {
   internal string? label = label;
   public string? unit = unit;
}

public partial class HMI : ObservableObject {
   [ObservableProperty, Prop("Machine Id", "s")] 
   string? orientation = "Portrait";


   [ObservableProperty] double? stepSizetoIncrement = 10;
   [ObservableProperty] int? maximumDaysKeepBackupFiles = 10;
   [ObservableProperty] int? minimumStoragetoKeepBackupFiles = 10;
   [ObservableProperty] string? plcMessagesToDisplay = "Only error";
   [ObservableProperty] bool? captionForcommandBarIcons = true;
   [ObservableProperty] bool? miniPlayer = true;
   [ObservableProperty] string? language = "EN";
   [ObservableProperty] string? theme = "Grey";

   [ObservableProperty] double? width = 10;
   [ObservableProperty] double? height = 10;
}
