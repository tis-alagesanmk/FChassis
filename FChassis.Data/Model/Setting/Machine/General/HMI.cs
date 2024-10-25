using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.General;
public partial class HMI : ObservableObject {
   [ObservableProperty, Prop ("General", Prop.Type.Combo, "Orientation", null!, "orientations")]
   string? orientation = "Portrait";
   [ObservableProperty, Prop (Prop.Type.Text, "Step size to increment")]
   double? stepSizetoIncrement = 10;

   [ObservableProperty, Prop (Prop.Type.Text, "Maximum days keep back up files")]
   int? maximumDaysKeepBackupFiles = 10;

   [ObservableProperty, Prop (Prop.Type.Text, "Minimum storage to keep back up files")] 
   int? minimumStoragetoKeepBackupFiles = 10;

   [ObservableProperty, Prop (Prop.Type.Combo, "PLC messages to display", null!, "plcMessages")] 
   string? plcMessagesToDisplay = "Only error";

   [ObservableProperty, Prop (Prop.Type.Check, "Caption for command-bar icons")]
   bool? captionForcommandBarIcons = true;

   [ObservableProperty] bool? miniPlayer = true;
   [ObservableProperty] string? language = "EN";
   [ObservableProperty] string? theme = "Grey";

   [ObservableProperty] double? width = 10;
   [ObservableProperty] double? height = 10;
}