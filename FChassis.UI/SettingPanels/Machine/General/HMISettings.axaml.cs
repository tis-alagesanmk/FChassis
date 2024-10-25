using FChassis.Data.ViewModels.Setting.Machine.General;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      HMIViewModel vm = new HMIViewModel ();
      this.DataContext = vm;

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid!, [
            new GroupControlInfo ("General"),
            new ComboControlInfo ("Orientation", "Orientation", "Orientations"),
            new _TextControlInfo ("Step size to increment", "vm.StepSizetoIncrement"),
            new _TextControlInfo ("Maximum days keep back up files", "MaximumDaysKeepBackupFiles"),
            new _TextControlInfo ("Minimum storage to keep back up files", "MinimumStoragetoKeepBackupFiles", "GB"),                                  
            new ComboControlInfo ("PLC messages to display", "PLCMessagesToDisplay", "plcMessages"),
            new CheckControlInfo ("Caption for command-bar icons", "CaptionForcommandBarIcons"),
            new CheckControlInfo ("Mini player", "miniPlayer"),
            new ComboControlInfo ("Language", "Language", "language"),
            new ComboControlInfo ("Theme", "Theme", "themes"),

            new GroupControlInfo ("Screen size"),
            new _TextControlInfo ("Width", "Width"),
            new _TextControlInfo ("Height", "Height"),
      ]);
   }
}