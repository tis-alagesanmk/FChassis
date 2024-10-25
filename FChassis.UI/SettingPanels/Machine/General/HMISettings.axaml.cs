using FChassis.Data.ViewModels.Setting.Machine.General;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      this.DataContext = new HMIViewModel (); 

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid!, [
            new GroupControlInfo ("General"),
            new ComboControlInfo ("Orientation", "Orientation", "Orientations"),
            new _TextControlInfo ("Step size to increment", "StepSizetoIncrement"),
            new _TextControlInfo ("Maximum days keep back up files", "MaximumDaysKeepBackupFiles"),
            new _TextControlInfo ("Minimum storage to keep back up files", "MinimumStoragetoKeepBackupFiles", "GB"),                                  
            new ComboControlInfo ("PLC messages to display", "PlcMessagesToDisplay", "PlcMessages"),
            new CheckControlInfo ("Caption for command-bar icons", "CaptionForcommandBarIcons"),
            new CheckControlInfo ("Mini player", "MiniPlayer"),
            new ComboControlInfo ("Language", "Language", "Languages"),
            new ComboControlInfo ("Theme", "Theme", "Themes"),

            new GroupControlInfo ("Screen size"),
            new _TextControlInfo ("Width", "Width"),
            new _TextControlInfo ("Height", "Height"),
      ]);
   }
}