using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;


namespace FChassis.Data.Model.Setting.Machine.General;
public partial class HMI : ObservableObject {
   [ObservableProperty] string? orientation = "Portrait";
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
