using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;


namespace FChassis.Data.Model.Setting.Machine.General;
public partial class HMI : ObservableObject {
   [ObservableProperty] private string? orientation;
   [ObservableProperty] private double? stepSizetoIncrement = 10;
   [ObservableProperty] private int? maximumDaysKeepBackupFiles = 10;
   [ObservableProperty] private int? minimumStoragetoKeepBackupFiles = 10;
   [ObservableProperty] private string? pLCMessagesToDisplay;
   [ObservableProperty] private bool? captionForcommandBarIcons = true;
   [ObservableProperty] private bool? miniPlayer = true;
   [ObservableProperty] private string? language;
   [ObservableProperty] private string? theme;

   [ObservableProperty] private double? width = 10;
   [ObservableProperty] private double? height = 10;
   [ObservableProperty] List<string> orientations = ["Portrait", "Landscape"];
}
