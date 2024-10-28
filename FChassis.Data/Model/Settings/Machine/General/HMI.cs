using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.General;
public partial class HMI : ObservableObject {
   [ObservableProperty, Prop (Prop.Type.Combo, "Orientation", null!, null!, "orientations")]
   string? orientation = "Portrait";
   [ObservableProperty, Prop (Prop.Type.Text, "Step size to increment")]
   double? stepSizetoIncrement = 10;

   [ObservableProperty, Prop (Prop.Type.Text, "Maximum days keep back up files")]
   int? maximumDaysKeepBackupFiles = 10;

   [ObservableProperty, Prop (Prop.Type.Text, "Minimum storage to keep back up files")]
   int? minimumStoragetoKeepBackupFiles = 10;

   [ObservableProperty, Prop (Prop.Type.Combo, "PLC messages to display", null!, ["Only error", "Warn & error", "info, warn & error"])]
   string? plcMessagesToDisplay = "Only error";

   [ObservableProperty, Prop (Prop.Type.Check, "Caption for command-bar icons")]
   bool? captionForcommandBarIcons = true;

   [ObservableProperty, Prop (Prop.Type.Check, "Mini player")]
   bool? miniPlayer = true;

   [ObservableProperty, Prop (Prop.Type.Combo, "Language", null!, null!, "languages")]
   string? language = "EN";

   [ObservableProperty, Prop (Prop.Type.Combo, "Theme", null!, null!, "themes")]
   string? theme = "Grey";

   [ObservableProperty, Prop ("Screen size", Prop.Type.Text, "Width")]
   double? width = 10;

   [ObservableProperty, Prop (Prop.Type.Text, "Height")]
   double? height = 10;
}